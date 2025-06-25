using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Attributes;
using UIs.Panels.Infos;


namespace UIs.Panels.Popups
{
    // 팝업 창으로 뜨는 정보
    public class PopupUI : PanelUI
    {
        [Bind("TitleText")]
        private TextMeshProUGUI titleText;
        [Bind("ContentText")]
        private TextMeshProUGUI contentText;

        public override void SetInfo(UIInfo info)
        {
            base.SetInfo(info);

            if (info is PopupInfo popupInfo)
            {
                // 팝업 정보 설정
                if (!string.IsNullOrEmpty(popupInfo.title)) titleText.text = popupInfo.title;
                if (!string.IsNullOrEmpty(popupInfo.content)) contentText.text = popupInfo.content;

                // 버튼 텍스트 설정
                if (_buttons != null)
                {
                    if (!string.IsNullOrEmpty(popupInfo.okButtonText))
                        _buttons.SetButtonText(popupInfo.okButtonText, popupInfo.cancelButtonText);

                    if (info.buttonType == EnumFiles.EButtonType.TWO_BUTTONS)
                    {
                        if (info.buttonActions == null || info.buttonActions.Length < 2 || info.buttonActions[1] == null)
                        {
                            if (!info.placeQuitButton)
                            {
                                // 두 번째 버튼에 닫기 이벤트 연결
                                _buttons.onClickSecondBtn += Close;
                            }
                        }
                    }
                }
            }
        }

        public override void SetInfosInPanel(params object[] args)
        {
            bool setTitle = false;
            foreach (var arg in args)
            {
                if (arg is string text)
                {
                    if (setTitle) titleText.text = text;
                    else contentText.text = text;
                }
            }
        }
    }
}