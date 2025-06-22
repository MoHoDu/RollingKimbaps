using Cysharp.Threading.Tasks;
using EnumFiles;
using GameDatas;
using ManagerSystem.InGame;
using Panels.Base;
using Panels.Spawn;

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

                int generateCount = obstacleSpawnLayer.CalculateObstacleCount(raceStatus.Velocity, raceStatus.MaxVelocity);
                float newPrapRightX = _spawnBaseX;
                for (int i = 0; i < generateCount; i++)
                {
                    Prap newPrap = GetPrap(travelDistance);
                    newPrapRightX = _targetLayer.SetPrapAndReturnRightPosX(newPrap, raceStatus);

                    if (!obstacleSpawnLayer.CanGenerateObstacleOnGround()) break;

                    await UniTask.Yield();
                }
                return newPrapRightX;
            }
            else return _spawnBaseX;
        }
    }
}