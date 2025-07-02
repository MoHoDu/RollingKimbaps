using System;
using EnumFiles;
using GameDatas;
using ManagerSystem.Base;


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

        private bool _needToInitVelocity = false;

        // event
        public event Action<int> OnScoreChanged;

        public override void Initialize()
        {
            base.Initialize();

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
                // 죽음 이후에 다시 1초 후 시작 속력으로 초기화
                if (_needToInitVelocity)
                {
                    RaceStatus.InitVelocity();
                    _needToInitVelocity = false;
                }
                // 죽은 경우에 속력을 잠시 0으로 만듦
                else if (CharacterStatus.State == ECharacterState.DIED)
                {
                    RaceStatus.StopVelocity();
                    _needToInitVelocity = true;
                }
                // 그 외의 경우 속력 높임
                else
                {
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