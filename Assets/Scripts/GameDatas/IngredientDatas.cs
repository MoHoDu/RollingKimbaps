using System.Collections.Generic;
using System.Linq;

namespace GameDatas
{
    [DataName("Ingredient", true)]
    public class IngredientDatas : BaseData<IngredientData>
    {
        public Dictionary<string, IngredientData> Data { get; private set; } = new Dictionary<string, IngredientData>();
        public Dictionary<string, List<IngredientData>> GroupData { get; private set; } = new Dictionary<string, List<IngredientData>>();

        protected override void Set(List<IngredientData> inList)
        {
            Data.Clear();
            Data = inList.ToDictionary(x => x.id);

            GroupData.Clear();
            foreach (var item in inList)
            {
                if (!GroupData.ContainsKey(item.groupId))
                {
                    GroupData[item.groupId] = new List<IngredientData>();
                }
                GroupData[item.groupId].Add(item);
            }
        }

        public IngredientData Get(string id)
        {
            if (Data.TryGetValue(id, out IngredientData ingredient))
            {
                return ingredient;
            }

            return null;
        }

        public List<IngredientData> GetGroup(string groupId)
        {
            if (GroupData.TryGetValue(groupId, out List<IngredientData> group))
            {
                return group;
            }

            return null;
        }

        public bool CheckingGroup(string groupId, IngredientData ingredient)
        {
            if (GroupData.TryGetValue(groupId, out List<IngredientData> group))
            {
                return group.Contains(ingredient);
            }

            return false;
        }
    }
}