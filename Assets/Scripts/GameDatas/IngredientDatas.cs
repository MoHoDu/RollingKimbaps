using System.Collections.Generic;
using System.Linq;
using EnumFiles;

namespace GameDatas
{
    [DataName("Ingredient", EFileType.SO)]
    public class IngredientDatas : BaseData<IngredientData>
    {
        public Dictionary<string, IngredientData> Data { get; private set; } = new Dictionary<string, IngredientData>();
        public Dictionary<EIngredientIndex, List<IngredientData>> GroupData { get; private set; } = new Dictionary<EIngredientIndex, List<IngredientData>>();

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

        public List<IngredientData> GetGroup(EIngredientIndex groupId)
        {
            if (GroupData.TryGetValue(groupId, out List<IngredientData> group))
            {
                return group;
            }

            return null;
        }

        public bool CheckingGroup(EIngredientIndex groupId, IngredientData ingredient)
        {
            if (GroupData.TryGetValue(groupId, out List<IngredientData> group))
            {
                return group.Contains(ingredient);
            }

            return false;
        }
    }
}