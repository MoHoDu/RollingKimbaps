using System;
using EnumFiles;
using GameDatas;
using InGame;
using ManagerSystem.Base;
using UnityEngine;


namespace ManagerSystem.InGame
{
    public enum EGameStatus
    {
        WAIT,
        PLAY,
        PAUSE,
        RESULT,
    }

    public class StatusManager : BaseManager
    {
        public EGameStatus GameStatus { get; private set; } = EGameStatus.WAIT;
        public int Score { get; private set; } = 0;

        public CharacterStatus CharacterStatus { get; private set; } = new CharacterStatus();
        public RaceStatus RaceStatus { get; private set; } = new RaceStatus();

        private bool _needToCheckRevivePos = true;

        private Prap _reviveGround = null;

        // event
        public event Action<int> OnScoreChanged;

        // DI
        private PrapManager _prapManager;

        public override void Initialize(params object[] infos)
        {
            foreach (var info in infos)
            {
                if (info is InGameManager inGameManager)
                {
                    _prapManager = inGameManager.Prap;
                }
            }

            GameStatus = EGameStatus.WAIT;
            Score = 0;

            CharacterStatus.Initialize();
            RaceStatus.Initialize(1f);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // 이벤트 해제
            OnScoreChanged = null;
            CharacterStatus.ClearEvents();
        }

        public void GetScore(int score)
        {
            Score += score;
            OnScoreChanged?.Invoke(Score);
        }

        public override void Tick()
        {
            base.Tick();

            if (GameStatus is EGameStatus.PLAY)
            {
                // 이동 거리 계산
                RaceStatus.AddDistance();
                // 시간을 더함
                RaceStatus.AddTime();

                // 죽은 경우에 속력을 잠시 0으로 만듦
                if (CharacterStatus.State == ECharacterState.DIED)
                {
                    // 다음 리바이브 지점까지 이동 후 멈춤
                    if (_needToCheckRevivePos)
                    {
                        RaceStatus.readyToRevive = false;
                        _needToCheckRevivePos = false;
                        _reviveGround = _prapManager.GetNextRebirthPosition();

                        Debug.Log($"Revive Ground: {_reviveGround?.name}", _reviveGround);
                    }
                }
                else if (CharacterStatus.State == ECharacterState.WAITFORREVIE)
                {
                    // 리바이브 지점이 없거나 
                    // 리바이브 지점이 지나간 경우 속력을 0으로 만듦 
                    float revivePosX = _reviveGround != null ? _reviveGround.transform.TransformPoint(_reviveGround.LeftPos).x : 0;
                    Debug.Log($"Revive Position X: {revivePosX}");

                    if (_reviveGround == null || revivePosX <= 0)
                    {
                        // 일단 정지
                        RaceStatus.StopVelocity();

                        // 리바이브 준비 상태로 만듦
                        RaceStatus.readyToRevive = true;
                        _needToCheckRevivePos = true;
                        _reviveGround = null;
                    }
                }
                // 그 외의 경우 속력 높임
                else
                {
                    if (RaceStatus.Velocity <= 0f)
                    {
                        // 속력 초기화
                        RaceStatus.InitVelocity();
                    }

                    // 속력 증가
                    RaceStatus.AddVelocity();
                }
            }
        }

        #region Set Game Status
        public void OnStartGame(float tickTime = 1f)
        {
            if (GameStatus == EGameStatus.WAIT)
            {
                GameStatus = EGameStatus.PLAY;
                this.CharacterStatus.Initialize();
                this.RaceStatus.Initialize(tickTime);
            }
        }

        public void OnPauseGame()
        {
            if (GameStatus == EGameStatus.PLAY)
                GameStatus = EGameStatus.PAUSE;
        }

        public void OnResumeGame()
        {
            if (GameStatus == EGameStatus.PAUSE)
                GameStatus = EGameStatus.PLAY;
        }

        public void OnQuitGame()
        {
            if (GameStatus == EGameStatus.PAUSE)
                GameStatus = EGameStatus.WAIT;
        }

        public void OnFinishedGame()
        {
            if (GameStatus == EGameStatus.PLAY)
                GameStatus = EGameStatus.RESULT;
        }
        #endregion

        #region Set Race Status

        protected void AddVelocity()
        {

        }

        #endregion
    }
}