using System.Collections.Generic;
using System.Linq;
using EnumFiles;

namespace GameDatas
{
    [DataName("Recipe", EFileType.SO)]
    public class RecipeDatas : BaseData<RecipeData>
    {
        public Dictionary<string, RecipeData> Data { get; private set; } = new Dictionary<string, RecipeData>();
        public Dictionary<long, RecipeData> DataByIngredientsMask { get; private set; } = new Dictionary<long, RecipeData>();

        public SortedList<float, List<RecipeData>> MinDistanceToRecipe { get; private set; } = new();

        protected override void Set(List<RecipeData> inList)
        {
            Data.Clear();
            Data = inList.ToDictionary(x => x.id);
            DataByIngredientsMask.Clear();
            DataByIngredientsMask = inList.ToDictionary(x => GetIngredientsMask(x), x => x);

            foreach (var item in inList)
            {
                if (MinDistanceToRecipe.TryGetValue(item.appearanceMinDistance, out var recipeList))
                {
                    recipeList.Add(item);
                }
                else
                {
                    MinDistanceToRecipe.Add(item.appearanceMinDistance, new List<RecipeData>() { item });
                }
            }
        }

        private long GetIngredientsMask(RecipeData recipe)
        {
            long mask = 0;
            foreach (var ingredient in recipe.requiredIngredients)
            {
                mask |= (1L << (int)ingredient);
            }
            return mask;
        }

        public RecipeData Get(string id)
        {
            return Data.ContainsKey(id) ? Data[id] : null;
        }

        /// <summary>
        /// 이동 거리에 추가된 최신 레시피 리스트 반환
        /// </summary>
        /// <param name="distance">현재 이동 거리</param>
        /// <returns>최신 레시피 리스트</returns>
        public List<RecipeData> Get(float distance = 0f)
        {
            // 바이너리 서치로 인덱스 찾기
            int index = MinDistanceToRecipe.Keys.ToList().BinarySearch(distance);

            // 만약 0 이하면, 값이 없어 새롭게 추가할 때의 인덱스를 가리킴
            // ex) -2 (값은 없으나 들어간다면, 2번째 인덱스에 있게 될 것
            // 즉, ~로 비트 반전 연산을 하고, 그 이전의 값을 얻기 위해 -1
            if (index < 0) index = ~index - 1;

            if (index >= 0 && index < MinDistanceToRecipe.Count)
                return MinDistanceToRecipe.Values[index];
            else
                return null;
        }

        public RecipeData GetRecipeFromIngredients(params IngredientData[] ingredients)
        {
            if (ingredients == null || ingredients.Length == 0)
                return null;

            // 재료의 비트 마스크 생성
            long ingredientsMask = 0;
            foreach (var ingredient in ingredients)
            {
                if (ingredient != null)
                {
                    ingredientsMask |= 1L << (int)ingredient.groupId;
                }
            }

            // 해당 비트 마스크에 맞는 레시피 찾기
            if (DataByIngredientsMask.TryGetValue(ingredientsMask, out var recipe))
            {
                return recipe;
            }
            else
            {
                // 해당 비트 마스크에 맞는 레시피가 없으면 null 반환
                return null;
            }
        }
    }
}