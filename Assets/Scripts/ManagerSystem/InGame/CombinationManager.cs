using System;
using System.Collections.Generic;
using EnumFiles;
using InGame.Combination;
using Panels;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace ManagerSystem.InGame
{
    public class CollectionInfo
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
        public CollectionInfo CurrentCollection = new();
        
        // DI
        private PrapManager _prapManager;
        private StatusManager _statusManager;
        
        // default values
        private float _lastCheckedDistance = -100f;
        private const float READ_RECIPE_SPACE = 50f;

        public override void Initialize(params object[] datas)
        {
            base.Initialize(datas);
            Order.Initialize();

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
                    CurrentCollection.AddRecipe(recipe);
            }
        }

        /// <summary>
        /// 재료 수집 시에 수집된 재료 프리팹을 생성 및 반환
        /// </summary>
        /// <param name="inData">재료 데이터</param>
        /// <returns>생성된 재료 프리팹 or null</returns>
        public CorrectedIngredient GetIngredientInner(IngredientData inData)
        {
            string innerPath = inData.innerPath;
            var go = Managers.Resource.Instantiate(innerPath);
            CorrectedIngredient ingredient = null;
            go.TryGetComponent<CorrectedIngredient>(out ingredient);
            if (ingredient == null)
            {
                Managers.Resource.Destroy(go);
                return null;
            }
            
            return ingredient;
        }

        public override void Tick()
        {
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
                    RecipeData newRecipe = CurrentCollection.GetNewRecipeForOrder();
                    if (newRecipe != null)
                    {
                        Order.AddOrder(newRecipe);
                    }
                }

                _lastCheckedDistance = currentDistance;
            }
            
            // 오더를 확인하여 재료 배치
        }
    }
}