using System.Collections;
using System.Collections.Generic;
using Attributes;
using Cysharp.Threading.Tasks;
using GameDatas;
using ManagerSystem;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class InGamePanel : BindUI
    {
        // Singleton
        private static InGamePanel _instance;
        public static InGamePanel Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("[Manager Error] InGamePanelController is null. Please check the inspector.");
                }
                
                return _instance;
            }
        }
        
        [Bind("BackGrounds")] public BackGroundPanel backGroundPanel;
        [Bind("Objects")] public InGameObjectsPanel objectsPanel;
        [Bind("Kimbap")] public Character characterPanel;
        
        private List<IFlowPanel> _flowPanels = new List<IFlowPanel>();
        private List<Coroutine> _flowCoroutines =  new List<Coroutine>();
        [SerializeField] private RaceStatus raceStatus;

        protected override async void Initialize()
        {
            base.Initialize();
            
            _instance = this;
            
            _flowPanels = new List<IFlowPanel>
            {
                backGroundPanel, objectsPanel
            };

            await UniTask.WaitUntil(() => objectsPanel.IsBindingDone);
            Managers.InGame.PlayGame(this, objectsPanel.groundsPanelTr).Forget();
        }

        private async UniTaskVoid TestRebirth()
        {
            raceStatus.StopVelocity();
            
            await UniTask.WaitForSeconds(2);
            characterPanel?.Rebirth();
            
            raceStatus.InitVelocity();
        }
        
        public void Setup(RaceStatus status)
        {
            // DI
            raceStatus = status;
            
            // Start Flow
            foreach (IFlowPanel flowPanel in _flowPanels)
            {
                _flowCoroutines.Add(flowPanel.StartFlow(raceStatus));
            }
            
            // Setup Character
            characterPanel.Setup(raceStatus);
            characterPanel.OnDeath -= () => TestRebirth().Forget();
            characterPanel.OnDeath += () => TestRebirth().Forget();

            raceStatus.InitVelocity();
        }
    }
}