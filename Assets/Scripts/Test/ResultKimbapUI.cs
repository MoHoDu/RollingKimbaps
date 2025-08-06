using UIs.Base;
using Attributes;
using UnityEngine;
using ManagerSystem;
using System.Collections.Generic;

namespace UIs
{
    public class ResultKimbapUI : BindUI
    {
        [Bind("inners")] private IngredientsInKimbapUI _innerParent;

        public void SetIngredients(RecipeData recipe)
        {
            if (recipe == null) return;

            // 재료 가져오기
            List<IngredientData> ingredients = GetRequiredIngredients(recipe);
            if (ingredients == null || ingredients.Count == 0) return;

            // UI 설정
            SetKimbapUI(ingredients);
        }

        private void SetKimbapUI(List<IngredientData> datas)
        {
            foreach (var ingredient in datas)
            {
                GenerateInnerObj(ingredient);
            }
        }

        private void GenerateInnerObj(IngredientData data)
        {
            string prefabPath = data.innerPath;
            GameObject go = Managers.Resource.Instantiate(prefabPath + "UI", _innerParent.transform);
            if (go.TryGetComponent<CollectedIngredient>(out var ci))
            {
                ci.Data = data;
                if (!_innerParent.AddIngredient(ci))
                {
                    Managers.Resource.Destroy(go);
                }
            }
            else Managers.Resource.Destroy(go);
        }

        private List<IngredientData> GetRequiredIngredients(RecipeData data)
        {
            List<IngredientData> ingredients = new List<IngredientData>();
            foreach (var index in data.requiredIngredients)
            {
                List<IngredientData> ingredientDatas = DataContainer.IngredientDatas.GetGroup(index);
                if (ingredientDatas == null) continue;

                IngredientData ingredientData = ingredientDatas[0];
                ingredients.Add(ingredientData);
            }

            return ingredients;
        }
    }
}