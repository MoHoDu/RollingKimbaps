using UIs;
using ManagerSystem.Base;

namespace ManagerSystem.InGame
{
    public class InGameUIManager : BaseManager
    {
        public InGameUI InGameUI { get; private set; }

        // DI
        private InGameManager _inGameManager;

        public override void Initialize(params object[] infos)
        {
            foreach (var info in infos)
            {
                if (info is InGameManager inGameManager)
                {
                    _inGameManager = inGameManager;
                    InGameUI = _inGameManager.UI.GetComponentFromCanvas<InGameUI>();
                }
            }
        }

        public override void OnStartGame()
        {
            base.OnStartGame();

            InGameUI?.SetInfoInUI(_inGameManager.Status);
        }
    }
}