using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EnumFiles;
using GameDatas;
using ManagerSystem;
using ManagerSystem.InGame;
using ManagerSystem.UIs;
using UIs.Base;
using UnityEngine;
using UnityEngine.UI;


namespace UIs
{
    public class OrdersUI : CanvasUI
    {
        // 여러 개의 오더UI를 관리하는 UI

        // DI
        private CombinationManager _combinationManager;

        // 내부 OrderUI 목록
        private Dictionary<OrderData, OrderUI> _orderUIList = new Dictionary<OrderData, OrderUI>();
        private List<OrderUI> _orderUIs => _orderUIList?.Values?.ToList();

        // 정보 
        private uint _requiredIngredients = 0;
        private uint _collectionIdx = 0;

        // 애니메이션
        Coroutine _animCoroutine;
        HashSet<Tween> activeTweens = new HashSet<Tween>();
        HashSet<Tween> activeEnterTweens = new HashSet<Tween>();

        protected override void Initialize()
        {
            base.Initialize();

            OnClosedEvent += () =>
            {
                if (_animCoroutine != null)
                {
                    StopCoroutine(_animCoroutine);
                    _animCoroutine = null;
                }
            };
        }

        private void OnDestroy()
        {
            OnClosedEvent?.Invoke();
        }

        public void OnChangedReverseUI(bool isReversed)
        {
            // 등장 애니메이션이 있는 경우에는 정지
            foreach (Tween tween in activeEnterTweens)
            {
                if (tween != null && tween.IsActive())
                {
                    tween.Kill();
                }
            }

            foreach (var orderUI in _orderUIs)
            {
                orderUI.ReplaceUIPosition(isReversed);
            }
        }

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
                _orderUIList[orderData]?.Close();
                _orderUIList.Remove(orderData);
            }

            RefreshAnimations();
        }

        /// <summary>
        /// 재료 수집 목록을 보고 애니메이션을 동시 정지, 재생
        /// </summary>
        /// <param name="collection">수집된 재료 리스트</param>
        public void OnChangedCollection(HashSet<IngredientData> collection)
        {
            _collectionIdx = 0;
            foreach (var collect in collection)
            {
                _collectionIdx |= 1u << (int)collect.groupId;
            }

            RefreshAnimations();

            if (_animCoroutine == null) _animCoroutine = StartCoroutine(PlayIngredientAnimations());
        }

        private void RefreshAnimations()
        {
            for (int i = 0; i < Enum.GetValues(typeof(EIngredientIndex)).Length; i++)
            {
                uint ingredientIndex = 1u << i;

                // 재료에 해당하는 모든 tween을 가져옴 
                List<Tween> tweens = new List<Tween>();
                List<Tween> disactiveTweens = new List<Tween>();
                foreach (var orderData in _orderUIList.Keys)
                {
                    if ((orderData.Mask & ingredientIndex) != 0)
                    {
                        OrderUI orderUI = _orderUIList[orderData];
                        Tween tween = orderUI.GetIngredientAnimation(ingredientIndex);
                        if (tween != null)
                        {
                            // 오더에 불필요한 재료를 수집한 경우에는 정지 
                            uint extraCollected = _collectionIdx & ~orderData.Mask;
                            if (extraCollected == 0) tweens.Add(tween);
                            else disactiveTweens.Add(tween);
                        }
                    }
                }

                bool isOn = (_collectionIdx & ingredientIndex) != 0;

                foreach (var tween in tweens)
                {
                    // _requiredIngredients을 보면서 1인 값은 재생 목록에 추가
                    if (isOn) activeTweens.Add(tween);
                    // _requiredIngredients을 보면서 0인 값에 대한 애니메이션은 정지
                    else activeTweens.Remove(tween);
                }

                // 잘못된 재료가 들어간 오더의 애니메이션은 정지
                foreach (var tween in disactiveTweens)
                {
                    activeTweens.Remove(tween);
                }
            }
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
            newOrderUI.OnClosedEvent += () =>
            {
                if (_orderUIList.ContainsKey(order))
                {
                    _orderUIList.Remove(order);
                }
            };

            // 화면 밖에서 안으로 들어오는 애니메이션 재생
            activeEnterTweens.Add(newOrderUI.PlayEnterAnimation());
        }

        private IEnumerator PlayIngredientAnimations()
        {
            do
            {
                Tween[] previousActiveTweens = activeTweens.ToArray();
                foreach (Tween tween in previousActiveTweens)
                {
                    if (tween != null)
                    {
                        if (tween.IsActive())
                        {
                            if (tween.IsComplete()) tween.Restart();
                            else tween.Play();
                        }
                        else
                        {
                            tween.Restart();
                        }
                    }
                }

                foreach (Tween tween in previousActiveTweens)
                {
                    if (tween != null && tween.IsActive())
                    {
                        yield return tween.WaitForCompletion();
                    }
                }

                yield return new WaitForFixedUpdate();
            } while (activeTweens.Count > 0);

            _animCoroutine = null;
        }
    }
}