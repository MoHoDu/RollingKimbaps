namespace ManagerSystem
{
    public class UIManager : BaseManager
    {
        private CanvasManager _canvasManager;
        
        public void FindCanvasManager()
        {
            _canvasManager = CanvasManager.Instance;
        }
    }
}