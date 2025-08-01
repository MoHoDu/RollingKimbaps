using Audio;
using Cysharp.Threading.Tasks;
using ManagerSystem;
using UnityEngine;


namespace InGame
{
    public class InGameSceneHandler : MonoBehaviour
    {
        [SerializeField] private AudioEmitter bgmAudioEmitter;
        [SerializeField] private AudioEmitter systemSfxAudioEmitter;

        private void Awake()
        {
            // audio manager 대기 후 애미터 등록
            SetEmitter();
        }

        private async void SetEmitter()
        {
            if (bgmAudioEmitter == null || systemSfxAudioEmitter == null)
            {
                Debug.LogError("AudioEmitter is not assigned in InGame Scene.");
                return;
            }

            // Wait for AudioManager to be initialized
            await UniTask.WaitUntil(() => Managers.Audio != null && Managers.Audio.IsInitialized);

            // Set the audio emitters in the AudioManager
            Managers.Audio.SetEmitterInScene(bgmAudioEmitter, systemSfxAudioEmitter);

            // 브금 재생 요청
            Managers.Audio.PlayAudioFromSystem(EAudioType.BGM, EAudioSituation.BGM_Ingame);
        }
    }
}