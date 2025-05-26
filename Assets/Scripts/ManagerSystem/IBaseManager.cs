namespace ManagerSystem
{
    public interface IBaseManager
    {
        public void Initialize(params object[] data);
        public void Start();
        public void Update();
        public void FixedUpdate();
        public void LateUpdate();
        public void OnDestroy();
    }
}