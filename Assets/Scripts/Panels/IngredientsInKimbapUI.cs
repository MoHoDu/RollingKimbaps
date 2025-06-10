using System.Collections.Generic;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class IngredientsInKimbapUI : BindUI
    {
        public Dictionary<int, List<Vector3>> _ingredientPositions = new();
        public List<CorrectedIngredient> _ingredients = new();
        private int _ingredientsMaxCount;      
        
        protected override void Initialize()
        {
            base.Initialize();
            _ingredientsMaxCount = BaseValues.MAX_COLLECTED_INGREDIENTS;
        }

        public void AddIngredient(CorrectedIngredient ingredient)
        {
            if (ingredient?.gameObject == null) return;
            
            _ingredients.Add(ingredient);
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            int currentCount = _ingredients.Count;
            if (_ingredientPositions.TryGetValue(currentCount, out var ingredientPositions))
            {
                for (int i = 0; i < currentCount; i++)
                {
                    Vector3 ingredientPosition = ingredientPositions[i];
                    _ingredients[i].transform.position = ingredientPosition;
                }
            }
        }
    }
}