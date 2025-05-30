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
        protected float _targetStartX = 0f;

        protected override void Initialize()
        {
            base.Initialize();

            _targetStartX = ScreenScaler.CAM_LEFTSIDE.x - MaxSpace;
        }

        public bool CanGenerateObstacleOnGround()
        {
            (float endX, Prap prap) lastPrap = GetLastPrap();
            return _prevGroundPrap != null && lastPrap.endX + MaxSpace < _prevGroundPrap.GetRightPosLocalX(transform);
        }
        
        public override float SetPrapAndReturnRightPosX(Prap newPrap, RaceStatus raceStatus)
        {
            // 새 프랍을 검사 후 추가
            if (newPrap is null || _spawnedPraps.Values.Contains(newPrap)) return -1f;
            
            // 상태에서 필요한 값 추출 
            float curVelocity = raceStatus.Velocity;
            float maxVelocity = raceStatus.MaxVelocity;
            
            // 마지막 프랍 정보 가져옴
            (float endX, Prap prap) lastPrap = GetLastPrap();
            float groundLeftSideX = _prevGroundPrap.GetLeftPosLocalX(transform);
            
            // 마지막 프랍 위치에서 최소 ~ 최대 간격을 두어 위치 상정
            // float currentSpace = 0f;
            // if (lastPrap.endX < groundLeftSideX) currentSpace = GetCurrentSpacingOnFirstOfGround();
            // else currentSpace = GetCurrentSpacing(curVelocity, maxVelocity);
            float currentSpace = GetCurrentSpacing(curVelocity, maxVelocity);
            
            // 현재 세팅된 그라운드의 좌측 위치와 비교하여 더 우측인 x값을 사용
            float minPosX = Mathf.Max(lastPrap.endX, groundLeftSideX);
            
            // 새 프랍의 Prap.OnSpawned()를 실행하여 생성 시 효과 및 세팅 작업을 함
            newPrap.OnSpawned();
            
            // 상정된 위치대로 위치 이동 
            // 새롭게 리스트에 추가 
            PlaceNewPrap(newPrap, minPosX, currentSpace);
            UpdateSpawnedData();
            
            // 세팅이 끝난 새 프랍의 우측 끝 x 월드 좌표를 전달
            return newPrap.GetRightPosWorldX();
        }

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
        
        public int CalculateObstacleCount(float velocity, float maxVelocity)
        {
            if (_prevGroundPrap != null)
            {
                _targetStartX = _prevGroundPrap.GetRightPosWorldX() + MinSpace;
            }

            Prap nextGround = groundSpawnLayer.GetGroundPrap(_targetStartX);
            _prevGroundPrap = nextGround ?? _prevGroundPrap;
            if (nextGround == null) return 0;
            
            float width = _prevGroundPrap.GetWidth();
            int availableCount = Mathf.FloorToInt(width / MaxSpace);
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
        
        // protected float GetCurrentSpacingOnFirstOfGround()
        // {
        //     float randomSpace = Random.Range(0f, MaxSpace);
        //     return randomSpace;
        // }
    }
}