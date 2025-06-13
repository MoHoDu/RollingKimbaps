using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using EnumFiles;
using GameDatas;
using InGame;
using ManagerSystem.InGame;
using Panels.Base;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;

namespace ManagerSystem
{
    public class InGameManager : BaseManager
    {
        // 매니저 필드
        public StatusManager Status { get; private set; } = new();
        public FlowManager Flow { get; private set; } = new();
        public PrapManager Prap { get; private set; } = new();
        public CombinationManager Combination { get; private set; } = new();
        
        private HashSet<IBaseManager> _managers = new();
        
        private EGameStatus _gameStatus => Status.GameStatus;

        // 캐릭터
        private CharacterHandler characterHandler;
        
        // DI
        public StageManager stageManager { get; private set; }
        public InputManager inputManager { get; private set; }
        public ResourceManager resourceManager { get; private set; }
        public UIManager uiManager { get; private set; }
        
        // 정해진 시간마다 반복적으로 실행할 로직
        private Sequence _tickSequence;
        private readonly float _tickTime = 1f;

        public override void Initialize(params object[] datas)
        {
            base.Initialize(datas);

            ScreenScaler.Initialize();

            stageManager = Managers.Stage;
            resourceManager = Managers.Resource;
            uiManager = Managers.UI;
            
            stageManager.AddEventAfterSceneOpened("GameScene", (_) => InitManagers());
        }

        public async void InitManagers()
        {
            inputManager = InputManager.Instance;
            
            Status.Initialize();

            FlowLayer[] flowLayers = stageManager?.FindFlowLayers();
            Flow.Initialize(this, flowLayers);
            
            SpawnLayer[] spawnLayers = stageManager?.FindSpawnLayers();
            Prap.Initialize(this, spawnLayers);
            
            Combination.Initialize(Prap, Status);

            // 매니저 리스트에 추가
            _managers.Add(Status);
            _managers.Add(Flow);
            _managers.Add(Prap);
            _managers.Add(Combination);

            await UniTask.WaitForSeconds(1);
            OnStartGame();
        }

        private bool CreateCharacter()
        {
            PrapDatas? characterPraps = DataContainer.Praps.Get(EPrapType.CHARACTER);
            if (characterPraps.HasValue)
            {
                PrapData characterPrap = characterPraps.Value.GetFirstOrNull();
                if (characterPrap is null) return false;

                Prap _prap = Prap.CreatePrap(characterPrap, new Vector3(0, 0, 0), uiManager.InGamePanel.transform);
                characterHandler = _prap.GetOrAddComponent<CharacterHandler>();
                
                // 초기화 
                characterHandler.Initialize(this);
                
                // 이벤트 연결 
                inputManager.AddEvent(EInputType.JUMP, characterHandler.InputJumpKey);
                characterHandler.OnDeath -= Status.CharacterStatus.OnDied;
                characterHandler.OnDeath += Status.CharacterStatus.OnDied;
                characterHandler.OnRevive -= Status.CharacterStatus.OnRevived;
                characterHandler.OnRevive += Status.CharacterStatus.OnRevived;
                
                // 다른 컴포넌트에 DI
                Combination.SetHandler(characterHandler);
                
                return true;
            }
            
            return false;
        }

        public override void OnStartGame()
        {
            if (_gameStatus is not EGameStatus.WAIT) return;
            
            // 캐릭터 생성 
            if (characterHandler is null && !CreateCharacter())
            {
                return;
            }

            // 시작 함수 차례로 실행 
            foreach (var manager in _managers)
            {
                if (manager is StatusManager state) 
                    state.OnStartGame(_tickTime);
                else 
                    manager.OnStartGame();
            }
            this.characterHandler?.OnStartGame();
            
            // 틱 타임 간격으로 계속 실행되는 시퀀스 생성 
            _tickSequence = DOTween.Sequence()
                .AppendInterval(_tickTime)
                .AppendCallback(() =>
                {
                    if (_gameStatus is EGameStatus.PLAY)
                    {
                        foreach (var manager in _managers)
                        {
                            manager.Tick();
                        }
                    }
                    else if (_gameStatus is EGameStatus.WAIT or EGameStatus.RESULT)
                    {
                        _tickSequence?.Kill();
                    }
                })
                .SetLoops(-1);
        }

        public void StopGame()
        {
            
        }

        public override void FixedUpdate()
        {
            characterHandler?.FixedUpdate();
        }
    }
}