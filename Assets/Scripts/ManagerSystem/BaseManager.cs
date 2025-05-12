using UnityEngine;

namespace ManagerSystem
{
    public class BaseManager : IBaseManager
    {
        public virtual void Initialize() { }
        
        public virtual void Start() { }

        public virtual void Update() { }

        public virtual void FixedUpdate() { }

        public virtual void LateUpdate() { }

        public virtual void OnDestroy() { }
    }

    public class MonoBaseManager : MonoBehaviour, IBaseManager
    {
        public virtual void Initialize() { }
        
        public virtual void Start() { }

        public virtual void Update() { }

        public virtual void FixedUpdate() { }

        public virtual void LateUpdate() { }

        public virtual void OnDestroy() { }
    }
}