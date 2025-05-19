using System.Collections;
using Attributes;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class BackGroundPanel : BindUI
    {
        // terrain
        [Bind("Back_terrains")] Transform _terrains;
        
        // trees
        [Bind("FrontLine")] Transform _frontLine;
        [Bind("MiddleLine")] Transform _middleLine;
        [Bind("BackLine")] Transform _backLine;
        [Bind("Trees_ForeLine")] Transform _foreLine;

        [SerializeField][Range(0, 10f)] private float _characterSpeed = 1f;

        private Transform[] _lines => new[] { _backLine, _middleLine, _frontLine, _terrains, _foreLine };
        private readonly float _flowSpeed = 3f;
        private readonly float _speedGap = 1f;
        
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

                for (int i = 0; i < _lines.Length; i++)
                {
                    float gap = i == 0 ? 1f : _speedGap * i;
                    Vector3 movement = Vector3.left * (flowSpeed * gap * Time.deltaTime);
                    _lines[i].localPosition += movement;
                }
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