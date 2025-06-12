using System;
using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using ManagerSystem;
using Panels;

namespace GameDatas
{
    public class OrderData
    {
        public RecipeData recipe;
        public OrdererPrap OrdererPrap;
        public List<EIngredientIndex> requires;
        public int score = 0;

        public event Action OnClearOrder;
        public event Action onFailedOrder;

        public OrderData(RecipeData recipe, OrdererPrap ordererPrap)
        {
            this.recipe = recipe;
            this.OrdererPrap = ordererPrap;
            score = recipe.price;
            requires = recipe.requiredIngredients;
        }

        public bool CanServe(RecipeData recipe)
        {
            return recipe.id == this.recipe.id;
        }

        public void OnServed()
        {
            OnClearOrder?.Invoke();
        }

        public void OnFailed()
        {
            onFailedOrder?.Invoke();
        }
    }
}