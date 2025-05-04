namespace ManagerSystem
{
    public interface IBaseManager
    {
        public void Initialize();
        public void Start();
        public void Update();
        public void FixedUpdate();
        public void LateUpdate();
        public void OnDestroy();
    }
}