using System.Collections;
using Attributes;
using ManagerSystem;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class BackGroundPanel : BindUI, IFlowPanel
    {
        // terrain
        [Bind("Back_terrains")] Transform _terrains;
        
        // trees
        [Bind("FrontLine")] Transform _frontLine;
        [Bind("MiddleLine")] Transform _middleLine;
        [Bind("BackLine")] Transform _backLine;
        [Bind("Trees_ForeLine")] Transform _foreLine;

        private Transform[] _lines => new[] { _backLine, _middleLine, _frontLine, _terrains, _foreLine };
        private readonly float _speedGap = 0.8f;
        
        private InGameStatus _inGameStatus;
        private Coroutine _flowCoroutine;

        protected override void Initialize()
        {
            // _flowCoroutine = StartCoroutine(Flow());
        }

        public Coroutine StartFlow(InGameStatus status)
        {
            _inGameStatus = status;
            _flowCoroutine = StartCoroutine(Flow());
            return _flowCoroutine;
        }

        private IEnumerator Flow()
        {
            while (true)
            {
                float flowSpeed = _inGameStatus.GetFlowSpeed() * 0.2f;

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