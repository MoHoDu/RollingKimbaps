using System;
using System.Collections.Generic;
using EnumFiles;
using UIs.Base;
using UIs.Panels;
using UIs.Panels.Infos;


namespace ManagerSystem.UIs
{
    public partial class UIManager
    {
        private Queue<Action> _nextMessage = new();
        public bool IsMessaging => _nextMessage.Count > 0;
        public int MessageCount => _nextMessage.Count;

        public void AddMessage(Action inAction)
        {
            _nextMessage.Enqueue(inAction);
            if (MessageCount == 1) inAction?.Invoke();
        }

        public void NextMessage()
        {
            if (IsMessaging)
            {
                _nextMessage.Dequeue();
            }

            if (IsMessaging)
            {
                _nextMessage.Peek()?.Invoke();
            }
        }

        public void ShowMessage<T>(string uiName = null, Action onClosedCallback = null, params object[] inParms) where T : PanelUI
        {
            if (string.IsNullOrEmpty(uiName))
            {
                uiName = $"Messages/{typeof(T).Name}";
            }

            AddMessage(() =>
            {
                UIInfo info = new UIInfo(EButtonType.NONE, false, false);
                T ui = AddPanel<T>(info, uiName, inParms);

                ui.OnClosedEvent += NextMessage;
                if (onClosedCallback != null) ui.OnClosedEvent += onClosedCallback;
            });
        }

        public void ClearMessage()
        {
            _nextMessage.Clear();
        }
    }
}