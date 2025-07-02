using UIs.Base;
using Attributes;
using ManagerSystem.InGame;
using UnityEngine;
using UnityEngine.UI;
using ManagerSystem;

namespace UIs
{
    public class InGameUI : CanvasUI
    {
        [Bind("LeftSide")] private VerticalLayoutGroup _leftSide;
        [Bind("RightSide")] private VerticalLayoutGroup _rightSide;
        [Bind("SettingUIButtonUI")] private SettingUIButtonUI _settingsUI;
        [Bind("ScoreUI")] private ScoreUI _scoreUI;
        [Bind("LifeUI")] private LifeUI _lifeUI;
        [Bind("OrdersUI")] private OrdersUI _ordersUI;
        [Bind("OrdersUI")] private VerticalLayoutGroup _ordersLayout;

        public OrdersUI OrdersUI => _ordersUI;
        
        private RectTransform _leftSideRect;
        private RectTransform _rightSideRect;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Initialize()
        {
            base.Initialize();

            // RectTransform 컴포넌트 가져오기
            _leftSideRect = _leftSide.GetComponent<RectTransform>();
            _rightSideRect = _rightSide.GetComponent<RectTransform>();

            // 세팅에 따라 UI 위치 조정
            bool isReversed = Managers.Save.PlayerSettings.Data.ReverseUI;
            SetUIPosition(isReversed);
        }

        public void SetUIPosition(bool isReversed)
        {
            if (isReversed)
            {
                _leftSideRect.anchorMin = new Vector2(1, 1f);
                _leftSideRect.anchorMax = new Vector2(1, 1f);
                _leftSideRect.pivot = new Vector2(1, 1f);

                _rightSideRect.anchorMin = new Vector2(0, 1f);
                _rightSideRect.anchorMax = new Vector2(0, 1f);
                _rightSideRect.pivot = new Vector2(0, 1f);

                _leftSide.childAlignment = TextAnchor.UpperRight;
                _rightSide.childAlignment = TextAnchor.UpperLeft;

                _ordersLayout.childAlignment = TextAnchor.UpperLeft;
            }
            else
            {
                _leftSideRect.anchorMin = new Vector2(0, 1f);
                _leftSideRect.anchorMax = new Vector2(0, 1f);
                _leftSideRect.pivot = new Vector2(0, 1f);

                _rightSideRect.anchorMin = new Vector2(1, 1f);
                _rightSideRect.anchorMax = new Vector2(1, 1f);
                _rightSideRect.pivot = new Vector2(1, 1f);

                _leftSide.childAlignment = TextAnchor.UpperLeft;
                _rightSide.childAlignment = TextAnchor.UpperRight;

                _ordersLayout.childAlignment = TextAnchor.UpperRight;
            }

            _leftSideRect.anchoredPosition = Vector2.zero;
            _rightSideRect.anchoredPosition = Vector2.zero;

            // OrderUI 위치 조정
            _ordersUI.OnChangedReverseUI(isReversed);

            // UI 위치 조정 후, 레이아웃 갱신
            Canvas.ForceUpdateCanvases();
        }

        public override void SetInfoInUI(params object[] infos)
        {
            if (!IsBindingDone)
                Awake();

            foreach (var info in infos)
            {
                if (info is StatusManager statusManager)
                {
                    // scoreUI에 상태 매니저 전달
                    _scoreUI.SetInfoInUI(statusManager);
                    // LifeUI에 상태 매니저 전달
                    _lifeUI.SetInfoInUI(statusManager);
                }
            }
        }
    }
}