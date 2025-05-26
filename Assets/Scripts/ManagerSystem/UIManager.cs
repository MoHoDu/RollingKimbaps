using Panels;
using UnityEngine;

namespace ManagerSystem
{
    public class UIManager : BaseManager
    {
        private CanvasManager _canvasManager;
        private InGamePanel _inGamePanel;
        
        public void FindCanvasAndGamePanel()
        {
            _canvasManager = CanvasManager.Instance;
            _inGamePanel = InGamePanel.Instance;
        }

        public T[] GetComponentsFromCanvas<T>() where T : MonoBehaviour
        {
            return _canvasManager?.transform.GetComponentsInChildren<T>();
        }
        
        public T[] GetComponentsFromPanel<T>() where T : MonoBehaviour
        {
            return _inGamePanel?.transform.GetComponentsInChildren<T>();
        }
    }
}