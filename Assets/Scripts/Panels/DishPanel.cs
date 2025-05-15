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

        public override void SetInfoInPanel(object[] infos)
        {
            if (infos is not { Length: > 0 } || infos[0] is not IngredientData ingredient) return;
            
            ingredientData = ingredient;
            ingredientImage.sprite = ingredient.icon;
        }
    }
}