using System.Collections.Generic;
using System.Linq;
using EnumFiles;

namespace GameDatas
{
    [DataName("Recipe", EFileType.SO)]
    public class RecipeDatas : BaseData<RecipeData>
    {
        public Dictionary<string, RecipeData> Data { get; private set; } = new Dictionary<string, RecipeData>();
        public Dictionary<HashSet<string>, RecipeData> ingredientsToRecipe { get; private set; } = new Dictionary<HashSet<string>, RecipeData>();

        protected override void Set(List<RecipeData> inList)
        {
            Data.Clear();
            Data = inList.ToDictionary(x => x.id);

            ingredientsToRecipe.Clear();
            ingredientsToRecipe = inList.ToDictionary(x => new HashSet<string>(x.requiredIngredients), x => x);
        }

        public RecipeData Get(string id)
        {
            return Data.ContainsKey(id) ? Data[id] : null;
        }
    }
}