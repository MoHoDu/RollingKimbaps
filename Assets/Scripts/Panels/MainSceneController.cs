using Attributes;
using Panels.Base;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EnumFiles;

namespace Panels
{
    public class MainSceneController : BindUI
    {
        [Bind("StartButton")] private Button startBtn;
        [Bind("InfiniteButton")] private Button infiniteBtn;
        [Bind("SettingButton")] private Button settingBtn;
        [Bind("QuitButton")] private Button quitBtn;

        protected override void Initialize()
        {
            startBtn.onClick.AddListener(TempSetGameScene);
        }

        private void TempSetGameScene()
        {
            SceneManager.LoadScene("GameScene");
        }

        private void LoadSaveFiles(GameType gameType)
        {

        }
    }
}