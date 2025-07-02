using Cysharp.Threading.Tasks;
using EnumFiles;
using GameDatas;
using ManagerSystem;
using ManagerSystem.InGame;
using UIs.Base;
using UIs.Spawn;
using UnityEngine;

namespace InGame.PrapManagement
{
    public class GroundSpawner : PrapSpawner
    {
        public GroundSpawner(PrapManager inManager, GroundSpawnLayer targetLayer, RaceStatus raceStatus) : base(inManager, targetLayer, raceStatus)
        {
            SpawnPrapsAsManyAsPossible(0).Forget();
        }
    }
}