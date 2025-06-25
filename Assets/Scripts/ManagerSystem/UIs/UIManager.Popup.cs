using UIs.Panels.Infos;
using System.Collections.Generic;
using UIs.Panels.Popups;


namespace ManagerSystem.UIs
{
    //UIManager의 Panel 관련 내용
    public partial class UIManager
    {
        private Stack<PopupUI> popupUIs = new Stack<PopupUI>();

        public T AddPopup<T>(PopupInfo panelInfo, string uiName = null, params object[] infos) where T : PopupUI
        {
            if (string.IsNullOrEmpty(uiName))
            {
                uiName = $"Popups/{typeof(T).Name}";
            }

            T panel = AddPanel<T>(panelInfo, uiName, infos);
            if (panel != null)
            {
                popupUIs.Push(panel);
                panel.OnClosedEvent += ClosePopup;
            }

            return panel;
        }

        public void ClosePopup()
        {
            if (popupUIs.Count > 0)
            {
                PopupUI popup = popupUIs.Pop();
                popup?.Close();
            }
        }

        public void CloseAllPopups()
        {
            while (popupUIs.Count > 0)
            {
                PopupUI popup = popupUIs.Pop();
                popup?.Close();
            }
        }
    }
}