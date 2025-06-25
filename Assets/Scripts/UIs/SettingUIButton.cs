using UnityEngine;
using UnityEngine.UI;
using Attributes;
using UIs.Base;
using ManagerSystem;


namespace UIs
{
    public class SettingUIButtonUI : CanvasUI
    {
        [Bind("Button")] private Button _button;

        protected override void Initialize()
        {
            base.Initialize();

            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            // 설정 버튼 클릭 시 호출되는 메서드
            Debug.Log("Setting button clicked!");

            // 설정 패널을 열기 위한 메서드 호출
            Managers.UI.AddSettingPanel();
        }
    }
}