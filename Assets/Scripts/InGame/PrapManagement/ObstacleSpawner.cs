using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EnumFiles;
using GameDatas;
using ManagerSystem;
using ManagerSystem.InGame;
using Panels.Base;
using Panels.Spawn;
using UnityEngine;

namespace InGame.PrapManagement
{
    public class ObstacleSpawner : PrapSpawner
    {
        public ObstacleSpawner(PrapManager inManager, ObstacleSpawnLayer target, RaceStatus raceStatus) : base(inManager,
            target, raceStatus)
        {
            SpawnPrapsAsManyAsPossible(0).Forget();
        }
        
        protected override async UniTask<float> SpawnPrapsOnLayer(float travelDistance)
        {
            if (_targetLayer == null) return _spawnBaseX;
            if (_targetLayer is ObstacleSpawnLayer obstacleSpawnLayer)
            {
                if (obstacleSpawnLayer.groundSpawnLayer is null)
                {
                    SpawnLayer defaultLayer = _prapManager.GetDefaultLayer(EPrapType.GROUND);
                    if (defaultLayer is GroundSpawnLayer correctLayer)
                    {
                        obstacleSpawnLayer.groundSpawnLayer = correctLayer;
                    }
                    else return _spawnBaseX;
                }
                
                List<Prap> praps = GetAllPraps(travelDistance);
                float maxWidth = 0;
                foreach (Prap prap in praps)
                {
                    prap.OnSpawned();
                    float width = prap.GetWidth();
                    if (width > maxWidth) maxWidth = width;
                    _prapManager.DestroyPrap(prap);
                }

                int generateCount =
                    obstacleSpawnLayer.CalculateObstacleCount(raceStatus.Velocity, raceStatus.MaxVelocity, maxWidth);

                float newPrapRightX = _spawnBaseX;
                for (int i = 0; i < generateCount; i++)
                {
                    Prap newPrap = GetPrap(travelDistance);
                    if (newPrap is null) return newPrapRightX;
                    if (!obstacleSpawnLayer.IsAvailableCreateOnGround(newPrap.GetWidth()))
                    {
                        newPrap.OnSpawned();
                        newPrap.OnDestroy();
                        return newPrapRightX;
                    }
                
                    newPrapRightX = _targetLayer.SetPrapAndReturnRightPosX(newPrap, raceStatus);

                    await UniTask.Yield();
                }
                return newPrapRightX;
            }
            else return _spawnBaseX;
        }

        protected List<Prap> GetAllPraps(float travelDistance)
        {
            EPrapType targetPrapType = _targetLayer.PrapType;
            PrapDatas? datas = DataContainer.Praps.Get(targetPrapType, travelDistance);
            if (!datas.HasValue) return null;

            List<Prap> praps = new List<Prap>();
            int index = 0;
            foreach (var data in datas.Value.DataList)
            {
                Prap newPrap = _prapManager?.CreatePrap(data, _startSpawnPos + (Vector3.right * index), _targetLayer.transform);
                
                if (newPrap is null) continue;
                praps.Add(newPrap);
                
                index++;
            }
            
            return praps;
        }
    }
}