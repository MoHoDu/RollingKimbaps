using System;
using System.Collections.Generic;
using InGame.Combination;
using Panels;
using Utils;

namespace ManagerSystem.InGame
{
    public class CollectionInfo
    {
        // 보유 레시피
        public HashSet<RecipeData> recipeList = new();
        // 재료 비트마스크 to 레시피 딕셔너리
        public Dictionary<uint, RecipeData> ingredientToRecipe = new();
        // 최근 레시피 FixedCapacityStack 5개
        public FixedCapacityStack<RecipeData> recentRecipes = new(5);
        // 오더 대기 큐 : 다음 차례에 오더로 생성될 레시피 큐
        public Queue<RecipeData> waitList = new(3);
    }
    
    public class CombinationManager : BaseManager
    {
        // 컨트롤러
        public OrderSystem Order { get; private set; } = new();
        public IngredientPlacer IngredientPlacer { get; private set; } = new();
        
        // 수집 정보 
        public CollectionInfo Collection = new();
        
        // DI
        private PrapManager _prapManager;
        
        // default values
        private const string POST_FIX_INNER = "_inner";

        public override void Initialize(params object[] datas)
        {
            base.Initialize(datas);

            foreach (var data in datas)
            {
                if (data is PrapManager prapManager)
                {
                    _prapManager = prapManager;
                }
            }
        }

        /// <summary>
        /// 재료 수집 시에 수집된 재료 프리팹을 생성 및 반환
        /// </summary>
        /// <param name="inData">재료 데이터</param>
        /// <returns>생성된 재료 프리팹 or null</returns>
        public CorrectedIngredient GetIngredientInner(IngredientData inData)
        {
            string groupName = inData.groupId;
            if (string.IsNullOrEmpty(groupName)) return null;

            Char firstLetter = Char.ToUpper(groupName[0]);
            string fileName = firstLetter + groupName.Substring(1) + POST_FIX_INNER;

            var go = Managers.Resource.Instantiate(fileName);
            CorrectedIngredient ingredient = null;
            go.TryGetComponent<CorrectedIngredient>(out ingredient);
            if (ingredient == null)
            {
                Managers.Resource.Destroy(go);
                return null;
            }
            
            return ingredient;
        }
    }
}