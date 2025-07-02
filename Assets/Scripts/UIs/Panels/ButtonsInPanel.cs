using System;
using Attributes;
using UnityEngine.UI;
using UIs.Base;
using TMPro;


namespace UIs.Panels
{
    public class ButtonsInPanel : BindUI
    {
        // Buttons
        [Bind("CloseButton")]
        private Button _closeBtn;
        [Bind("BottomFirstButton")]
        private Button _bottomFirstButton;
        [Bind("BottomSecondButton")]
        private Button _bottomSecondButton;

        // Events
        public event Action onClickCloseBtn;
        public event Action onClickFirstBtn;
        public event Action onClickSecondBtn;

        protected override void Initialize()
        {
            // 각 버튼?.onclick.addEvent로 이벤트를 걸어둠
            _closeBtn?.onClick.AddListener(() => onClickCloseBtn?.Invoke());
            _bottomFirstButton?.onClick.AddListener(() => onClickFirstBtn?.Invoke());
            _bottomSecondButton?.onClick.AddListener(() => onClickSecondBtn?.Invoke());
        }

        public void SetButtonText(string firstText, string secondText = "")
        {
            if (!IsBindingDone) Awake();

            // 버튼 텍스트를 가져옴
            TextMeshProUGUI first = _bottomFirstButton?.GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI second = _bottomSecondButton?.GetComponentInChildren<TextMeshProUGUI>();

            // 버튼 텍스트 설정
            if (first != null) first.text = firstText;
            if (second != null) second.text = secondText;
        }

        public void SetButtonActive(bool closeActive = true, bool firstActive = false, bool secondActive = false)
        {
            // 버튼 활성화 설정
            _closeBtn?.gameObject?.SetActive(closeActive);
            _bottomFirstButton?.gameObject?.SetActive(firstActive);
            _bottomSecondButton?.gameObject?.SetActive(secondActive);
        }
    }
}