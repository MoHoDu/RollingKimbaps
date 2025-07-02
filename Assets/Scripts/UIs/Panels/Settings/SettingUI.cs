using UnityEngine.UI;
using Attributes;
using UIs.Panels.Parts;
using ManagerSystem.SaveLoad;
using System;


namespace UIs.Panels.Settings
{
    public class SettingUI : PanelUI
    {
        // Sound
        [Bind("BGMSoundBar")]
        private CustomScrollbar _bgmSoundBar;
        [Bind("SFXSoundBar")]
        private CustomScrollbar _sfxSoundBar;
        // Touch
        [Bind("TouchReverseToggle")]
        private Toggle _touchReverseToggle;
        [Bind("UIReverseToggle")]
        private Toggle _uiReverseToggle;

        // Events
        public event Action onClickBackToMainMenu;

        public override void SetInfosInPanel(params object[] args)
        {
            foreach (var arg in args)
            {
                if (arg is PlayerSettingsController controller)
                {
                    // 플레이어 세팅 가져오기
                    PlayerSettings settings = controller.Data;

                    // 받은 플레이어 세팅 값을 기반으로 UI 세팅
                    SetVolumns(settings.BGMVolume, settings.SFXVolume);     // 볼륨 세팅
                    SetReverses(settings.ReverseTouch, settings.ReverseUI); // 반전 여부 세팅

                    // 이벤트 연결 
                    SetEvents(controller);
                }
            }
        }

        private void SetVolumns(float bgmVolume, float sfxVolume)
        {
            if (_bgmSoundBar != null) _bgmSoundBar.SetValue(bgmVolume);
            if (_sfxSoundBar != null) _sfxSoundBar.SetValue(sfxVolume);
        }

        private void SetReverses(bool touchReverse, bool uiReverse)
        {
            if (_touchReverseToggle != null) _touchReverseToggle.isOn = touchReverse;
            if (_uiReverseToggle != null) _uiReverseToggle.isOn = uiReverse;
        }

        private void SetEvents(PlayerSettingsController controller)
        {
            // 사운드 바 이벤트 설정
            if (_bgmSoundBar != null) _bgmSoundBar.onChangedValue += controller.ChangesBGMVolume;
            if (_sfxSoundBar != null) _sfxSoundBar.onChangedValue += controller.ChangesSFXVolume;

            // 토글 이벤트 설정
            if (_touchReverseToggle != null)
                _touchReverseToggle.onValueChanged.AddListener(controller.ChangesTouchReverse);
            if (_uiReverseToggle != null)
                _uiReverseToggle.onValueChanged.AddListener(controller.ChangesUIReverse);

            // 버튼 이벤트 설정
            if (_buttons != null)
            {
                // 초기화 버튼 이벤트
                _buttons.onClickFirstBtn += () =>
                {
                    _bgmSoundBar?.SetValue(1.0f);
                    _sfxSoundBar?.SetValue(1.0f);
                    if (_touchReverseToggle != null) _touchReverseToggle.isOn = false;
                    if (_uiReverseToggle != null) _uiReverseToggle.isOn = false;
                };

                // 게임 종료 버튼 이벤트
                _buttons.onClickSecondBtn += () => onClickBackToMainMenu?.Invoke();
            }
        }
    }
}