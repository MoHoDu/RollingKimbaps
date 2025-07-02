using Cysharp.Threading.Tasks;
using EnumFiles;
using GameDatas;
using ManagerSystem;
using ManagerSystem.InGame;
using UnityEngine;
using UIs.Spawn;

namespace InGame.PrapManagement
{
    public class PrapSpawner
    {
        protected PrapManager _prapManager;
        
        protected SpawnLayer _targetLayer;
        protected PrapData _targetPrapData;

        protected Vector3 _startSpawnPos = new Vector3(0, 100, 0);
        protected RaceStatus raceStatus;

        protected bool _isSpawning = false;
        protected float _curSpawnX = 0f;
        protected float _spawnBaseX;

        public PrapSpawner(PrapManager inManager, SpawnLayer targetLayer, RaceStatus inRaceStatus)
        {
            this._curSpawnX = ScreenScaler.CAM_LEFTSIDE.x - ScreenScaler.MARGIN;;
            this._spawnBaseX = ScreenScaler.CAM_RIGHTSIDE.x + ScreenScaler.MARGIN;
            this._prapManager = inManager;
            this._targetLayer = targetLayer;
            this.raceStatus = inRaceStatus;
        }

        protected Prap GetPrap(float travelDistance)
        {
            EPrapType targetPrapType = _targetLayer.PrapType;
            PrapDatas? datas = DataContainer.Praps.Get(targetPrapType, travelDistance);
            if (datas.HasValue)
            {
                _targetPrapData = datas.Value.GetRandomOrNull();
            }

            if (_targetPrapData == null) return null;

            Prap newPrap = _prapManager?.CreatePrap(_targetPrapData, _startSpawnPos, _targetLayer.transform);
            
            return newPrap;
        }

        protected virtual async UniTask<float> SpawnPrapsOnLayer(float travelDistance)
        {
            if (_targetLayer == null) return _spawnBaseX;
            
            Prap newPrap = GetPrap(travelDistance);
            if (newPrap is null) return _spawnBaseX;
            
            float newPrapRightX = _targetLayer.SetPrapAndReturnRightPosX(newPrap, raceStatus);

            await UniTask.Yield();
            
            return newPrapRightX;
        }

        protected async UniTaskVoid SpawnPrapsAsManyAsPossible(float travelDistance)
        {
            _isSpawning = true;
            
            do
            { 
                _curSpawnX = await SpawnPrapsOnLayer(travelDistance);
            } while (_targetLayer?.transform != null && _curSpawnX < _spawnBaseX && _targetLayer.CanSpawn());
            
            _isSpawning = false;
        }

        public void Tick(float travelDistance)
        {
            if (_isSpawning || _targetLayer == null) return;
            if (_targetLayer.CanSpawn())
            {
                _isSpawning = true;
                SpawnPrapsAsManyAsPossible(travelDistance).Forget();
            }
        }
    }
}