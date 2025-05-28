using GameDatas;

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
        
        public override void Initialize()
        {
            base.Initialize();
            
            GameStatus = EGameStatus.WAIT;
            Score = 0;
            
            CharacterStatus.Initialize();
            RaceStatus.Initialize(1f);
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
                // 속력을 높임
                RaceStatus.AddVelocity();
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
            if ( GameStatus == EGameStatus.PLAY)
                GameStatus = EGameStatus.PAUSE;
        }
        
        public void OnQuitGame()
        {
            if ( GameStatus == EGameStatus.PAUSE)
                GameStatus = EGameStatus.WAIT;
        }

        public void OnFinishedGame()
        {
            if ( GameStatus == EGameStatus.PLAY)
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