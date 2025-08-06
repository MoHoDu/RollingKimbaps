using UIs.Panels;
using UIs.Panels.Infos;
using ManagerSystem.SaveLoad;
using UIs.Panels.Settings;
using EnumFiles;
using UIs.Panels.Popups;
using UIs.Messages;
using UnityEngine;


namespace ManagerSystem.UIs
{
    //UIManager의 Panel 관련 내용
    public partial class UIManager
    {
        public T AddPanel<T>(UIInfo panelInfo, string uiName = null, params object[] infos) where T : PanelUI
        {
            T panel = AddUI<T>(uiName);

            // 패널에 기본 정보 설정: 버튼 타입, 종료 버튼 유무, 게임 일시 정지 여부, 버튼 액션들
            panel?.SetInfo(panelInfo);
            // 패널에 추가 정보 설정
            panel?.SetInfosInPanel(infos);

            if (panel != null && panelInfo != null)
            {
                if (panelInfo.pauseGame)
                {
                    // 패널이 null인 경우, 게임 일시 정지
                    Time.timeScale = 0f;
                    Managers.InGame?.PauseGame();

                    // 패널이 닫힐 때에 게임 재개
                    panel.OnClosedEvent += () =>
                    {
                        Time.timeScale = 1f; // 게임 시간 재개

                        // 3초 후에 게임 재개
                        // 3초를 세는 메시지 UI 띄우고, onClosedEvent에 재개 로직 추가
                        ShowMessage<CountMessageUI>(onClosedCallback: Managers.InGame.ResumeGame, inParms: 3f);
                    };
                }
            }

            return panel;
        }

        public SettingUI AddSettingPanel()
        {
            // UIInfo 생성
            UIInfo uIInfo = new UIInfo(EButtonType.TWO_BUTTONS, true, true);
            PlayerSettingsController settingsController = Managers.Save.PlayerSettings;

            // SettingUI 생성
            SettingUI settingUI = AddPanel<SettingUI>(uIInfo, "SettingUI", settingsController);

            // SFX 재생
            Managers.Audio?.PlayAudioFromSystem(EAudioType.SFX, EAudioSituation.System_Notice, 0, 1f);

            // 메인 메뉴 버튼 클릭 이벤트 설정
            settingUI.AddButtonAction(null, () =>
            {
                // 팝업 창 띄우고, 확인 시 메인 메뉴로 이동
                PopupInfo popupInfo = new PopupInfo(() =>
                {
                    Time.timeScale = 1f; // 게임 시간 재개
                    Managers.InGame?.ReturnToMainMenu();
                }, null, "메인 메뉴로 이동", "정말로 메인 메뉴로 이동하시겠습니까?", "네", "아니요");

                AddPopup<PopupUI>(popupInfo);

                // SFX 재생
                Managers.Audio?.PlayAudioFromSystem(EAudioType.SFX, EAudioSituation.System_Alert, 0, 1f);
            });

            return settingUI;
        }
    }
}