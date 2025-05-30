using System.Collections.Generic;
using GameDatas;
using InGame;
using Panels.Base;
using UnityEngine;

namespace Panels.Spawn
{
    public class ObstacleSpawnLayer : SpawnLayer
    {
        [SerializeField] public GroundSpawnLayer groundSpawnLayer;
        [SerializeField] public int minObstacleCount = 0;
        [SerializeField] public int maxObstacleCount = 3;
        
        protected Prap _prevGroundPrap;

        protected override void PlaceNewPrap(Prap newPrap, float referenceRightX, float spacing)
        {
            float newWidth = newPrap.GetWidth();

            // 새 위치 계산 (왼쪽에서 spacing만큼 떨어뜨려 배치)
            float newLocalX = referenceRightX + spacing + (newWidth / 2f);

            Vector3 newLocalPos = new Vector3(newLocalX, 0f, 0f);
            newPrap.transform.localPosition = newLocalPos;

            float newRightX = newPrap.GetRightPosLocalX(transform);
            _spawnedPraps.TryAdd(newRightX, newPrap);
        }

        public bool IsAvailableCreateOnGround(float prapWidth)
        {
            float endX = GetLastPrap().endX;
            float posX = endX + MaxSpace + (prapWidth/2f);
            float localX = _prevGroundPrap.transform.InverseTransformPoint(new Vector3(posX, 0f, 0f)).x;

            bool isAvailable = localX >= 0f;
            isAvailable = localX <= _prevGroundPrap.GetWidth();
            
            return isAvailable;
        }
        
        public int CalculateObstacleCount(float velocity, float maxVelocity, float obstacleWidth)
        {
            float targetStartX = 0f;
            if (_prevGroundPrap is null)
            {
                // 화면 너비 및 좌측 끝단의 위치
                Vector3 screenLeftLocalPos = transform.InverseTransformPoint(ScreenScaler.CAM_LEFTSIDE);
                targetStartX = screenLeftLocalPos.x - MaxSpace;
            }
            else
            {
                targetStartX = _prevGroundPrap.GetRightPosLocalX(transform) + MinSpace;
            }
                
            _prevGroundPrap = groundSpawnLayer.GetGroundPrap(targetStartX);
            if (_prevGroundPrap is null) return 0;
            
            float width = _prevGroundPrap.GetWidth();
            int availableCount = Mathf.FloorToInt(width / (obstacleWidth + MaxSpace));
            availableCount = Mathf.Min(maxObstacleCount, availableCount);
            
            velocity = velocity >= maxVelocity ? maxVelocity : velocity;
            // 최소 카운트를 구하기 위함이므로 속력이 낮을 수록 개수가 적어야 함
            float percentage = Mathf.Clamp01(velocity / maxVelocity);
            // 최소 카운트와 최대 카운트의 차를 기준으로 퍼센트 계산한 뒤 다시 최소 카운트를 추가하면 실제 랜덤으로 사용할 최소 개수가 나옴
            int minCount = Mathf.RoundToInt((availableCount - minObstacleCount) * percentage) + minObstacleCount;
            // 랜덤한 개수 설정
            int obstacleCount = Random.Range(minCount, availableCount + 1);
            
            return obstacleCount;
        }
    }
}