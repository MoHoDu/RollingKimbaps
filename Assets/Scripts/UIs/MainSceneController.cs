using Attributes;
using UIs.Base;
using UnityEngine.UI;
using EnumFiles;
using JsonData;
using ManagerSystem;
using UnityEngine;
using ManagerSystem.UIs;
using Audio;
using Cysharp.Threading.Tasks;


namespace UIs
{
    public class MainSceneController : BindUI
    {
        [Bind("StartButton")] private Button startBtn;
        [Bind("InfiniteButton")] private Button infiniteBtn;
        [Bind("SettingButton")] private Button settingBtn;
        [Bind("QuitButton")] private Button quitBtn;

        // Audio Emitters
        [SerializeField] private AudioEmitter bgmAudioEmitter;
        [SerializeField] private AudioEmitter systemSfxAudioEmitter;

        protected override void Initialize()
        {
            // startBtn?.onClick.AddListener(() => LoadSaveFiles(GameType.Story));
            // infiniteBtn?.onClick.AddListener(() => LoadSaveFiles(GameType.Infinite));
            infiniteBtn?.onClick.AddListener(() =>
            {
                infiniteBtn.interactable = false; // Prevent multiple clicks
                Managers.Stage.LoadSceneAsync("GameScene", null);
            });

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
            Managers.Audio.PlayAudioFromSystem(EAudioType.BGM, EAudioSituation.BGM_Lobby, 0, 0.3f);
        }

        private void LoadSaveFiles(GameType gameType)
        {
            SaveData defaultData = DataContainer.SaveFiles.GetDefaultData(gameType);
            SaveData[] saveDatas = DataContainer.SaveFiles.Get(gameType);

            SavePanel panel = CanvasManager.Instance.GetUI<SavePanel>();
            if (panel != null)
            {
                panel.SetInfoInUI(gameType, defaultData, saveDatas);
            }
            else
            {
                panel = CanvasManager.Instance.AddCanvasUI<SavePanel>("SavePanel", null, gameType, defaultData, saveDatas);
                panel.AddEventOnDeleteData((data) =>
                {
                    bool result = Managers.Save.RemoveData(data);
                    string resultText = result ? "성공!!" : "실패...";
                    Debug.Log($"세이브 데이터 제거 {resultText}");
                });
                panel.OnRefreshData = DataContainer.SaveFiles.Get;
            }
        }
    }
}