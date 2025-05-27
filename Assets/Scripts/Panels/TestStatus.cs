using System;
using GameDatas;
using ManagerSystem;
using ManagerSystem.InGame;
using UnityEngine;

namespace Panels
{
    public class TestStatus : MonoBehaviour
    {
        public EGameStatus gameStatus;
        public int Score;
        public CharacterStatus characterStatus;
        public RaceStatus raceStatus;

        private void Update()
        {
            if (Managers.InGame?.Status != null)
            {
                StatusManager status = Managers.InGame.Status;
                gameStatus = status.GameStatus;
                Score = status.Score;
                characterStatus = status.CharacterStatus;
                raceStatus = status.RaceStatus;
            }
        }
    }
}