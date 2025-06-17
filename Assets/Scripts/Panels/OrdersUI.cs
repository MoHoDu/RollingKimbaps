using System.Collections.Generic;
using System.Linq;
using GameDatas;
using ManagerSystem;
using ManagerSystem.InGame;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class OrdersUI : CanvasUI
    {
        // 여러 개의 오더UI를 관리하는 UI
        
        // DI
        private CombinationManager _combinationManager;
        
        // 내부 OrderUI 목록
        private Dictionary<OrderData, OrderUI> _orderUIList = new Dictionary<OrderData, OrderUI>();
        private List<OrderUI> _orderUIs => _orderUIList?.Values?.ToList();
        
        // Animations
        private Coroutine _anim;
        private Dictionary<uint, bool> _ingredientAnims = new Dictionary<uint, bool>();
        private Dictionary<OrderUI, bool> _recipeAnims = new Dictionary<OrderUI, bool>();
        
        // 정보 
        private uint _requiredIngredients = 0;

        public void CheckNewOrderList(List<OrderData> orderList)
        {
            List<OrderData> prevOrders = _orderUIList.Keys.ToList();
            _requiredIngredients = 0;
            foreach (var orderData in orderList)
            {
                if (prevOrders.Contains(orderData))
                {
                    prevOrders.Remove(orderData);
                }
                else
                {
                    GenerateOrderUI(orderData);
                }

                _requiredIngredients |= orderData.Mask;
            }

            foreach (var orderData in prevOrders)
            {
                CanvasManager.Instance.ReleseUI(_orderUIList[orderData]);
                _orderUIList.Remove(orderData);
            }
        }

        // TODO.
        public void OnChangedCollection(HashSet<IngredientData> collection)
        {
            // _requiredIngredients을 보면서 0인 값에 대한 애니메이션은 정지
            // _requiredIngredients을 보면서 1인 값은 재생 목록에 추가
            // 재생 목록을 동시 재생
        }

        private void GenerateOrderUI(OrderData order)
        {
            OrderUI newOrderUI = CanvasManager.Instance.AddCanvasUI<OrderUI>(null, transform, order);
            newOrderUI.gameObject.name = string.IsNullOrEmpty(order?.recipe?.displayName) ? "새로운 오더" : order.recipe.displayName;
            if (newOrderUI.transform.TryGetComponent<RectTransform>(out var rect))
            {
                rect.sizeDelta = new Vector2(400f, 150f);
            }
            _orderUIList[order] = newOrderUI;
        }
    }
}