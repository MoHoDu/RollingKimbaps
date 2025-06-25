using UnityEngine;

namespace ManagerSystem.Base
{
    public class BaseManager : IBaseManager
    {
        public virtual void Initialize(params object[] data) { }

        public virtual void Initialize()
        {
            Initialize(null);
        }
        
        public virtual void Start() { }
        public virtual void Tick() { }

        public virtual void OnStartGame() { }

        public virtual void Update() { }

        public virtual void FixedUpdate() { }

        public virtual void LateUpdate() { }

        public virtual void OnDestroy() { }
    }

    public class MonoBaseManager : MonoBehaviour, IBaseManager
    {
        public virtual void Initialize(params object[] data) { }

        public virtual void Initialize()
        {
            Initialize(null);
        }
        
        public virtual void Start() { }

        public virtual void Tick() { }
        public virtual void OnStartGame() { }

        public virtual void Update() { }

        public virtual void FixedUpdate() { }

        public virtual void LateUpdate() { }

        public virtual void OnDestroy() { }
    }
}