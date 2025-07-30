using UIs;
using ManagerSystem.Base;

namespace ManagerSystem.InGame
{
    public class InGameUIManager : BaseManager
    {
        public InGameUI InGameUI { get; private set; }

        // DI
        private InGameManager _inGameManager;
        private StatusManager _statusManager;

        public override void Initialize(params object[] infos)
        {
            foreach (var info in infos)
            {
                if (info is InGameManager inGameManager)
                {
                    _inGameManager = inGameManager;
                    InGameUI = _inGameManager.UI.GetComponentFromCanvas<InGameUI>();

                    _statusManager = inGameManager.Status;
                }
            }
        }

        public override void OnStartGame()
        {
            base.OnStartGame();
            InGameUI?.SetInfoInUI(_inGameManager, _inGameManager.Status);
        }

        public override void Tick()
        {
            base.Tick();

            if (InGameUI != null)
            {
                InGameUI.OrdersUI?.SetCurrentPosX(_statusManager.RaceStatus.TravelDistance);
            }
        }
    }
}