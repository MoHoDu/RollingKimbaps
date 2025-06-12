using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using GameDatas;
using ManagerSystem;
using ManagerSystem.InGame;
using Panels;
using UnityEngine;

namespace InGame.Combination
{
    public class OrderSystem
    {
        public List<OrderData> Orders { get; private set; }
        
        // DI
        private PrapManager _prapManager;
        
        // 계산을 위한 정해진 값
        private readonly int _maxOrder = 6;
        private Dictionary<Rarity, float> _readyToOrderDistance;

        public bool CanCreateOrder()
        {
            return Orders.Count < _maxOrder;
        }

        public void Initialize(PrapManager prapManager)
        {
            Orders = new List<OrderData>();

            _prapManager = prapManager;

            _readyToOrderDistance = new()
            {
                { Rarity.Common, 500f },
                { Rarity.Uncommon, 800f },
                { Rarity.Rare, 1000f },
                { Rarity.Epic, 1200f },
                { Rarity.Legendary, 1500f },
            };
        }

        public void AddOrder(RecipeData newRecipe, float currentDistance)
        {
            if (Orders.Count >= _maxOrder) return;
            
            // 타겟의 위치 계산
            float posX = CalculateOrdererPositionX(newRecipe, currentDistance);
            Vector3 targetPos = new Vector3(posX, 0f, -0.5f);
            
            // 손님 프랍 생성
            OrdererPrap newOrderer = CreateNewOrderer(targetPos);
            OrderData newOrder = new OrderData(newRecipe, newOrderer, targetPos.x);
            newOrderer.OnSpawned(newOrder);

            // 데이터 추가
            Orders.Add(newOrder);
        }

        private float CalculateOrdererPositionX(RecipeData recipe, float currentDistance)
        {
            Rarity rarity = recipe.rarity;
            if (_readyToOrderDistance.TryGetValue(rarity, out float distance))
            {
                return currentDistance + distance;
            }

            return currentDistance + 800f;
        }

        private OrdererPrap CreateNewOrderer(Vector3 pos)
        {
            PrapDatas? datas = DataContainer.Praps.Get(EPrapType.ORDERER, pos.x);
            if (datas.HasValue)
            {
                PrapData data = datas.Value.GetRandomOrNull();
                if (data != null)
                {
                    Prap prap = _prapManager.CreatePrapInRealDistance(data, pos);
                    if (prap is OrdererPrap orderer)
                    {
                        return orderer;
                    }
                    
                    _prapManager.DestroyPrap(prap);
                }
            }

            return null;
        }
    }
}