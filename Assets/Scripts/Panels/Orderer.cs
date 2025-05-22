using Attributes;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    [RequireComponent(typeof(Animator))]
    public class Orderer : BindUI
    {
        [Bind("Camera")] private Camera _camera;
        private Animator _animator;

        protected override void Initialize()
        {
            base.Initialize();
            
            _animator = GetComponent<Animator>();
            _camera.enabled = false;
        }

        public void Successed()
        {
            _camera.enabled = true;
            _animator.SetTrigger("Successed");
        }
    }
}