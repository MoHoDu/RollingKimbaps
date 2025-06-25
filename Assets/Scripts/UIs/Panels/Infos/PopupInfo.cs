using System;
using EnumFiles;


namespace UIs.Panels.Infos
{
    public class PopupInfo : UIInfo
    {
        public string title;
        public string content;

        // Button Texts
        public string okButtonText;
        public string cancelButtonText;

        // 확인 버튼만 있는 팝업 UI
        public PopupInfo(Action onClickOK,
            string title = null, string content = null, string okButtonText = "OK")
            : base(EButtonType.ONE_BUTTON, false, false, onClickOK)
        {
            this.title = title;
            this.content = content;
            this.okButtonText = okButtonText;
        }

        // 확인/취소 버튼이 있는 팝업 UI
        public PopupInfo(Action onClickOK, Action onClickCancel,
            string title = null, string content = null,
            string okButtonText = "OK", string cancelButtonText = "No")
            : base(EButtonType.TWO_BUTTONS, false, false, onClickOK, onClickCancel)
        {
            this.title = title;
            this.content = content;
            this.okButtonText = okButtonText;
            this.cancelButtonText = cancelButtonText;
        }
    }
}