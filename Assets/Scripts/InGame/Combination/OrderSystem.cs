using System;
using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using GameDatas;
using ManagerSystem;
using ManagerSystem.InGame;
using ManagerSystem.UIs;
using UIs;
using UnityEngine;


namespace InGame.Combination
{
    public class OrderSystem
    {
        public List<OrderData> Orders { get; private set; }

        // DI
        private PrapManager _prapManager;

        // 계산을 위한 정해진 값
        private readonly int _maxOrder = 4;
        private Dictionary<Rarity, float> _readyToOrderDistance;

        private event Action<List<OrderData>> onChangedOrders;

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

        public OrdersUI SetOrdersUI()
        {
            OrdersUI ui = CanvasManager.Instance?.GetUIOrCreateUI<OrdersUI>();
            AddEventListenerOnOrder(ui.CheckNewOrderList);

            return ui;
        }

        public void AddEventListenerOnOrder(Action<List<OrderData>> listener)
        {
            onChangedOrders -= listener;
            onChangedOrders += listener;
        }

        public void AddOrder(RecipeData newRecipe, float currentDistance)
        {
            if (!CanCreateOrder()) return;

            // 타겟의 위치 계산
            float posX = CalculateOrdererPositionX(newRecipe, currentDistance);
            Vector3 targetPos = new Vector3(posX, 0f, -0.5f);

            // 손님 프랍 생성
            OrdererPrap newOrderer = CreateNewOrderer(targetPos);
            OrderData newOrder = new OrderData(newRecipe, newOrderer, targetPos.x);
            newOrderer.OnSpawned(newOrder);

            // 데이터 추가
            Orders.Add(newOrder);

#if UNITY_EDITOR || DEBUG_SERVING
            Debug.Log($"[SERVING] OrderSystem.AddOrder completed. New order added for recipe: {newRecipe.name}, Total orders: {Orders.Count}");
#endif
            onChangedOrders?.Invoke(Orders);
#if UNITY_EDITOR || DEBUG_SERVING
            Debug.Log($"[SERVING] OrderSystem.onChangedOrders event invoked with {Orders.Count} orders");
#endif
        }

        public bool Serving(RecipeData recipe)
        {
#if UNITY_EDITOR || DEBUG_SERVING
            Debug.Log($"[SERVING] OrderSystem.Serving called for recipe: {(recipe != null ? recipe.name : "null")}, Current orders count: {Orders.Count}");
#endif
            OrderData target = null;
            foreach (OrderData order in Orders)
            {
#if UNITY_EDITOR || DEBUG_SERVING
                Debug.Log($"[SERVING] Checking order: {order.recipe.name}, CanServe: {order.CanServe(recipe)}");
#endif
                if (order.CanServe(recipe))
                {
#if UNITY_EDITOR || DEBUG_SERVING
                    Debug.Log($"[SERVING] Order match found! Serving order for recipe: {order.recipe.name}");
#endif
                    order.OnServed();
                    target = order;
                    break;
                }
            }

            if (target != null)
            {
#if UNITY_EDITOR || DEBUG_SERVING
                Debug.Log($"[SERVING] Order successfully served and removed from list. Recipe: {target.recipe.name}");
#endif
                Orders.Remove(target);
#if UNITY_EDITOR || DEBUG_SERVING
                Debug.Log($"[SERVING] OrderSystem.Serving - Order removed, remaining orders: {Orders.Count}");
#endif
                onChangedOrders?.Invoke(Orders);
#if UNITY_EDITOR || DEBUG_SERVING
                Debug.Log($"[SERVING] OrderSystem.onChangedOrders event invoked after serving success");
#endif
                return true;
            }

#if UNITY_EDITOR || DEBUG_SERVING
            Debug.Log($"[SERVING] No matching order found for recipe: {(recipe != null ? recipe.name : "null")}");
#endif
            return false;
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
                        // 지형에 발이 닿을 수 있도록 y값 수정
                        prap.transform.localPosition = new Vector3(
                            prap.transform.localPosition.x,
                            0f,
                            prap.transform.localPosition.z
                        );
                        return orderer;
                    }

                    _prapManager.DestroyPrap(prap);
                }
            }

            return null;
        }
    }
}