using System.Collections.Generic;
using Attributes;
using Panels.Base;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EnumFiles;
using JsonData;
using ManagerSystem;
using UnityEngine;

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

        private void LoadSaveFiles(GameType gameType)
        {
            SaveData defaultData = DataContainer.SaveFiles.GetDefaultData(gameType);
            SaveData[] saveDatas = DataContainer.SaveFiles.Get(gameType);

            SavePanel panel = CanvasManager.Instance.GetUI<SavePanel>();
            if (panel != null)
            {
                panel.SetInfoInPanel(gameType, defaultData, saveDatas);
            }
            else
            {
                panel = CanvasManager.Instance.AddCanvasUI<SavePanel>("SavePanel", null, gameType, defaultData, saveDatas);
                panel.AddEventOnDeleteData((data) =>
                {
                    bool result = Managers.Save.RemoveData(data);
                    string resultText = result ? "성공!!" : "실패...";
                    Debug.Log($"세이브 데이터 제거 {resultText}");
                });
                panel.OnRefreshData = DataContainer.SaveFiles.Get;
            }
        }
    }
}