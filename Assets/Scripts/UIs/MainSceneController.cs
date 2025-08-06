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
using UIs.Panels.Infos;
using ManagerSystem.SaveLoad;
using UIs.Panels.Settings;
using UIs.Panels.Popups;


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
            settingBtn?.onClick.AddListener(OnClickSettingBtn);
            quitBtn?.onClick.AddListener(OnClickQuitBtn);

            SetEmitter();
            SetCanvas();
        }

        private async void SetCanvas()
        {
            if (infiniteBtn != null && settingBtn != null && quitBtn != null)
            {
                infiniteBtn.interactable = false;
                settingBtn.interactable = false;
                quitBtn.interactable = false;
            }

            await UniTask.WaitUntil(() => CanvasManager.Instance != null);
            Managers.UI?.FindCanvasAndGamePanel();

            if (infiniteBtn != null && settingBtn != null && quitBtn != null)
            {
                infiniteBtn.interactable = true;
                settingBtn.interactable = true;
                quitBtn.interactable = true;
            }
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

        private void OnClickSettingBtn()
        {
            // UIInfo 생성
            UIInfo uIInfo = new UIInfo(EButtonType.ONE_BUTTON, true, false);
            PlayerSettingsController settingsController = Managers.Save.PlayerSettings;

            // SettingUI 생성
            Managers.UI?.AddPanel<SettingUI>(uIInfo, "SettingUI", settingsController);

            // SFX 재생
            Managers.Audio?.PlayAudioFromSystem(EAudioType.SFX, EAudioSituation.System_Notice, 0, 1f);
        }

        private void OnClickQuitBtn()
        {
            // 팝업 창 띄우고, 확인 시 메인 메뉴로 이동
            PopupInfo popupInfo = new PopupInfo(() =>
            {
                Application.Quit(); // 애플리케이션 종료
            }, null, "게임 종료", "정말로 게임을 종료하시겠습니까?", "네", "아니요");

            Managers.UI?.AddPopup<PopupUI>(popupInfo);

            // SFX 재생
            Managers.Audio?.PlayAudioFromSystem(EAudioType.SFX, EAudioSituation.System_Alert, 0, 1f);
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