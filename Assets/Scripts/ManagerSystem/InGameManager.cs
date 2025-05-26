using System;
using Cysharp.Threading.Tasks;
using EnumFiles;
using GameDatas;
using JsonData;
using ManagerSystem.InGame;
using Panels;
using UnityEngine;
using Utils;

namespace ManagerSystem
{
    public class InGameManager : BaseManager
    {
        // 매니저 
        public StatusManager Status { get; private set; } = new();
        public PrapManager Prap { get; private set; } = new();
        
        private EGameStatus _gameStatus => Status.GameStatus;
        
        
        
        
        public GroundBuilder GroundBuilder => _groundBuilder;
        
        private RaceStatus raceStatus = new RaceStatus();
        private InputManager inputManager;

        private InGamePanelController _inGamePanel; 
        private GroundBuilder _groundBuilder = new GroundBuilder();

        public void SetInputController(InputManager inputManager)
        {
            this.inputManager = inputManager;
            this.inputManager?.Setup(raceStatus);
            this.inputManager?.Initialize();
        }

        public void AddEventOnInput(EInputType inType, Action action)
        {
            switch (inType)
            {
                case EInputType.JUMP:
                    inputManager.OnJumped -= action;
                    inputManager.OnJumped += action;
                    break;
                case EInputType.PAUSE:
                    inputManager.OnPause -= action;
                    inputManager.OnPause += action;
                    break;
                case EInputType.RESUME:
                    inputManager.OnResume -= action;
                    inputManager.OnResume += action;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 게임씬으로 변경된 이후에 호출해주세요.
        /// </summary>
        public async UniTaskVoid PlayGame(InGamePanelController controller, Transform groundTr)
        {
            await UniTask.WaitUntil(() => inputManager != null);
            
            // 초기화 
            raceStatus.ClearData();
            inputManager?.Initialize();
            
            _inGamePanel = controller;
            _inGamePanel.Setup(raceStatus);
            
            // key settings
            AddEventOnInput(EInputType.JUMP, _inGamePanel.characterPanel.OnJump);
            
            raceStatus.StartGame(groundTr);
        }

        public void SetData(SaveData saveData)
        {
            foreach (var id in saveData.recipeID)
            {
                RecipeData recipe = DataContainer.RecipeDatas.Get(id);

                if (recipe != null) raceStatus.CollectedRecipes.Add(recipe);
            }
        }

        public override void Update()
        {
            base.Update();
            
            // 게임 플로우
            if (_gameStatus == EGameStatus.PLAY)
            {
                
            }
        }
    }
}