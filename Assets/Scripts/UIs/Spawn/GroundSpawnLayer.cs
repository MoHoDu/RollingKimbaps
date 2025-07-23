using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using GameDatas;
using InGame;
using InGame.PrapManagement.Praps;
using UIs.Base;
using UnityEngine;
using Utils;

namespace UIs.Spawn
{
    public class GroundSpawnLayer : SpawnLayer
    {
        [SerializeField] public int minMiddleGroundCount = 2;
        [SerializeField] public int maxMiddleGroundCount = 5;

        protected override void Initialize()
        {
            base.Initialize();
            this.PrapType = EPrapType.GROUND;
        }

        public Prap GetRevivePrap()
        {
            if (_spawnedPraps == null || _spawnedPraps.Count == 0)
            {
                // 만약 프랍이 없다면 null 반환
                return null;
            }

            List<Prap> praps = _spawnedPraps.Values?
                .Where(prap => prap.transform != null && prap.transform.position.x >= 0)
                .OrderBy(prap => prap.transform.position.x)
                .ToList();

            if (praps != null && praps.Count > 0)
            {
                // 가장 왼쪽에 있는 프랍을 반환
                return praps.FirstOrDefault();
            }
            else
            {
                // 만약 프랍이 없다면 null 반환
                return null;
            }
        }

        public Prap GetGroundPrap(float startWorldX, bool forSetObstacles = false)
        {
            float localX = transform.InverseTransformPoint(new Vector3(startWorldX, 0, 0)).x;

            int index = _spawnedPraps.GetGreaterOrEqualIndex(localX);
            if (index < _spawnedPraps.Count)
            {
                for (int i = index; i < _spawnedPraps.Count; i++)
                {
                    float key = _spawnedPraps.Keys[i];
                    if (_spawnedPraps.TryGetValue(key, out Prap prap) && prap is not null)
                    {
                        if (prap is GroundPrap ground)
                        {
                            if (!forSetObstacles || !ground.SetObstacles)
                            {
                                ground.SetObstacles = true;
                                return ground;
                            }
                        }
                    }
                }
            }

            return null;
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

            // 새 프랍의 Prap.OnSpawned()를 실행하여 생성 시 효과 및 세팅 작업을 함
            int tileCount = GetGroundTileCount(curVelocity, maxVelocity);
            newPrap.OnSpawned(tileCount);

            // 마지막 프랍 위치에서 최소 ~ 최대 간격을 두어 위치 상정
            float currentSpace = Random.Range(MinSpace, MaxSpace);

            // 상정된 위치대로 위치 이동 
            // 새롭게 리스트에 추가
            PlaceNewPrap(newPrap, lastPrap.endX, currentSpace);
            UpdateSpawnedData();

            // 세팅이 끝난 새 프랍의 왼쪽 끝 x 월드 좌표를 전달
            return newPrap.GetRightPosWorldX();
        }

        protected int GetGroundTileCount(float velocity, float maxVelocity)
        {
            velocity = velocity >= maxVelocity ? maxVelocity : velocity;
            // 최소 카운트를 구하기 위함이므로 속력이 낮을 수록 수가 높아야 함
            float percentage = 1f - Mathf.Clamp01(velocity / maxVelocity);
            // 최소 카운트와 최대 카운트의 차를 기준으로 퍼센트 계산한 뒤 다시 최소 카운트를 추가하면 실제 랜덤으로 사용할 최소 개수가 나옴
            int minCount = Mathf.RoundToInt((maxMiddleGroundCount - minMiddleGroundCount) * percentage) + minMiddleGroundCount;

            int randomCount = Random.Range(minCount, maxMiddleGroundCount + 1);
            return randomCount;
        }
    }
}