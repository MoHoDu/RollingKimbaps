using System.Collections.Generic;
using EnumFiles;
using GameDatas;
using InGame.PrapManagement.Praps;
using ManagerSystem;
using ManagerSystem.InGame;
using UnityEngine;

namespace InGame.Combination
{
    public class IngredientPlacer
    {
        // DI
        private CombinationManager _combinationManager;
        private PrapManager _prapManager;
        
        private List<IngredientPrap> _activeIngredients = new List<IngredientPrap>();

        public void Initialize(CombinationManager combinationManager, PrapManager prapManager)
        {
            _combinationManager = combinationManager;
            _prapManager = prapManager;
        }

        public void CheckOrderAndPlaceIngredients(List<OrderData> orderList)
        {
            // TODO. 구현해야 함
            // Test
            
        }

        protected void PlaceIngredient(IngredientData ingredientData)
        {
            // IngredientData.prapID로 PrapData 가져오기
            PrapData data = DataContainer.Praps.Get(EPrapType.INGREDIENT, ingredientData.prapID);

            // TODO. 위치 계산
            // 임시 코드 
            Vector3 placedPosition = Vector3.right * 10;
            
            // PrapManager.Instance를 사용하여 Prap 생성
            Prap prap = _prapManager.CreatePrap(data, placedPosition);
            if (prap is IngredientPrap ingredientPrap)
            {
                // OnTrigger에 _combinationManager.OnCollectIngredient() 연결 
                ingredientPrap.OnTriggerd += _combinationManager.OnCollectedIngredient;

                // OnDestroyed에 RemoveIngredient() 연결
                ingredientPrap.OnDestroyed += RemoveIngredient;
            }
            else
            {
                _prapManager.DestroyPrap(prap);
            }
        }

        protected void RemoveIngredient(IngredientPrap ingredientPrap)
        {
            _activeIngredients.Remove(ingredientPrap);
        }
    }
}