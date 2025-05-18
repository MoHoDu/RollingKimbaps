using System.Collections;
using Attributes;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class BackGroundPanel : BindUI
    {
        [Bind("FrontLine")] Transform _frontLine;
        [Bind("MiddleLine")] Transform _middleLine;
        [Bind("BackLine")] Transform _backLine;

        [SerializeField][Range(0, 10f)] private float _characterSpeed = 1f;

        private Transform[] _lines => new[] { _frontLine, _middleLine, _backLine };
        private readonly float _flowSpeed = 1.8f;
        private readonly float _speedGap = 0.5f;
        
        private Coroutine _flowCoroutine;

        protected override void Initialize()
        {
            _flowCoroutine = StartCoroutine(Flow());
        }

        private IEnumerator Flow()
        {
            while (true)
            {
                float[] flowSpeed = new float[]
                {
                    _characterSpeed * _flowSpeed, 
                    _characterSpeed * (_flowSpeed + _speedGap),
                    _characterSpeed * (_flowSpeed + _speedGap * 2)
                };

                for (int i = 0; i < _lines.Length; i++)
                {
                    Vector3 movement = Vector3.left * (flowSpeed[i] * Time.deltaTime);
                    _lines[i].position += movement;
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