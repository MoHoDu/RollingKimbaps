using System.Collections.Generic;
using System.Linq;
using ManagerSystem;
using UIs.Base;
using UnityEngine;
using Attributes;
using UnityEngine.UI;

namespace UIs
{
    public class IngredientsInKimbapUI : BindUI
    {
        [Bind("Preset")] private SpriteRenderer _presetSpriteRenderer;
        [Bind("Preset")] private Image _presetImage;

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

        private bool SetPresetOnCanvas(RecipeData recipeData)
        {
            if (_presetImage == null)
            {
                Debug.LogWarning("Preset Image is not assigned.", this);
                Initialize();
            }

            // 프리셋을 가지고 와서 적용 
            _presetImage.color = Color.white;
            SpriteRenderer sr = null;
            var obj = Resources.Load<GameObject>($"Prefabs/Ingredients/Inner/Presets/{recipeData.groupId}");
            obj?.TryGetComponent(out sr);
            if (sr != null)
            {
                _presetImage.sprite = sr.sprite;
                int currentCount = _ingredients.Count;
                for (int i = 0; i < currentCount; i++)
                {
                    if (_rect && _ingredients[i].TryGetComponent(out Image target))
                    {
                        target.color = Color.clear;
                    }
                }

                return true;
            }

            return false;
        }

        private bool SetPreset(RecipeData recipeData)
        {
            if (_presetSpriteRenderer == null)
            {
                Debug.LogWarning("Preset Image is not assigned.", this);
                Initialize();
            }

            // 프리셋을 가지고 와서 적용 
            _presetSpriteRenderer.color = Color.white;
            SpriteRenderer sr = null;
            var obj = Resources.Load<GameObject>($"Prefabs/Ingredients/Inner/Presets/{recipeData.groupId}");
            obj?.TryGetComponent(out sr);
            if (sr != null)
            {
                _presetSpriteRenderer.sprite = sr.sprite;
                int currentCount = _ingredients.Count;
                for (int i = 0; i < currentCount; i++)
                {
                    _ingredients[i].gameObject?.SetActive(false);
                }

                return true;
            }

            return false;
        }

        private void UpdatePosition()
        {
            // 존재하는 조합이 있는지 확인
            IngredientData[] ingredientDatas = _ingredients.Select(i => i.Data).ToArray();
            RecipeData recipe = DataContainer.RecipeDatas.GetRecipeFromIngredients(ingredientDatas);
            bool setPresetSprite = false;
            if (recipe != null)
            {
                if (_presetSpriteRenderer != null)
                {
                    setPresetSprite = SetPreset(recipe);
                }
                else if (_presetImage != null)
                {
                    setPresetSprite = SetPresetOnCanvas(recipe);
                }
            }

            if (recipe == null || !setPresetSprite)
            {
                if (_presetSpriteRenderer)
                {
                    _presetSpriteRenderer.sprite = null;
                    _presetSpriteRenderer.color = Color.clear;
                }
                else if (_presetImage)
                {
                    _presetImage.sprite = null;
                    _presetImage.color = Color.clear;
                }

                int currentCount = _ingredients.Count;
                InnerPosition? positionInfo = Settings.GetPositionInfo(currentCount);
                if (positionInfo.HasValue)
                {
                    for (int i = 0; i < currentCount; i++)
                    {
                        Vector3 ingredientPosition = positionInfo.Value.placePositions[i];

                        _ingredients[i].gameObject?.SetActive(true);

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
}