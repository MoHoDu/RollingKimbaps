using Cysharp.Threading.Tasks;
using EnumFiles;
using GameDatas;
using ManagerSystem;
using ManagerSystem.InGame;
using Panels.Base;
using UnityEngine;

namespace InGame.PrapManagement
{
    public class PrapSpawner
    {
        protected PrapManager _prapManager;
        
        protected SpawnLayer _targetLayer;
        protected PrapData _targetPrapData;
        protected float _lastEndPosX = 0f;
        protected bool _isSpawning = false;

        protected Vector3 _startSpawnPos = new Vector3(0, 100, 0);

        public PrapSpawner(PrapManager inManager, SpawnLayer targetLayer)
        {
            this._prapManager = inManager;
            this._targetLayer = targetLayer;
            this._lastEndPosX = ScreenScaler.CAM_LEFTSIDE.x - ScreenScaler.MARGIN;
        }

        public async virtual UniTaskVoid SpawnPrapsOnLayer(float travelDistance)
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
            _targetLayer.SetPrapAndReturnRightPosX(newPrap);
            
            await UniTask.Yield();

            _isSpawning = false;
        }

        public void Tick(float travelDistance)
        {
            if (_isSpawning || _targetLayer == null) return;
            if (_targetLayer.CanSpawn())
            {
                _isSpawning = true;
                SpawnPrapsOnLayer(travelDistance).Forget();
            }
        }
    }
}