using System.Collections.Generic;
using Attributes;
using DG.Tweening;
using GameDatas;
using ManagerSystem;
using Panels.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Panels
{
    public class OrderUI : CanvasUI
    {
        // 오더 하나에 대한 UI
        // 오더 데이터 -> 
        // 김밥 속 inner 채우기
        // 필요 재료 옆에 나열하기
        // 재료 수집 시마다 mask를 가져와 해당하는 UI 애니메이션 효과
        // 서빙 가능 시에 김밥 UI도 애니메이션 효과
        // 서빙 완료 혹은 EndX 도달 시에 사라지는 애니메이션 및 제거

        [Bind("KimbapUI")] private RectTransform _kimbapUI;
        [Bind("Ingredients")] private RectTransform _ingredientsParent;
        [Bind("inners")] private IngredientsInKimbapUI _innerParent;
        
        private OrderData _data;
        private Dictionary<uint, Image> _ingredients = new Dictionary<uint, Image>();

        public Tween kimbapAnimation;
        public Dictionary<uint, Tween> ingredientAnimations = new Dictionary<uint, Tween>();

        public Image GetIngredientImage(uint ingredientId) => _ingredients.GetValueOrDefault(ingredientId, null);
        
        public override void SetInfoInPanel(params object[] infos)
        {
            foreach (var info in infos)
            {
                if (info is OrderData orderData)
                {
                    _data = orderData;
                    List<IngredientData> required = GetRequiredIngredients(_data);

                    SetIngredientsUI(required);
                    SetKimbapUI(required);
                    
                    orderData.OnClearOrder += () =>
                    {
                        if (kimbapAnimation != null && kimbapAnimation.IsActive())
                        {
                            kimbapAnimation.Kill();
                        }

                        foreach (var ingredient in ingredientAnimations)
                        {
                            if (ingredient.Value.IsActive())
                            {
                                ingredient.Value.Kill();
                            }
                        }

                        Close();
                    };
                }
            }
        }

#region SetIngredients
        private List<IngredientData> GetRequiredIngredients(OrderData data)
        {
            List<IngredientData> ingredients = new List<IngredientData>();
            foreach (var index in data.requires)
            {
                List<IngredientData> ingredientDatas = DataContainer.IngredientDatas.GetGroup(index);
                if (ingredientDatas == null) continue;
                
                IngredientData ingredientData = ingredientDatas[0];
                ingredients.Add(ingredientData);
            }
            
            return ingredients;
        }

        private void SetIngredientsUI(List<IngredientData> datas)
        {
            foreach (IngredientData data in datas)
            {
                uint mask = 1u << (int)data.groupId;
                if (!_ingredients.TryGetValue(mask, out _))
                {
                    string prefabPath = data.placedPath;
                    GameObject go = Managers.Resource.Instantiate(prefabPath + "UI", _ingredientsParent.transform);
                    if (go.TryGetComponent<Image>(out var ui))
                    {
                        _ingredients.Add(mask, ui);
                        if (!ingredientAnimations.TryGetValue(mask, out _))
                        {
                            Tween tw = SetIngredientAnimation(ui);
                            ingredientAnimations.Add(mask, tw);
                        }
                    }
                    else Managers.Resource.Destroy(go);
                }
            }
        }

        private void SetKimbapUI(List<IngredientData> datas)
        {
            foreach (var ingredient in datas)
            {
                GenerateInnerObj(ingredient);
            }

            kimbapAnimation = SetKimbapAnimation(_kimbapUI);
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
#endregion

        public Tween GetIngredientAnimation(uint mask)
        {
            return ingredientAnimations.GetValueOrDefault(mask);
        }

        private Tween SetKimbapAnimation(RectTransform image)
        {
            Vector2 size = image.sizeDelta;
            Tween tw = image
                .DOSizeDelta(new Vector2(120f, 120f), .3f)
                .SetEase(Ease.InQuad)
                .SetLoops(1, LoopType.Yoyo)
                .SetAutoKill(false)
                .OnPause(() =>
                {
                    image.sizeDelta = size;
                })
                .Pause();

            return tw;
        }

        private Tween SetIngredientAnimation(Image image)
        {
            Vector2 size = image.rectTransform.sizeDelta;
            Sequence tw = DOTween.Sequence()
                .Append(
                    image.rectTransform
                    .DOSizeDelta(new Vector2(100f, 100f), .3f)
                    .SetEase(Ease.InQuad))
                .Append(
                    image.rectTransform
                        .DOSizeDelta(size, .3f)
                        .SetEase(Ease.InQuad))
                .OnKill(() =>
                {
                    image.rectTransform.sizeDelta = size;
                })
                .SetAutoKill(false)
                .Pause();

            return tw;
        }
    }
}