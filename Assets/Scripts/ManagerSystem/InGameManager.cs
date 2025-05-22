using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EnumFiles;
using GameDatas;
using JsonData;
using Panels;
using UnityEngine;
using Utils;

namespace ManagerSystem
{
    [Serializable]
    public class InGameStatus
    {
        // 게임 정보
        public bool IsPlaying { get; private set; } = false;
        public bool IsPaused { get; private set; } = false;
        
        // 플레이어 정보
        public int Score;
        public int Life;
        public float travelDistance;
        
        // 속력에 대한 값들
        public float Velocity;
        public float addedSpeed;
        public float flowSpeed;

        // 수집한 레시피, 재료 목록
        public List<RecipeData> CollectedRecipes = new();
        public List<IngredientData> CollectedIngredients = new();

        // 제시된 메뉴 
        public List<(float distance, OrderData order)> OrderedRecipes = new();
        public List<IngredientData> PlacedIngredients = new();
        
        // 계산을 위한 값들 
        private readonly float _maxSpeed = 8f;
        private readonly float _distanceUnit = 1f;
        private readonly int _maxOrderCount = 6;
        
        private Transform _targetGround;
        private Sequence _setStatusLoop;
        private Func<float, (bool isAvailable, Orderer orderer)> _availablePlacedPerson;

        public Action OnValueChanged;
        
        public void ClearData()
        {
            IsPlaying = false;
            IsPaused = false;
            
            Score = 0;
            Life = 5;
            travelDistance = 0;
            
            Velocity = 0;
            addedSpeed = 0.02f;
            flowSpeed = 8f;
            
            //CollectedIngredients.Clear();

            OnValueChanged = null;
        }

        public void SetFunction(Func<float, (bool isAvailable, Orderer orderer)> availablePlacedPerson)
        {
            _availablePlacedPerson = availablePlacedPerson;
        }

        public void StartGame(Transform groundTr)
        {
            IsPlaying = true;
            IsPaused = false;
            
            _targetGround = groundTr;

            InitVelocity();

            _setStatusLoop = DOTween.Sequence()
                .AppendInterval(1f)
                .AppendCallback(() =>
                {
                    if (Life <= 0 || !IsPlaying)
                    {
                        _setStatusLoop?.Kill();
                        return;
                    }
                    
                    // 속도 가속화
                    AddVelocity();
                    
                    // 이동 거리 계산 
                    CalculateDistance();
                    
                    // 오더 추가 
                    AddOrder();
                })
                .SetLoops(-1);
        }

        public void StopVelocity()
        {
            Velocity = 0;
        }

        public void InitVelocity()
        {
            Velocity = 2f;
        }

        public void AddVelocity()
        {
            if (IsPaused) return;
            if (Velocity == 0) return;
            
            Velocity += addedSpeed;
            if (Velocity > _maxSpeed) Velocity = _maxSpeed;
        }

        public void CalculateDistance()
        {
            if (IsPaused) return;

            travelDistance = _targetGround.transform.position.x * _distanceUnit;
        }
        
        public float GetFlowSpeed() => Velocity * flowSpeed;

        public void AddOrder()
        {
            if (IsPaused) return;
            if (OrderedRecipes.Count >= _maxOrderCount) return;
            if (_availablePlacedPerson == null) return;

            float targetPos = travelDistance + (_distanceUnit * 100f);
            
            if (OrderedRecipes.Count > 0)
            {
                OrderedRecipes.Sort((x, y) => x.distance.CompareTo(y.distance));
                (float distance, OrderData order) lastOrder = OrderedRecipes.LastOrDefault();
                targetPos = lastOrder.distance + (_distanceUnit * 50f);
            }

            // 배치 및 수정 값 반환 
            float realPositionX = targetPos / _distanceUnit;
            var result = _availablePlacedPerson(realPositionX);
            if (!result.isAvailable) return;
            
            RecipeData recipe = CollectedRecipes[UnityEngine.Random.Range(0, CollectedRecipes.Count)];
            OrderData newOrder = new (recipe, result.orderer);
            
            OrderedRecipes.Add((targetPos, newOrder));
        }

        public void ServingKimbap()
        {
            OrderedRecipes.Sort((x, y) => x.distance.CompareTo(y.distance));

            List<(float distance, OrderData order)> completed = new();
            foreach (var data in OrderedRecipes)
            {
                if (data.order.ServingKimbap(CollectedIngredients.ToArray()))
                {
                    // 김밥 날아가는 애니메이션 
                    
                    // 스코어 적용
                    Score += data.order.score;
                    completed.Add(data);
                    
                    break;
                }
            }
            
            // 완료된 오더 제거 
            completed.ForEach(o => OrderedRecipes.Remove(o));
            
            CollectedIngredients.Clear();
            
            OnValueChanged?.Invoke();
        }
        
        public void PauseGame()
        {
            IsPaused = true;
        }

        public void ResumeGame()
        {
            IsPaused = false;
        }
    }
    
    public class InGameManager : BaseManager
    {
        public GroundBuilder GroundBuilder => _groundBuilder;
        
        private InGameStatus _inGameStatus = new InGameStatus();
        private InputController _inputController;

        private InGamePanelController _inGamePanel; 
        private GroundBuilder _groundBuilder = new GroundBuilder();

        public void SetInputController(InputController inputController)
        {
            _inputController = inputController;
            _inputController?.Setup(_inGameStatus);
            _inputController?.InitSettings();
        }

        public void AddEventOnInput(EInputType inType, Action action)
        {
            switch (inType)
            {
                case EInputType.JUMP:
                    _inputController.OnJumped -= action;
                    _inputController.OnJumped += action;
                    break;
                case EInputType.PAUSE:
                    _inputController.OnPause -= action;
                    _inputController.OnPause += action;
                    break;
                case EInputType.RESUME:
                    _inputController.OnResume -= action;
                    _inputController.OnResume += action;
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
            await UniTask.WaitUntil(() => _inputController != null);
            
            // 초기화 
            _inGameStatus.ClearData();
            _inputController?.InitSettings();
            
            _inGamePanel = controller;
            _inGamePanel.Setup(_inGameStatus);
            
            // key settings
            AddEventOnInput(EInputType.JUMP, _inGamePanel.characterPanel.OnJump);
            
            _inGameStatus.StartGame(groundTr);
        }

        public void SetData(SaveData saveData)
        {
            foreach (var id in saveData.recipeID)
            {
                RecipeData recipe = DataContainer.RecipeDatas.Get(id);

                if (recipe != null) _inGameStatus.CollectedRecipes.Add(recipe);
            }
        }
    }
}