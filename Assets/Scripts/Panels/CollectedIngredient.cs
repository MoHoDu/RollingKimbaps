using Panels.Base;

namespace Panels
{
    public class CollectedIngredient : BindUI
    {
        public IngredientData Data;

        public CollectedIngredient(IngredientData inData)
        {
            Data = inData;
        }
    }
}