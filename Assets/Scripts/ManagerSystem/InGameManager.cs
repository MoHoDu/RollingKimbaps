using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnumFiles;
using GameDatas;
using InGame;
using ManagerSystem.InGame;
using UIs.Base;
using Utils;
using ManagerSystem.Base;
using ManagerSystem.UIs;
using UIs.Spawn;
using Cysharp.Threading.Tasks;
using System;
using UIs.Panels.Infos;
using UIs;
using UIs.Panels.Popups;


namespace ManagerSystem
{
    public class InGameManager : BaseManager
    {
        // 매니저 필드
        public StatusManager Status { get; private set; } = new();
        public FlowManager Flow { get; private set; } = new();
        public PrapManager Prap { get; private set; } = new();
        public CombinationManager Combination { get; private set; } = new();
        public InGameUIManager GameUI { get; private set; } = new();

        private HashSet<IBaseManager> _managers = new();

        private EGameStatus _gameStatus => Status.GameStatus;

        // 캐릭터
        private CharacterHandler characterHandler;

        // DI
        public StageManager Stage { get; private set; }
        public InputManager Input { get; private set; }
        public ResourceManager Resource { get; private set; }
        public UIManager UI { get; private set; }

        // 정해진 시간마다 반복적으로 실행할 로직 
        private Coroutine _tickSequence;
        private readonly float _tickTime = 1f;

        // evnet
        public event Action<(int score, int tip)> onSuccessUIEvent;

        public override void Initialize(params object[] datas)
        {
            base.Initialize(datas);

            ScreenScaler.Initialize();

            // 매니저 재정의
            ReGenerateManagers();

            Stage = Managers.Stage;
            Resource = Managers.Resource;
            UI = Managers.UI;

            Stage.AddEventAfterSceneOpened("GameScene", (_) => InitManagers());
        }

        private void ReGenerateManagers()
        {
            _managers.Clear();

            Status = new StatusManager();
            Flow = new FlowManager();
            Prap = new PrapManager();
            Combination = new CombinationManager();
            GameUI = new InGameUIManager();

            characterHandler = null;
        }

        public async void InitManagers()
        {
            Input = InputManager.Instance;

            Status.Initialize(this);
            GameUI.Initialize(this);

            FlowLayer[] flowLayers = Stage?.FindFlowLayers();
            Flow.Initialize(this, flowLayers);

            SpawnLayer[] spawnLayers = Stage?.FindSpawnLayers();
            Prap.Initialize(this, spawnLayers);

            Combination.Initialize(Prap, Status);

            // 매니저 리스트에 추가
            _managers.Add(Status);
            _managers.Add(Flow);
            _managers.Add(Prap);
            _managers.Add(Combination);
            _managers.Add(GameUI);

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

                Prap _prap = Prap.CreatePrap(characterPrap, new Vector3(0, 0, 0), UI.InGamePanel.transform);
                characterHandler = _prap.GetComponent<CharacterHandler>();
                if (characterHandler is null) characterHandler = _prap.gameObject.AddComponent<CharacterHandler>();

                // 초기화 
                characterHandler.Initialize(this);

                // 이벤트 연결 
                Input.AddEvent(EInputType.JUMP, characterHandler.InputJumpKey);
                Input.AddEvent(EInputType.SUBMIT, characterHandler.InputSubmitKey);
                characterHandler.OnDeath -= Status.CharacterStatus.OnDied;
                characterHandler.OnDeath += Status.CharacterStatus.OnDied;
                characterHandler.OnDeath -= Combination.ClearCollectedIngredients;
                characterHandler.OnDeath += Combination.ClearCollectedIngredients;
                characterHandler.OnRevive -= Status.CharacterStatus.OnRevived;
                characterHandler.OnRevive += Status.CharacterStatus.OnRevived;
                Status.CharacterStatus.AddEventOnDeath(OnGameOver);

                // 서빙 성공 시
                Combination.onSuccessedServing -= async (reward) => await OnSuccessedServing(reward.rewards, reward.tips);
                Combination.onSuccessedServing += async (reward) => await OnSuccessedServing(reward.rewards, reward.tips);

                // 서빙 실패 시
                Combination.onFailedServing -= OnFailedServing;
                Combination.onFailedServing += OnFailedServing;

                // 다른 컴포넌트에 DI
                Combination.SetHandler(characterHandler);

                return true;
            }

            return false;
        }

        private async UniTask OnSuccessedServing(int price, int tip)
        {
            // 캐릭터 애니메이션 재생
            if (characterHandler) await characterHandler.PlayServingAnimation(true);
            // 점수 애니메이션 후 UI에 점수 추가 
            onSuccessUIEvent?.Invoke((price, tip));
        }

        private async void OnFailedServing()
        {
            // 캐릭터 애니메이션 재생
            if (characterHandler) await characterHandler.PlayServingAnimation(false);
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
            _tickSequence = CoroutineHelper.StartNewCoroutine(TickAsync());
        }

        private IEnumerator TickAsync()
        {
            do
            {
                if (_gameStatus is EGameStatus.PLAY)
                {
                    foreach (var manager in _managers)
                    {
                        manager.Tick();
                    }
                }

                yield return new WaitForSeconds(1f);
            } while (_gameStatus is EGameStatus.PLAY or EGameStatus.PAUSE);
        }

        public void PauseGame()
        {
            characterHandler?.OnPaused();

            Status.OnPauseGame();
            Flow.OnStopGame();

            // 당장 틱 함수도 중지
            if (_tickSequence != null) CoroutineHelper.StopAnyCoroutine(_tickSequence);
            _tickSequence = null;
        }

        public void ResumeGame()
        {
            Status.OnResumeGame();

            // 틱 타임 간격으로 계속 실행되는 시퀀스 생성 
            _tickSequence = CoroutineHelper.StartNewCoroutine(TickAsync());

            characterHandler?.OnResumed();
        }

        public void OnGameOver()
        {
            // 게임 상태 변경
            Status.OnFinishedGame();

            // SFX 재생
            Managers.Audio.PlayAudioFromSystem(EAudioType.SFX, EAudioSituation.Game_Over, 0, 1f);

            // 브금 재생 요청
            Managers.Audio.PlayAudioFromSystem(EAudioType.BGM, EAudioSituation.BGM_Result, 0, 0.3f);

            // 게임 결과 창 열기
            UIInfo uiInfo = new UIInfo(EButtonType.ONE_BUTTON, false, false);
            int score = Status.Score;
            ResultUI ui = Managers.UI?.AddPanel<ResultUI>(uiInfo, "ResultUI");
            ui.AddButtonAction(() =>
            {
                // 팝업 창 띄우고, 확인 시 메인 메뉴로 이동
                PopupInfo popupInfo = new PopupInfo(() =>
                {
                    Time.timeScale = 1f; // 게임 시간 재개
                    Managers.InGame?.ReturnToMainMenu();
                }, null, "메인 메뉴로 이동", "정말로 메인 메뉴로 이동하시겠습니까?", "네", "아니요");

                Managers.UI?.AddPopup<PopupUI>(popupInfo);

                // SFX 재생
                Managers.Audio?.PlayAudioFromSystem(EAudioType.SFX, EAudioSituation.System_Alert, 0, 1f);
            });
            ui?.SetScoreText(score);
        }

        public void ReturnToMainMenu()
        {
            Status.OnQuitGame();

            // 당장 틱 함수도 중지
            if (_tickSequence != null) CoroutineHelper.StopAnyCoroutine(_tickSequence);
            _tickSequence = null;

            // 모든 매니저 종료 처리
            foreach (var manager in _managers)
            {
                manager.OnDestroy();
            }

            // 매니저 재정의
            ReGenerateManagers();

            // 씬 이동 
            Managers.Stage.LoadSceneAsync("MainScene");
        }

        public override void FixedUpdate()
        {
            characterHandler?.FixedUpdate();
        }
    }
}