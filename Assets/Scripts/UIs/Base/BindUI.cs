using Attributes;
using UnityEngine;


namespace UIs.Base
{
    public class BindUI : MonoBehaviour
    {
        private bool _isBindingDone = false;
        public bool IsBindingDone => _isBindingDone;

        protected virtual void Awake()
        {
            InstallBindings();
            Initialize();
        }

        public void InstallBindings()
        {
            if (_isBindingDone) return;
            BindAttribute.InstallBindings(this);
            _isBindingDone = true;
        }

        protected virtual void Initialize() { }
    }
}