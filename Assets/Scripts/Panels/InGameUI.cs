using Panels.Base;
using Attributes;
using ManagerSystem.InGame;

namespace Panels
{
    public class InGameUI : CanvasUI
    {
        [Bind("SettingsUI")] private SettingsUI _settingsUI;
        [Bind("ScoreUI")] private ScoreUI _scoreUI;
        [Bind("LifeUI")] private LifeUI _lifeUI;

        protected override void Awake()
        { 
            base.Awake();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public override void SetInfoInPanel(params object[] infos)
        {
            if (!IsBindingDone)
                Awake();

            foreach (var info in infos)
            {
                if (info is StatusManager statusManager)
                {
                    // scoreUI에 상태 매니저 전달
                    _scoreUI.SetInfoInPanel(statusManager);
                    // LifeUI에 상태 매니저 전달
                    _lifeUI.SetInfoInPanel(statusManager);
                }
            }
        }
    }
}