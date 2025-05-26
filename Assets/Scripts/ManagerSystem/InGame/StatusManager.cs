using DG.Tweening;
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
        
        // 플레이 중에 반복 실행되는 시퀀스
        private Sequence _onPlayingSequence;
        
        // 계산을 위해 정해진 수치값
        

        public override void Initialize()
        {
            base.Initialize();
            
            GameStatus = EGameStatus.WAIT;
        }

        #region Set Game Status
        public void OnStartGame()
        {
            if ( GameStatus == EGameStatus.WAIT)
                GameStatus = EGameStatus.PLAY;

            if (_onPlayingSequence is not null)
            {
                _onPlayingSequence?.Kill();
            }
            
            _onPlayingSequence = DOTween.Sequence()
                .AppendInterval(1f)
                .AppendCallback(() =>
                {
                    if (CharacterStatus.Life <= 0 || GameStatus == EGameStatus.RESULT)
                    {
                        _onPlayingSequence?.Kill();
                        return;
                    }
                    
                    // 속도 가속화
                    RaceStatus.AddVelocity();
                })
                .SetLoops(-1);
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