using System;
using Audio;
using ManagerSystem.Base;
using UnityEngine;
using UnityEngine.Audio;


public enum EAudioType
{
    Master = 0,
    BGM,
    SFX
}

public enum EAudioSituation
{
    Character_Jump = 0,
    Character_Land,
    Character_Damaged,
    Character_Died,
    Collect_Ingredient,
    Success_Submit,
    Fail_Submit,
    Break_Obstacle,
    Game_Over,
    System_Error,
    System_Notice,
    System_Alert,
}

namespace ManagerSystem
{
    public class AudioManager : BaseManager
    {
        private AudioMixer audioMixer;

        private AudioEmitter systemEmitter;
        private AudioEmitter bgmEmitter;

        private bool[] isMute = new bool[3];
        private float[] audioVolumes = new float[3];

        public bool IsInitialized { get; private set; } = false;

        public override void Initialize(params object[] args)
        {
            base.Initialize(args);

            audioMixer = Managers.Resource.GetResource<AudioMixer>("Audio/AudioMixer");

            if (audioMixer == null)
            {
                Debug.LogError("AudioMixer not found");
                return;
            }

            isMute = new bool[Enum.GetValues(typeof(EAudioType)).Length];
            audioVolumes = new float[Enum.GetValues(typeof(EAudioType)).Length];

            IsInitialized = true;
        }

        public void SetEmitterInScene(AudioEmitter bgmEmitter, AudioEmitter systemSFXEmitter)
        {
            if (bgmEmitter == null || systemSFXEmitter == null)
            {
                Debug.LogError("AudioEmitter is not initialized");
                return;
            }

            this.bgmEmitter = bgmEmitter;
            this.systemEmitter = systemSFXEmitter;

            // AudioEmitter의 오디오 타입 설정
            this.bgmEmitter.SetAudioType(EAudioType.BGM);
            this.systemEmitter.SetAudioType(EAudioType.SFX);
        }

        public void SetAudioVolume(EAudioType audioType, float volume)
        {
            if (audioMixer == null)
            {
                Debug.LogError("AudioMixer is not initialized");
                return;
            }

            string parameterName = audioType.ToString();
            // 오디오 믹서의 값은 -80 ~ 0까지이기 때문에 0.0001 ~ 1의 Log10 * 20을 한다.
            audioMixer.SetFloat(parameterName, Mathf.Log10(volume) * 20);
        }

        /// <summary>
        /// 오디오 타입에 따라 오디오를 음소거하거나 음소거 해제합니다.
        /// 음소거 상태를 반전시키며, 음소거 상태일 때는 현재 볼륨을 저장하고, 음소거 해제 시에는 저장된 볼륨으로 되돌립니다.
        /// </summary>
        /// <param name="audioType">오디오 타입(BGM, SFX, ALL)</param>
        public void MuteAudio(EAudioType audioType)
        {
            if (audioMixer == null)
            {
                Debug.LogError("AudioMixer is not initialized");
                return;
            }

            // 현재 오디오의 뮤트 상태를 반전 
            bool isMute = !this.isMute[(int)audioType];
            string parameterName = audioType.ToString();
            if (isMute)
            {
                this.isMute[(int)audioType] = true;
                audioMixer.GetFloat(parameterName, out float currentVolume);
                // 오디오 볼륨 배열에 저장
                audioVolumes[(int)audioType] = currentVolume;
                // 오디오 볼륨을 0.001f로 설정하여 음소거
                SetAudioVolume(audioType, 0.001f);
            }
            else
            {
                this.isMute[(int)audioType] = false;
                // 직전 오디오 볼륨으로 되돌리기
                SetAudioVolume(audioType, audioVolumes[(int)audioType]);
            }
        }

        public void SetOutputMixer(ref AudioSource source, EAudioType audioType)
        {
            if (audioMixer == null || source == null) return;

            // AudioSource의 outputAudioMixerGroup을 AudioMixer에 설정
            source.outputAudioMixerGroup = audioMixer.FindMatchingGroups(audioType.ToString())[0];
        }

        /// <summary>
        /// 시스템 오디오로 BGM 또는 SFX를 재생합니다.
        /// </summary>
        /// <param name="audioType">BGM or SFX</param>
        /// <param name="audioSituation">오디오 소스 종류</param>
        /// <param name="clipVolume">오디오 클립 볼륨</param>
        public void PlayAudioFromSystem(EAudioType audioType, EAudioSituation audioSituation, float clipVolume = 1f)
        {
            AudioEmitter targetEmitter = audioType switch
            {
                EAudioType.BGM => bgmEmitter,
                EAudioType.SFX => systemEmitter,
                _ => null
            };

            if (targetEmitter == null)
            {
                Debug.LogError($"AudioEmitter for {audioType} is not initialized");
                return;
            }

            PlayAudioFromEmitter(ref targetEmitter, audioSituation, clipVolume);
        }

        /// <summary>
        /// 지정된 AudioEmitter에서 오디오를 재생합니다.
        /// </summary>
        /// <param name="emitter">재생할 AudioEmitter</param>
        /// <param name="audioSituation">오디오 소스 종류</param>
        /// <param name="clipVolume">오디오 클립 볼륨</param>
        public void PlayAudioFromEmitter(ref AudioEmitter emitter, EAudioSituation audioSituation, float clipVolume = 1f)
        {
            if (emitter == null) return;

            // AudioEmitter의 오디오 타입 가져오기
            EAudioType audioType = emitter.CurrentAudioType;
            bool loop = audioType == EAudioType.BGM; // BGM 타입은 루프 재생
            // AudioData에서 오디오 타입에 맞는 오디오 리소스 가져오기
            AudioClip clip = null;
            // AudioEmitter에서 오디오 리소스 재생
            emitter.PlayAudio(clip, clipVolume, loop);
        }
    }
}