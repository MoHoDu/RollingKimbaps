using ManagerSystem;
using UnityEngine;


namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioEmitter : MonoBehaviour
    {
        private AudioSource audioSource;
        private EAudioType currentAudioType = EAudioType.Master;

        public EAudioType CurrentAudioType => currentAudioType;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource component is missing on the AudioEmitter GameObject.");
            }
        }

        public void SetAudioType(EAudioType audioType)
        {
            currentAudioType = audioType;

            // AudioSource가 null인 경우 Awake()를 호출하여 초기화
            if (audioSource == null) Awake();

            // AudioSource가 여전히 null인 경우 에러 메시지 출력
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is not initialized in AudioEmitter.");
                return;
            }

            // output 세팅 요청 
            Managers.Audio.SetOutputMixer(ref audioSource, audioType);
        }

        public void PlayAudio(AudioClip clip, float clipVolume, bool loop = false)
        {
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is not initialized in AudioEmitter.");
                return;
            }

            if (audioSource.isPlaying)
            {
                StopAudio();
            }

            audioSource.clip = clip;
            audioSource.volume = clipVolume;
            audioSource.loop = loop;

            audioSource.Play();
        }

        public void StopAudio()
        {
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is not initialized in AudioEmitter.");
                return;
            }

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}