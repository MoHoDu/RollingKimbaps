using System;
using UnityEngine;
using UnityEngine.UI;
using Attributes;
using UIs.Base;


namespace UIs.Panels.Parts
{
    [RequireComponent(typeof(Scrollbar))]
    public class CustomScrollbar : BindUI
    {
        // Components
        [Bind("FillingBG")]
        private RectTransform _fillingRect;
        [Bind("MuteIcon")]
        private CanvasGroup _muteIcon;
        private Scrollbar scrollbar;
        private Image[] _fillImages;                // 스크롤바 값만큼 채워지는 이미지들

        // Values
        private int _currentStep = 0;               // 현재 스크롤바의 단계

        // Events
        public event Action<float> onChangedValue;  // 스크롤바 값이 변경될 때 호출되는 이벤트

        protected override void Initialize()
        {
            // _fillingImages를 가져옴 
            _fillImages = _fillingRect.GetComponentsInChildren<Image>();

            // 스크롤바를 가져와 이벤트 연결
            scrollbar = GetComponent<Scrollbar>();
            scrollbar?.onValueChanged.AddListener(OnChangedScrollValue);
        }

        public void SetValue(float value)
        {
            if (!IsBindingDone) Awake();

            // Scrollbar 컴포넌트에 접근하여 값을 설정
            if (scrollbar != null)
            {
                scrollbar.value = value;
                OnChangedScrollValue(value); // 값이 변경되었을 때 이벤트 호출
            }
        }

        protected void OnChangedScrollValue(float value)
        {
            // fillImages가 있는지 여부 확인
            bool hasFillImages = _fillImages != null && _fillImages.Length > 0;

            // Step 가져오기
            float handleSize = 1f / scrollbar.numberOfSteps; // 스크롤바의 핸들 크기
            int step = Mathf.Clamp(Mathf.FloorToInt(value / handleSize), 0, scrollbar.numberOfSteps - 1);
            int fillSize = hasFillImages ? _fillImages.Length : 0;

            // step 변경 시
            if (_currentStep != step)
            {
                // step - 1 만큼 fillImage On
                for (int i = 0; i < fillSize; i++)
                {
                    _fillImages[i].gameObject.SetActive(i < step);
                }

                _currentStep = step; // 현재 스텝 업데이트
            }

            // value가 0이면 muteIcon 알파를 1로 설정
            _muteIcon.alpha = value <= 0 ? 1f : 0f;

            // 외부 이벤트 호출
            onChangedValue?.Invoke(value);
        }
    }
}