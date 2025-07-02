using Attributes;
using GameDatas;
using InGame;
using UIs.Base;
using UnityEngine;

namespace UIs
{
    [RequireComponent(typeof(Animator))]
    public class OrdererPrap : Prap
    {
        [Bind("Camera")] private Camera _camera;
        private Animator _animator;
        private OrderData _orderInfo;

        protected override void Initialize()
        {
            base.Initialize();
            
            _animator = GetComponent<Animator>();
            _camera.enabled = false;
        }

        public override void OnSpawned(params object[] args)
        {
            foreach (var arg in args)
            {
                if (arg is OrderData menu)
                {
                    _orderInfo = menu;
                    _orderInfo.OnClearOrder += Successed;
                    _orderInfo.onFailedOrder += Failed;
                }
            }
        }

        public void Successed()
        {
            _camera.enabled = true;
            _animator.SetTrigger("Successed");
        }

        public void Failed()
        {
            
        }
    }
}