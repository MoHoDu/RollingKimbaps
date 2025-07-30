using System;
using System.Collections.Generic;
using EnumFiles;
using InGame;
using InGame.Combination;
using UIs;
using UnityEngine;
using Utils;
using ManagerSystem.Base;


namespace ManagerSystem.InGame
{
    public class CollectedRecipeInfo
    {
        // 보유 레시피
        public HashSet<RecipeData> recipeList = new();
        // 재료 비트마스크 to 레시피 딕셔너리
        // uint 자료형 = 32개까지 재료 저장 가능
        public Dictionary<uint, RecipeData> ingredientToRecipe = new();
        // 최근 레시피 FixedCapacityStack 5개
        public FixedCapacityStack<RecipeData> recentRecipes = new(5);
        // 오더 대기 큐 : 다음 차례에 오더로 생성될 레시피 큐
        public Queue<RecipeData> waitList = new(3);

        public void AddRecipe(RecipeData recipe)
        {
            if (recipeList.Contains(recipe)) return;

            // 재료 리스트를 비트마스크로 변경
            uint required = 0;
            foreach (var ingredient in recipe.requiredIngredients)
            {
                // 1u = 00000000 00000000 00000000 00000001
                // (1u << v) : 1을 왼쪽으로 몇 번째 인덱스로 이동
                // r |= v : OR 연산 (하나라도 1이면 1)
                required |= (1u << (int)ingredient);
            }

            // 재료 to 레시피 딕셔너리에 추가
            if (ingredientToRecipe.TryAdd(required, recipe))
            {
                // 전체 보유 레시피에 추가 
                recipeList.Add(recipe);
                // 최근 추가 레시피에 추가
                recentRecipes.Push(recipe);
            }
        }

        public RecipeData GetRecipe(uint ingredient)
        {
            return ingredientToRecipe.GetValueOrDefault(ingredient, null);
        }

        public RecipeData GetNewRecipeForOrder()
        {
            if (waitList.Count <= 0)
            {
                FillWaitList();
            }

            if (waitList.Count == 0)
            {
                Debug.Log("대기열에 아무것도 없어 null 반환");
                return null;
            }

            return waitList.Dequeue();
        }

        private void FillWaitList()
        {
            Debug.Log("새롭게 대기열을 생성합니다.");

            List<RecipeData> pool = new();
            Stack<RecipeData> copied = recentRecipes.GetCopied();
            int weight = 3;
            while (copied.Count > 0)
            {
                RecipeData recipe = copied.Pop();
                for (int i = 0; i < weight; i++)
                    pool.Add(recipe);
                weight = weight == 1 ? 1 : weight - 1;
            }

            int count = Mathf.Min(3, pool.Count);
            List<RecipeData> choised = pool.GetRandomElementsShuffle(count);
            foreach (RecipeData recipe in choised)
            {
                waitList.Enqueue(recipe);
            }
        }
    }

    public class CombinationManager : BaseManager
    {
        // 컨트롤러
        public OrderSystem Order { get; private set; } = new();
        public IngredientPlacer IngredientPlacer { get; private set; } = new();

        // 수집 정보 
        private CollectedRecipeInfo _collectedRecipes = new();
        private HashSet<IngredientData> _collectedIngredients = new();
        private Dictionary<EIngredientIndex, IngredientData> _collectedIngredientType = new();

        // DI
        private PrapManager _prapManager;
        private StatusManager _statusManager;
        private CharacterHandler _handler;

        // default values
        private float _lastCheckedDistance = -100f;
        private const float READ_RECIPE_SPACE = 50f;

        // Events
        private event Action<HashSet<IngredientData>> onChangedIngredients;
        public event Action<(int rewards, int tips)> onSuccessedServing;
        public event Action onFailedServing;

        public override void Initialize(params object[] datas)
        {
            base.Initialize(datas);

            foreach (var data in datas)
            {
                if (data is PrapManager prapManager)
                {
                    _prapManager = prapManager;
                }
                else if (data is StatusManager statusManager)
                {
                    _statusManager = statusManager;
                }
            }

            Order.Initialize(_prapManager);
            IngredientPlacer.Initialize(this, _prapManager, _statusManager.RaceStatus);
        }

        public void SetHandler(CharacterHandler inHandler)
        {
            IngredientPlacer.SetHandler(inHandler);
        }

        /// <summary>
        /// 새로운 레시피 목록을 가져와 현재 보유 레시피에 추가 
        /// </summary>
        public void GetRecipeDatas()
        {
            float curDistance = _statusManager.RaceStatus.TravelDistance;
            List<RecipeData> newRecipes = DataContainer.RecipeDatas.Get(curDistance);

            if (newRecipes != null)
            {
                foreach (var recipe in newRecipes)
                    _collectedRecipes.AddRecipe(recipe);
            }
        }

        /// <summary>
        /// 재료를 수집 하였을 때에 리스트에 추가
        /// </summary>
        /// <param name="ingredient">수집한 재료 데이터</param>
        public void OnCollectedIngredient(IngredientData ingredient)
        {
            if (_collectedIngredientType.TryGetValue(ingredient.groupId, out var prev))
            {
                if (ingredient.rarity > prev.rarity)
                {
                    _collectedIngredients.Remove(prev);
                    _collectedIngredients.Add(ingredient);
                    _collectedIngredientType[ingredient.groupId] = ingredient;
                }
            }
            else
            {
                _collectedIngredients.Add(ingredient);
                _collectedIngredientType.Add(ingredient.groupId, ingredient);
            }

            onChangedIngredients?.Invoke(_collectedIngredients);
        }

        public void ClearCollectedIngredients()
        {
            _collectedIngredients.Clear();
            _collectedIngredientType.Clear();

            onChangedIngredients?.Invoke(_collectedIngredients);
        }

        public void AddListenerOnChangedIngredients(Action<HashSet<IngredientData>> action)
        {
            onChangedIngredients -= action;
            onChangedIngredients += action;
        }

        public void RemoveListenerOnChangedIngredients(Action<HashSet<IngredientData>> action)
        {
            onChangedIngredients -= action;
        }

        /// <summary>
        /// 현재 수집한 재료로 만들어지는 레시피가 있으면 반환
        /// </summary>
        /// <returns>만들어지는 레시피</returns>
        public RecipeData GetCurrentRecipe()
        {
            uint curIngredient = 0;
            foreach (var ingredient in _collectedIngredientType.Keys)
            {
                curIngredient |= (1u << (int)ingredient);
            }

            return _collectedRecipes.GetRecipe(curIngredient);
        }

        /// <summary>
        /// 수집한 재료들을 비트마스크 형식으로 반환
        /// </summary>
        /// <returns>수집한 재료에 대한 비트마스크</returns>
        public uint GetCollectedMask()
        {
            uint mask = 0;
            foreach (var ingredient in _collectedIngredients)
            {
                EIngredientIndex index = ingredient.groupId;
                mask |= (1u << (int)index);
            }

            return mask;
        }

        public void OnTryServing()
        {
            RecipeData recipe = GetCurrentRecipe();
            if (recipe != null && Order.Serving(recipe))
            {
                // 보상 제공
                int rewards = GetRewards();
                int tips = GetTips();
                _statusManager.GetScore(rewards + tips);
                // 서빙 성공 시 이벤트 호출: 보상을 UI로 송출 등
                onSuccessedServing?.Invoke((rewards, tips));
            }
            else
            {
                if (_collectedIngredientType.Count > 0) onFailedServing?.Invoke();
            }

            // 재료 삭제
            ClearCollectedIngredients();
        }

        /// <summary>
        /// 현재 재료로 조합한 레시피의 점수 반환
        /// </summary>
        /// <returns>점수</returns>
        public int GetRewards()
        {
            int rewards = 0;

            // 레시피 기본 가격
            RecipeData recipe = GetCurrentRecipe();
            rewards += recipe.price;

            return rewards;
        }

        /// <summary>
        /// 사용한 재료에 따라 추가적으로 받은 팁 계산
        /// </summary>
        /// <returns>팁</returns>
        public int GetTips()
        {
            int tips = 0;
            foreach (var ingredientType in _collectedIngredientType.Keys)
            {
                tips += (int)ingredientType;
            }
            return tips;
        }

        public override void OnStartGame()
        {
            base.OnStartGame();
            OrdersUI ordersUI = Order.SetOrdersUI();
            AddListenerOnChangedIngredients(ordersUI.OnChangedCollection);
        }

        public override void Tick()
        {
            // 만약 플레이어 상태가 죽음 or 부활 대기 상태라면
            ECharacterState curState = _statusManager.CharacterStatus.State;
            if (curState == ECharacterState.WAITFORREVIE
                || curState == ECharacterState.DIED)
            {
                // 모아둔 재료 삭제
                if (_collectedIngredients.Count != 0) ClearCollectedIngredients();
            }

            // 이동 거리를 확인 후 최소 거리 이상 갔다면 실행
            float currentDistance = _statusManager.RaceStatus.TravelDistance;
            if (currentDistance - _lastCheckedDistance >= READ_RECIPE_SPACE)
            {
                // 레시피 불러오기
                GetRecipeDatas();

                // 새로운 오더 추가 가능한지 확인
                if (Order.CanCreateOrder())
                {
                    // waitList에서 레시피를 꺼내 새로운 오더 추가
                    RecipeData newRecipe = _collectedRecipes.GetNewRecipeForOrder();
                    if (newRecipe != null)
                    {
                        Order.AddOrder(newRecipe, currentDistance);
                    }
                }

                _lastCheckedDistance = currentDistance;
            }

            // 오더를 확인하여 재료 배치
            IngredientPlacer.CheckOrderAndPlaceIngredients(Order.Orders);
        }
    }
}