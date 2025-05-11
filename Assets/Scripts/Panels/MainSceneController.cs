using System.Collections.Generic;
using Attributes;
using Panels.Base;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EnumFiles;
using ManagerSystem;

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
            startBtn.onClick.AddListener(() => LoadSaveFiles(GameType.Story));
            infiniteBtn.onClick.AddListener(() => LoadSaveFiles(GameType.Infinite));
        }

        private void TempSetGameScene()
        {
            SceneManager.LoadScene("GameScene");
        }

        private void LoadSaveFiles(GameType gameType)
        {
            List<SaveData> saveDatas = DataContainer.SaveFiles.Get(gameType);

            SavePanel panel = CanvasManager.Instance.GetUI<SavePanel>();
            if (panel != null)
            {
                panel.SetInfoInPanel(saveDatas);
            }
            else
            {
                panel = CanvasManager.Instance.AddCanvasUI<SavePanel>("SavePanel", saveDatas);
                panel.AddEventOnDeleteData(DataContainer.SaveFiles.DeleteSaveData);
            }
        }
    }
}