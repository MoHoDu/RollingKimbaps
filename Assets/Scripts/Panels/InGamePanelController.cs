using System.Collections;
using System.Collections.Generic;
using Attributes;
using Cysharp.Threading.Tasks;
using ManagerSystem;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class InGamePanelController : BindUI
    {
        [Bind("BackGrounds")] public BackGroundPanel backGroundPanel;
        [Bind("Objects")] public InGameObjectsPanel objectsPanel;
        [Bind("Kimbap")] public Character characterPanel;
        
        private List<IFlowPanel> _flowPanels = new List<IFlowPanel>();
        private List<Coroutine> _flowCoroutines =  new List<Coroutine>();
        [SerializeField] private InGameStatus _inGameStatus;
        [SerializeField] private float _fastSpeed = 1.2f;
        [SerializeField] private float _maxSpeed = 20f;

        protected override void Initialize()
        {
            base.Initialize();
            
            _flowPanels = new List<IFlowPanel>
            {
                backGroundPanel, objectsPanel
            };

            Managers.InGame.PlayGame(this).Forget();
        }

        private async UniTaskVoid TestRebirth()
        {
            _inGameStatus.Velocity = 0;
            
            await UniTask.WaitForSeconds(3);
            characterPanel?.Rebirth();
            
            _inGameStatus.InitVelocity();
            StartCoroutine(FasterFlow());
        }

        private IEnumerator FasterFlow()
        {
            do
            {
                _inGameStatus.Velocity += _fastSpeed * Time.deltaTime;
                if (_inGameStatus.Velocity > _maxSpeed)
                    _inGameStatus.Velocity = _maxSpeed;
                yield return new WaitForSeconds(1);
            } while (_inGameStatus.Velocity > 0);
        }
        
        public void Setup(InGameStatus status)
        {
            // DI
            _inGameStatus = status;
            
            // Start Flow
            foreach (IFlowPanel flowPanel in _flowPanels)
            {
                _flowCoroutines.Add(flowPanel.StartFlow(_inGameStatus));
            }
            
            // Setup Character
            characterPanel.Setup(_inGameStatus);
            characterPanel.OnDeath -= () => TestRebirth().Forget();
            characterPanel.OnDeath += () => TestRebirth().Forget();

            _inGameStatus.InitVelocity();
            StartCoroutine(FasterFlow());
        }
    }
}