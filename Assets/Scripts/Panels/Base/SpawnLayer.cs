using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using InGame;
using UnityEngine;

namespace Panels.Base
{
    public class SpawnLayer : BindUI
    {
        [SerializeField] public EPrapType PrapType;
        [SerializeField] public float MinSpace;
        [SerializeField] public float MaxSpace;
        [SerializeField] public bool autoGenerate = false;
        
        protected const float marginX = 150f;
        
        protected SortedList<float, Prap> _spawnedPraps = new SortedList<float, Prap>();

        protected override void Initialize()
        {
            base.Initialize();
            
            Prap[] currentPraps = GetComponentsInChildren<Prap>();
            foreach (Prap prap in currentPraps)
            {
                _spawnedPraps.TryAdd(prap.GetStartPosLocalX(), prap);
            }
        }

        public bool CanSpawn()
        {
            // 화면 너비 및 우측 끝단의 위치
            Vector3 screenLeftLocalPos = transform.InverseTransformPoint(ScreenScaler.CAM_LEFTSIDE);
            
            // 마지막 프랍 정보 가져옴
            (float startX, Prap prap) lastPrap = GetLastPrap();

            // 마지막 프랍의 우측끝 위치가 화면 우측끝 위치 + 최대 간격 + 마진값보다 작으면 생성 
            if (lastPrap.startX > screenLeftLocalPos.x - MaxSpace - marginX)
            {
                return true;
            }

            return false;
        }
        
        public virtual float SetPrapAndReturnRightPosX(Prap newPrap)
        {
            // 새 프랍을 검사 후 추가
            if (newPrap is null || _spawnedPraps.Values.Contains(newPrap)) return -1f;
            
            // 마지막 프랍 정보 가져옴
            (float startX, Prap prap) lastPrap = GetLastPrap();
            
            // 새 프랍의 Prap.OnSpawned()를 실행하여 생성 시 효과 및 세팅 작업을 함
            newPrap.OnSpawned();
            
            // 마지막 프랍 위치에서 최소 ~ 최대 간격을 두어 위치 상정
            float currentSpace = Random.Range(MinSpace, MaxSpace);
            float newPrapX = lastPrap.startX - newPrap.GetWidth() - currentSpace;
            
            // 상정된 위치대로 위치 이동 
            newPrap.transform.localPosition = Vector3.right * newPrapX;
            
            // 새롭게 리스트에 추가 
            _spawnedPraps.TryAdd(newPrap.GetStartPosLocalX(), newPrap);
            
            // 세팅이 끝난 새 프랍의 왼쪽 끝 x 월드 좌표를 전달
            return newPrap.GetStartPosWorldX();
        }

        protected (float startX, Prap prap) GetLastPrap()
        {
            // 화면 너비 및 좌측 끝단의 위치
            Vector3 screenrightLocalPos = transform.InverseTransformPoint(ScreenScaler.CAM_RIGHTSIDE);
            
            (float startX, Prap prap) lastPrap;
            if (_spawnedPraps is null || _spawnedPraps.Count <= 0)
            {
                // 처음 생성하는 경우 화면 좌측 끝 위치보다 최대 간격 * 2만큼 더 왼쪽부터 생성 시작
                // minSpace ~ maxSpace 중 간격을 띄고 생성하기 때문에 간격 감안해서도 화면에 시작 위치가 노출되지 않기 위함
                lastPrap = (screenrightLocalPos.x + MaxSpace, null);
            }
            else
            {
                // 마지막 프랍의 정보를 가져옴
                var lastIndex = _spawnedPraps.LastOrDefault();
                lastPrap = (lastIndex.Key, lastIndex.Value);
            }
            
            return lastPrap;
        }
    }
}