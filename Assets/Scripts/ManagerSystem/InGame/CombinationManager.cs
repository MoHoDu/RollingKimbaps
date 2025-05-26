using System.Collections.Generic;
using GameDatas;
using InGame.Combination;

namespace ManagerSystem.InGame
{
    public class CombinationManager : BaseManager
    {
        // 컨트롤러
        public OrderSystem Order { get; private set; } = new();
        public IngredientPlacer IngredientPlacer { get; private set; } = new();
        
        
        
        // 수집한 레시피, 재료 목록
        public List<RecipeData> CollectedRecipes = new();
        public List<IngredientData> CollectedIngredients = new();
        
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