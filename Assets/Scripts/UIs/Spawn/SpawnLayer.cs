using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using GameDatas;
using InGame;
using UnityEngine;
using UIs.Base;

namespace UIs.Spawn
{
    public class SpawnLayer : BindUI
    {
        [SerializeField] public EPrapType PrapType;
        [SerializeField] public float MinSpace;
        [SerializeField] public float MaxSpace;
        [SerializeField] public bool autoGenerate = false;
        [SerializeField] public bool isDefaultLayer = false;
        
        protected const float marginX = 150f;
        
        protected SortedList<float, Prap> _spawnedPraps = new SortedList<float, Prap>();
        
        // 테스트 시에 확인하려고 만들어둔 필드
        [SerializeField] private List<float> _spawnedPosXs = new List<float>();
        [SerializeField] private List<Prap> _spawnedPrapObjs = new List<Prap>();

        protected override void Initialize()
        {
            base.Initialize();
            
            Prap[] currentPraps = GetComponentsInChildren<Prap>();
            foreach (Prap prap in currentPraps)
            {
                _spawnedPraps.TryAdd(prap.GetLeftPosLocalX(transform), prap);
                UpdateSpawnedData();
            }
        }

        public bool CanSpawn()
        {
            // 화면 너비 및 우측 끝단의 위치
            Vector3 screenRightLocalPos = transform.InverseTransformPoint(ScreenScaler.CAM_RIGHTSIDE);
            
            // 이미 사라진 프랍 리스트 확인
            List<float> haveToRemove = new List<float>();
            foreach (float key in _spawnedPraps.Keys)
            {
                if (_spawnedPraps.TryGetValue(key, out var prap) && prap == null)
                {
                    if (!haveToRemove.Contains(key)) haveToRemove.Add(key);
                }
            }

            // 없어진 프랍 데이터 제거 
            foreach (float key in haveToRemove) _spawnedPraps.Remove(key);
            
            // 마지막 프랍 정보 가져옴
            (float endX, Prap prap) lastPrap = GetLastPrap();

            // 마지막 프랍의 우측끝 위치가 화면 우측끝 위치 + 최대 간격 + 마진값보다 작으면 생성 
            if (lastPrap.endX < screenRightLocalPos.x + MaxSpace + marginX)
            {
                return true;
            }

            return false;
        }
        
        public virtual float SetPrapAndReturnRightPosX(Prap newPrap, RaceStatus raceStatus)
        {
            // 새 프랍을 검사 후 추가
            if (newPrap is null || _spawnedPraps.Values.Contains(newPrap)) return -1f;
            
            // 상태에서 필요한 값 추출 
            float curVelocity = raceStatus.Velocity;
            float maxVelocity = raceStatus.MaxVelocity;
            
            // 마지막 프랍 정보 가져옴
            (float endX, Prap prap) lastPrap = GetLastPrap();
            
            // 새 프랍의 Prap.OnSpawned()를 실행하여 생성 시 효과 및 세팅 작업을 함
            newPrap.OnSpawned();
            
            // 마지막 프랍 위치에서 최소 ~ 최대 간격을 두어 위치 상정
            float currentSpace = GetCurrentSpacing(curVelocity, maxVelocity);
            
            // 상정된 위치대로 위치 이동 
            // 새롭게 리스트에 추가 
            PlaceNewPrap(newPrap, lastPrap.endX, currentSpace);
            UpdateSpawnedData();
            
            // 세팅이 끝난 새 프랍의 우측 끝 x 월드 좌표를 전달
            return newPrap.GetRightPosWorldX();
        }

        protected virtual void PlaceNewPrap(Prap newPrap, float referenceRightX, float spacing)
        {
            float newWidth = newPrap.GetWidth();
            float newPivotOffset = newPrap.GetPivotToRightEdgeOffsetLocalX();

            // 새 위치 계산 (왼쪽에서 spacing만큼 떨어뜨려 배치)
            float newLocalX = referenceRightX + spacing + newWidth - newPivotOffset;

            Vector3 newLocalPos = new Vector3(newLocalX, 0f, 0f);
            newPrap.transform.localPosition = newLocalPos;

            float newRightX = newPrap.GetRightPosLocalX(transform);
            _spawnedPraps.TryAdd(newRightX, newPrap);
        }

        protected (float endX, Prap prap) GetLastPrap()
        {
            // 화면 너비 및 좌측 끝단의 위치
            Vector3 screenLeftLocalPos = transform.InverseTransformPoint(ScreenScaler.CAM_LEFTSIDE);
            
            (float endX, Prap prap) lastPrap;
            if (_spawnedPraps is null || _spawnedPraps.Count <= 0)
            {
                // 처음 생성하는 경우 화면 좌측 끝 위치보다 최대 간격 * 2만큼 더 좌측부터 생성 시작
                // minSpace ~ maxSpace 중 간격을 띄고 생성하기 때문에 간격 감안해서도 화면에 시작 위치가 노출되지 않기 위함
                lastPrap = (screenLeftLocalPos.x - MaxSpace, null);
            }
            else
            {
                // 마지막 프랍의 정보를 가져옴
                var lastIndex = _spawnedPraps.LastOrDefault();
                lastPrap = (lastIndex.Key, lastIndex.Value);
            }
            
            return lastPrap;
        }

        protected void UpdateSpawnedData()
        {
            _spawnedPosXs = _spawnedPraps?.Keys?.ToList();
            _spawnedPrapObjs = _spawnedPraps?.Values?.ToList();
        }
        
        protected float GetCurrentSpacing(float velocity, float maxVelocity)
        {
            velocity = velocity >= maxVelocity ? maxVelocity : velocity;
            // 최소 카운트를 구하기 위함이므로 속력이 낮을 수록 수가 높아야 함
            float percentage = 1f - Mathf.Clamp01(velocity / maxVelocity);
            // 최소 카운트와 최대 카운트의 차를 기준으로 퍼센트 계산한 뒤 다시 최소 카운트를 추가하면 실제 랜덤으로 사용할 최소 개수가 나옴
            float curMinSpace = ((MaxSpace - MinSpace) * percentage) + MinSpace;
            
            float randomSpace = Random.Range(curMinSpace, MaxSpace);
            return randomSpace;
        }
    }
}