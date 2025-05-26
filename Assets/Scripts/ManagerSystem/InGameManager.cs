using System;
using Cysharp.Threading.Tasks;
using EnumFiles;
using GameDatas;
using JsonData;
using ManagerSystem.InGame;
using Panels;
using Panels.Base;
using UnityEngine;
using Utils;

namespace ManagerSystem
{
    public class InGameManager : BaseManager
    {
        // 매니저 필드
        public StatusManager Status { get; private set; } = new();
        public FlowManager Flow { get; private set; } = new();
        public PrapManager Prap { get; private set; } = new();
        public CombinationManager Combination { get; private set; } = new();
        
        private EGameStatus _gameStatus => Status.GameStatus;
        
        // DI
        private StageManager _stageManager;
        private InputManager _inputManager;
        private ResourceManager _resourceManager;

        public override void Initialize(params object[] datas)
        {
            base.Initialize(datas);

            foreach (var data in datas)
            {
                if (data is StageManager stageManager)
                {
                    _stageManager = stageManager;
                }
                else if (data is InputManager inputManager)
                {
                    _inputManager = inputManager;
                }
                else if (data is ResourceManager resourceManager)
                {
                    _resourceManager = resourceManager;
                }
            }
            
            Status.Initialize();

            FlowLayer[] flowLayers = _stageManager?.FindFlowLayers();
            Flow.Initialize(flowLayers as object);
            
            SpawnLayer[] spawnLayers = _stageManager?.FindSpawnLayers();
            Prap.Initialize(_stageManager, _resourceManager, spawnLayers);
            
            Combination.Initialize(Prap);
            
            // 캐릭터 컨트롤러에 InputManager 주입
        }
        
        
        public GroundBuilder GroundBuilder => _groundBuilder;
        
        private RaceStatus raceStatus = new RaceStatus();
        private InputManager inputManager;

        private InGamePanel _inGamePanel; 
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
        public async UniTaskVoid PlayGame(InGamePanel controller, Transform groundTr)
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