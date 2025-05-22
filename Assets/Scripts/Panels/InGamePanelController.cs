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

        protected override async void Initialize()
        {
            base.Initialize();
            
            _flowPanels = new List<IFlowPanel>
            {
                backGroundPanel, objectsPanel
            };

            await UniTask.WaitUntil(() => objectsPanel.IsBindingDone);
            Managers.InGame.PlayGame(this, objectsPanel.groundsPanelTr).Forget();
        }

        private async UniTaskVoid TestRebirth()
        {
            _inGameStatus.StopVelocity();
            
            await UniTask.WaitForSeconds(2);
            characterPanel?.Rebirth();
            
            _inGameStatus.InitVelocity();
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
        }
    }
}