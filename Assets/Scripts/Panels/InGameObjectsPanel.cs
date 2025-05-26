using System.Collections;
using Attributes;
using GameDatas;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class InGameObjectsPanel : BindUI, IFlowPanel
    {
        [Bind("Grounds")] GroundsPanel _groundsPanel;
        
        public Transform groundsPanelTr => _groundsPanel != null ? _groundsPanel.transform : null;
        
        private readonly float _flowSpeed = 0.03f;
        private InGameStatus _inGameStatus;
        private Coroutine _flowCoroutine;
        
        public Coroutine StartFlow(InGameStatus status)
        {
            _groundsPanel.StartGame(status);
            _inGameStatus = status;
            _flowCoroutine = StartCoroutine(Flow());
            return _flowCoroutine;
        }

        private IEnumerator Flow()
        {
            while (true)
            {
                float flowSpeed = _inGameStatus.GetFlowSpeed();
                Vector3 movement = Vector3.left * (flowSpeed * Time.deltaTime);
                groundsPanelTr.position += movement;
                
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