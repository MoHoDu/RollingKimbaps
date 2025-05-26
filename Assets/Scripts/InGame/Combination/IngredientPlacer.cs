using UnityEngine;

namespace InGame.Combination
{
    public class IngredientPlacer
    {
        Transform _ingredientParent;
        
        public void Initialize(Transform ingredientParent)
        {
            
        }

        protected void PlaceIngredient(IngredientData ingredientData)
        {
            // IngredientData.placedPos를 가지고 parent의 로컬 포지션 화 하여 배치
            
            // IngredientData의 이름으로 PrapData 가져오기
            
            // PrapManager.Instance를 사용하여 Prap 생성
            
            // 부모 세팅 후 worldpos to localpos 하여 위치를 잡아줌
            
        }
    }
}