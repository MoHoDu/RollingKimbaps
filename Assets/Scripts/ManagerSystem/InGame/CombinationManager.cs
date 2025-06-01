using System.Collections.Generic;
using GameDatas;
using InGame.Combination;

namespace ManagerSystem.InGame
{
    public class CollectionInfo
    {
        // 오더 정보 
        public List<RecipeData> OrderList = new();
        
        // 배치된 재료 정보 
        public List<IngredientData> PlacedIngredients = new();
        
        // 수집한 레시피, 재료 목록
        public List<RecipeData> CollectedRecipes = new();
        public List<IngredientData> CollectedIngredients = new();
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
    }
}