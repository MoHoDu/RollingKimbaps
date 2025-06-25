using System;
using Attributes;
using UIs.Base;
using UIs.Panels.Infos;


namespace UIs.Panels
{
    // 일반 UI가 아닌 Panel용 UI
    public class PanelUI : CanvasUI
    {
        [Bind("Buttons")]
        protected ButtonsInPanel _buttons;

        /// <summary>
        /// 패널을 생성한 뒤 정보를 주입하는 메서드 (생성 즉시 호출하세요.)
        /// </summary>
        /// <param name="info">Panel 정보</param>
        public virtual void SetInfo(UIInfo info)
        {
            if (info == null) return;
            if (!IsBindingDone) Awake();

            // info의 타입에 맞게 버튼 설정
            if (_buttons != null)
            {
                // 버튼 on, off 설정
                bool hasQuitButton = info.placeQuitButton;
                if (info.buttonType == EnumFiles.EButtonType.NONE)
                    _buttons.SetButtonActive(hasQuitButton, false, false);
                else if (info.buttonType == EnumFiles.EButtonType.ONE_BUTTON)
                    _buttons.SetButtonActive(hasQuitButton, true, false);
                else if (info.buttonType == EnumFiles.EButtonType.TWO_BUTTONS)
                    _buttons.SetButtonActive(hasQuitButton, true, true);

                // 버튼 이벤트 설정
                _buttons.onClickCloseBtn += Close;
                if (info.buttonActions != null && info.buttonActions.Length > 0)
                {
                    if (info.buttonType == EnumFiles.EButtonType.ONE_BUTTON)
                    {
                        _buttons.onClickFirstBtn += info.buttonActions[0];
                    }
                    else if (info.buttonType == EnumFiles.EButtonType.TWO_BUTTONS)
                    {
                        _buttons.onClickFirstBtn += info.buttonActions[0];
                        if (info.buttonActions.Length > 1) _buttons.onClickSecondBtn += info.buttonActions[1];
                    }
                }
            }
        }

        /// <summary>
        /// 패널 내부에 필요한 정보들을 설정합니다. (SetInfo 이후에 호출하세요.)
        /// </summary>
        /// <param name="infos"></param>
        public virtual void SetInfosInPanel(params object[] infos)
        {
            // 패널 내부에 필요한 정보들을 설정하는 메서드
        }

        /// <summary>
        /// 버튼들의 텍스트를 설정합니다.
        /// </summary>
        /// <param name="texts">순차적으로 1, 2번째 버튼의 텍스트에 적용됩니다.</param>
        public void SetButtonText(params string[] texts)
        {
            // 버튼 텍스트 설정
            if (_buttons != null && texts != null)
            {
                if (texts.Length > 1) _buttons.SetButtonText(texts[0], texts[1]);
                else if (texts.Length == 1) _buttons.SetButtonText(texts[0]);
            }
        }

        /// <summary>
        /// 버튼 액션을 추가합니다.
        /// </summary>
        /// <param name="firstAction">첫 번째 버튼 클릭 시 실행할 액션</param>
        /// <param name="secondAction">두 번째 버튼 클릭 시 실행할 액션</param>
        public void AddButtonAction(Action firstAction = null, Action secondAction = null)
        {
            // 버튼 액션 추가
            if (_buttons != null)
            {
                if (firstAction != null) _buttons.onClickFirstBtn += firstAction;
                if (secondAction != null) _buttons.onClickSecondBtn += secondAction;
            }
        }
    }
}