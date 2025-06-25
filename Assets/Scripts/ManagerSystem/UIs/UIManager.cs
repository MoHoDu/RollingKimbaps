using UIs;
using UnityEngine;
using ManagerSystem.Base;
using UIs.Base;


namespace ManagerSystem.UIs
{
    public partial class UIManager : BaseManager
    {
        public CanvasManager CanvasManager { get; private set; }
        public InGamePanel InGamePanel { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            CloseAllPopups();
        }

        public void FindCanvasAndGamePanel()
        {
            CanvasManager = CanvasManager.Instance;
            InGamePanel = InGamePanel.Instance;
        }

        public T GetComponentFromCanvas<T>() where T : MonoBehaviour
        {
            return CanvasManager?.transform.GetComponent<T>();
        }

        public T[] GetComponentsFromCanvas<T>() where T : MonoBehaviour
        {
            return CanvasManager?.transform.GetComponentsInChildren<T>();
        }

        public T[] GetComponentsFromPanel<T>() where T : MonoBehaviour
        {
            return InGamePanel?.transform.GetComponentsInChildren<T>();
        }

        public T GetUI<T>(string name = null) where T : CanvasUI
        {
            return CanvasManager?.GetUI<T>(name);
        }

        public CanvasUI GetUI(string name)
        {
            return CanvasManager?.GetUI(name);
        }

        public T GetUIType<T>() where T : CanvasUI
        {
            return CanvasManager?.GetUIFromType<T>();
        }

        public T AddUI<T>(string name = null, params object[] inParms) where T : CanvasUI
        {
            if (name == null) name = typeof(T).Name;

            T ui = CanvasManager?.AddCanvasUI<T>(name, null, inParms);
            if (ui is null) return null;

            return ui;
        }

        public T AddUI<T>(params object[] inParms) where T : CanvasUI
        {
            string name = typeof(T).Name;
            return AddUI<T>(name, inParms);
        }
        
        public void RemoveUI(CanvasUI uiObj)
        {
            CanvasManager?.RemoveCanvasUI(uiObj);
        }

        public void RemoveUI(string uiName)
        {
            CanvasManager?.RemoveCanvasUI(uiName);
        }

        public void RemoveAllUI()
        {
            CanvasManager?.RemoveAllUIs();
        }
    }
}