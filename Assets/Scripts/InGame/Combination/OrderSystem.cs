using System.Collections.Generic;
using GameDatas;

namespace InGame.Combination
{
    public class OrderSystem
    {
        public List<OrderData> Orders { get; private set; }
        
        protected Queue<OrderData> _availableOrders;
        
        
        // 계산을 위한 정해진 값
        private readonly int _maxOrder = 6;

        public void Initialize()
        {
            Orders = new List<OrderData>(_maxOrder);
        }

        protected void AddOrder()
        {
            
        }
        
        protected void GetAvailableOrders()
        {
            
        }
    }
}