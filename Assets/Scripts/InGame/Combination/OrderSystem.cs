using System.Collections.Generic;
using System.Linq;
using GameDatas;
using UnityEngine;

namespace InGame.Combination
{
    public class OrderSystem
    {
        public List<OrderData> Orders { get; private set; }
        
        // 계산을 위한 정해진 값
        private readonly int _maxOrder = 6;

        public bool CanCreateOrder()
        {
            return Orders.Count < _maxOrder;
        }

        public void Initialize()
        {
            Orders = new List<OrderData>();
        }

        public void AddOrder(RecipeData newRecipe)
        {
            if (Orders.Count >= _maxOrder) return;
            
            OrderData newOrder = new OrderData(newRecipe, null);
            Orders.Add(newOrder);
            
            Debug.Log($"새로운 오더 추가: {newOrder.recipe.displayName}");
        }
        
        protected void GetAvailableOrders()
        {
            
        }
    }
}