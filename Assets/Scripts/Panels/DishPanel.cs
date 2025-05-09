using Attributes;
using Panels.Base;
using UnityEngine.UI;

namespace Panels
{
    public class DishPanel : CanvasUI
    {
        [Bind("")] private Image ingredientImage;
        
        private IngredientData ingredientData;
        
        protected override void Initialize()
        {
            
        }
        
        public override void SetDataInPanel(UnityEngine.Object inData)
        {
            if (inData is IngredientData ingredient)
            {
                ingredientData = ingredient;
                ingredientImage.sprite = ingredient.icon;
            }
        }
    }
}