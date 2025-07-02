using Attributes;
using UIs.Base;
using UnityEngine.UI;

namespace UIs
{
    public class DishPanel : CanvasUI
    {
        [Bind("")] private Image ingredientImage;

        private IngredientData ingredientData;

        protected override void Initialize()
        {

        }

        public override void SetInfoInUI(object[] infos)
        {
            if (infos is not { Length: > 0 } || infos[0] is not IngredientData ingredient) return;
            
            ingredientData = ingredient;
            ingredientImage.sprite = ingredient.icon;
        }
    }
}