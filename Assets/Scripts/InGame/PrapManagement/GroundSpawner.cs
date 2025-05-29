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
        protected RaceStatus raceStatus;
        
        public GroundSpawner(PrapManager inManager, GroundSpawnLayer targetLayer, RaceStatus raceStatus) : base(inManager, targetLayer)
        {
            this.raceStatus = raceStatus;
            SpawnPrapsOnLayer(0f).Forget();
        }
        
        public async override UniTaskVoid SpawnPrapsOnLayer(float travelDistance)
        {
            if (_targetLayer == null) return;
            _isSpawning = true;
            
            EPrapType targetPrapType = _targetLayer.PrapType;
            PrapDatas? datas = DataContainer.Praps.Get(targetPrapType, travelDistance);
            if (datas.HasValue)
            {
                _targetPrapData = datas.Value.GetRandomOrNull();
            }

            if (_targetPrapData == null) return;

            Prap newPrap = _prapManager?.CreatePrap(_targetPrapData, _startSpawnPos, _targetLayer.transform);

            if (_targetLayer is GroundSpawnLayer groundSpawnLayer)
            {
                groundSpawnLayer.SetPrapAndReturnRightPosX(newPrap, raceStatus.Velocity, raceStatus.MaxVelocity);
            }
            else
            {
                _targetLayer.SetPrapAndReturnRightPosX(newPrap);
            }

            await UniTask.Yield();

            _isSpawning = false;
        }
    }
}