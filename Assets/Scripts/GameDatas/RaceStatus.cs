using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Panels;
using UnityEngine;

namespace GameDatas
{
    [Serializable]
    public class RaceStatus
    {
        public float Velocity { get; private set; }
        public float Time { get; private set; }
        public float TravelDistance => Velocity * Time;
        
        // 계산을 위한 값들 
        private readonly float _startVelocity = 1f; 
        private readonly float _maxVelocity = 8f;
        private readonly float _addedVelocity = 0.03f;

        public void Initialize()
        {
            Velocity = 0f;
            Time = 0f;
        }
        
        public void InitVelocity()
        {
            Velocity = _startVelocity;
        }
        
        public void AddVelocity()
        {
            Velocity += _addedVelocity;
            if (Velocity >= _maxVelocity) Velocity = _maxVelocity;
        }
        
        public void StopVelocity()
        {
            Velocity = 0f;
        }
        
        
        
        
        
        
        
        
        // 수집한 레시피, 재료 목록
        public List<RecipeData> CollectedRecipes = new();
        public List<IngredientData> CollectedIngredients = new();

        // 제시된 메뉴 
        public List<(float distance, OrderData order)> OrderedRecipes = new();
        public List<IngredientData> PlacedIngredients = new();
        
        
        private readonly float _maxSpeed = 8f;
        private readonly float _distanceUnit = 1f;
        private readonly int _maxOrderCount = 6;
        
        private Transform _targetGround;
        private Sequence _setStatusLoop;
        private Func<float, (bool isAvailable, Orderer orderer)> _availablePlacedPerson;

        public Action OnValueChanged;

        public void AddOrder()
        {
            if (IsPaused) return;
            if (OrderedRecipes.Count >= _maxOrderCount) return;
            if (_availablePlacedPerson == null) return;

            float targetPos = travelDistance + (_distanceUnit * 100f);
            
            if (OrderedRecipes.Count > 0)
            {
                OrderedRecipes.Sort((x, y) => x.distance.CompareTo(y.distance));
                (float distance, OrderData order) lastOrder = OrderedRecipes.LastOrDefault();
                targetPos = lastOrder.distance + (_distanceUnit * 50f);
            }

            // 배치 및 수정 값 반환 
            float realPositionX = targetPos / _distanceUnit;
            var result = _availablePlacedPerson(realPositionX);
            if (!result.isAvailable) return;
            
            RecipeData recipe = CollectedRecipes[UnityEngine.Random.Range(0, CollectedRecipes.Count)];
            OrderData newOrder = new (recipe, result.orderer);
            
            OrderedRecipes.Add((targetPos, newOrder));
        }

        public void ServingKimbap()
        {
            OrderedRecipes.Sort((x, y) => x.distance.CompareTo(y.distance));

            List<(float distance, OrderData order)> completed = new();
            foreach (var data in OrderedRecipes)
            {
                if (data.order.ServingKimbap(CollectedIngredients.ToArray()))
                {
                    // 김밥 날아가는 애니메이션 
                    
                    // 스코어 적용
                    Score += data.order.score;
                    completed.Add(data);
                    
                    break;
                }
            }
            
            // 완료된 오더 제거 
            completed.ForEach(o => OrderedRecipes.Remove(o));
            
            CollectedIngredients.Clear();
            
            OnValueChanged?.Invoke();
        }
    }
}