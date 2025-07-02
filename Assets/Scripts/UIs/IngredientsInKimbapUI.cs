using System.Collections.Generic;
using System.Linq;
using ManagerSystem;
using UIs.Base;
using UnityEngine;

namespace UIs
{
    public class IngredientsInKimbapUI : BindUI
    {
        [Tooltip("내부 위치 설정을 담고 있는 ScriptableObject")]
        public InnerPlacePositionSO Settings;
        
        private List<CollectedIngredient> _ingredients = new();
        private int _ingredientsMaxCount;

        private RectTransform _rect = null;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // SO가 할당되지 않은 경우 에러 로그 출력
            if (Settings == null)
            {
                Debug.LogWarning("SectionSettingsSO가 할당되지 않았습니다.", this);
            }
        }
#endif
        
        protected override void Initialize()
        {
            base.Initialize();
            _ingredientsMaxCount = BaseValues.MAX_COLLECTED_INGREDIENTS;
            TryGetComponent<RectTransform>(out _rect);
        }

        public void ClearIngredients()
        {
            foreach (var ingredient in _ingredients)
            {
                Managers.Resource.Destroy(ingredient.gameObject);
            }
            _ingredients.Clear();
            UpdatePosition();
        }

        public bool AddIngredient(CollectedIngredient ingredient)
        {
            if (ingredient?.gameObject == null) return false;
            if (_ingredients.Any(i => i.Data.groupId == ingredient.Data.groupId))
            {
                return false;
            }
            
            _ingredients.Add(ingredient);
            UpdatePosition();

            return true;
        }

        private void UpdatePosition()
        {
            int currentCount = _ingredients.Count;
            InnerPosition? positionInfo = Settings.GetPositionInfo(currentCount); 
            if (positionInfo.HasValue)
            {
                for (int i = 0; i < currentCount; i++)
                {
                    Vector3 ingredientPosition = positionInfo.Value.placePositions[i];
                    if (_rect)
                    {
                        if (_ingredients[i].TryGetComponent(out RectTransform target))
                        {
                            target.anchoredPosition = ingredientPosition;
                        }
                    }
                    else _ingredients[i].transform.localPosition = ingredientPosition;
                }
            }
        }
    }
}