using Panels;
using UnityEngine;

namespace ManagerSystem
{
    public class UIManager : BaseManager
    {
        public CanvasManager CanvasManager { get; private set; }
        public InGamePanel InGamePanel { get; private set; }
        
        public void FindCanvasAndGamePanel()
        {
            CanvasManager = CanvasManager.Instance;
            InGamePanel = InGamePanel.Instance;

            Managers.InGame.InitManagers();
        }

        public T[] GetComponentsFromCanvas<T>() where T : MonoBehaviour
        {
            return CanvasManager?.transform.GetComponentsInChildren<T>();
        }
        
        public T[] GetComponentsFromPanel<T>() where T : MonoBehaviour
        {
            return InGamePanel?.transform.GetComponentsInChildren<T>();
        }
    }
}