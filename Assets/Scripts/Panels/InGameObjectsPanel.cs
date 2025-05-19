using System.Collections;
using Attributes;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class InGameObjectsPanel : BindUI
    {
        [Bind("Grounds")] Transform _groundsPanelTr;
        
        [SerializeField][Range(0, 10f)] private float _characterSpeed = 1f;
        
        private readonly float _flowSpeed = 7f;
        private Coroutine _flowCoroutine;

        protected override void Initialize()
        {
            _flowCoroutine = StartCoroutine(Flow());
        }

        private IEnumerator Flow()
        {
            while (true)
            {
                float flowSpeed = _characterSpeed * _flowSpeed;
                Vector3 movement = Vector3.left * (flowSpeed * Time.deltaTime);
                _groundsPanelTr.localPosition += movement;
                
                yield return null;
            }
        }
        
        public void StopFlow()
        {
            if (_flowCoroutine != null)
            {
                StopCoroutine(_flowCoroutine);
                _flowCoroutine = null;
            }
        }
        
    }
}