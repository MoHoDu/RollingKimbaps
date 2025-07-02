using UIs.Base;

namespace UIs
{
    public class CollectedIngredient : BindUI
    {
        public IngredientData Data;
        public uint Mask;

        public CollectedIngredient(IngredientData inData)
        {
            Data = inData;
            Mask = 1u << (int)inData.groupId;
        }
    }
}