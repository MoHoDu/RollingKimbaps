using System.Collections.Generic;
using System.Linq;
using ManagerSystem;
using Panels;

namespace GameDatas
{
    public class OrderData
    {
        public RecipeData recipe;
        public Orderer orderer;
        public List<IngredientData> requires;
        public int score = 0;

        public OrderData(RecipeData recipe, Orderer orderer)
        {
            this.recipe = recipe;
            this.orderer = orderer;
            score = recipe.price;
            
            requires = new List<IngredientData>();
            foreach (var ingredientId in recipe.requiredIngredients)
            {
                IngredientData ingredient = DataContainer.IngredientDatas.Get(ingredientId);
                if (ingredient != null) requires.Add(ingredient);
            }
        }

        public bool ServingKimbap(params IngredientData[] ingredients)
        {
            List<IngredientData> required_copied = requires.ToList(); 
            foreach (IngredientData ingredient in ingredients)
            {
                if (required_copied.Contains(ingredient))
                {
                    required_copied.Remove(ingredient);
                }
                else
                {
                    return false;
                }
            }

            // 성공
            if (required_copied.Count == 0)
            {
                // 메뉴판 처리 
                
                // Orderer UI 처리 
                orderer.Successed();

                return true;
            }
            
            // 실패
            return false;
        }
    }
}