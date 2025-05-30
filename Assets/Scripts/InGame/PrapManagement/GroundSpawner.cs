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
    public class GroundSpawner : PrapSpawner
    {
        public GroundSpawner(PrapManager inManager, GroundSpawnLayer targetLayer, RaceStatus raceStatus) : base(inManager, targetLayer, raceStatus)
        {
            SpawnPrapsAsManyAsPossible(0).Forget();
        }
        
        public async override UniTask<float> SpawnPrapsOnLayer(float travelDistance)
        {
            if (_targetLayer == null) return _spawnBaseX;
            
            EPrapType targetPrapType = _targetLayer.PrapType;
            PrapDatas? datas = DataContainer.Praps.Get(targetPrapType, travelDistance);
            if (datas.HasValue)
            {
                _targetPrapData = datas.Value.GetRandomOrNull();
            }

            if (_targetPrapData == null) return _spawnBaseX;

            Prap newPrap = _prapManager?.CreatePrap(_targetPrapData, _startSpawnPos, _targetLayer.transform);
            float newPrapRightX = _targetLayer.SetPrapAndReturnRightPosX(newPrap, raceStatus.Velocity, raceStatus.MaxVelocity);

            await UniTask.Yield();

            return newPrapRightX;
        }
    }
}