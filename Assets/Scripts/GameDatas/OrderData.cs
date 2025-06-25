using System;
using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using ManagerSystem;
using UIs;

namespace GameDatas
{
    public class OrderData
    {
        public RecipeData recipe;
        public OrdererPrap OrdererPrap;
        public List<EIngredientIndex> requires;
        
        public readonly int price = 0;
        public readonly float EndX;
        public uint Mask;

        public event Action OnClearOrder;
        public event Action onFailedOrder;

        public OrderData(RecipeData recipe, OrdererPrap ordererPrap, float finishPosX)
        {
            this.recipe = recipe;
            if (recipe != null) SetMask();
            this.OrdererPrap = ordererPrap;
            price = recipe.price;
            requires = recipe.requiredIngredients;
            EndX = finishPosX;
        }

        private void SetMask()
        {
            // 재료 리스트를 비트마스크로 변경
            uint required = 0;
            foreach (var ingredient in recipe.requiredIngredients)
            {
                // 1u = 00000000 00000000 00000000 00000001
                // (1u << v) : 1을 왼쪽으로 몇 번째 인덱스로 이동
                // r |= v : OR 연산 (하나라도 1이면 1)
                required |= (1u << (int)ingredient);
            }

            Mask = required;
        }

        public bool CanServe(RecipeData recipe)
        {
            return recipe.groupId == this.recipe.groupId;
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