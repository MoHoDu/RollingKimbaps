using System.Collections;
using Attributes;
using ManagerSystem;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class InGameObjectsPanel : BindUI, IFlowPanel
    {
        [Bind("Grounds")] Transform _groundsPanelTr;
        [Bind("Grounds")] GroundsPanel _groundsPanel;
        [Bind("Obstacles")] Transform _obstaclesPanelTr;
        
        private readonly float _flowSpeed = 7f;
        private InGameStatus _inGameStatus;
        private Coroutine _flowCoroutine;

        protected override void Initialize()
        {
            _groundsPanel.StartGame();
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
                float flowSpeed = _inGameStatus.Velocity * _flowSpeed;
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