using System;
using Attributes;
using UIs.Base;
using TMPro;
using UnityEngine.UI;
using EnumFiles;
using GameDatas;
using JsonData;
using ManagerSystem;
using UnityEngine.SceneManagement;

namespace UIs
{
    public class BaseSaveSlotUI : BindUI
    {
        protected Button saveButton;
        public SaveData SaveData { get; protected set; } = null;
        public int Index;

        protected SaveData _defaultData = new SaveData(); 
        private Action<SaveData> onSaveButtonClicked;

        protected override void Initialize()
        {
            base.Initialize();
            saveButton = GetComponent<Button>();
            saveButton.onClick.AddListener(OnButtonClick);
        }
        
        public virtual void Setup(SaveData inData, SaveData defaultData)
        {
            SaveData = inData;
            _defaultData = defaultData;
        }

        public void LinkToEventOnClicked(Action<SaveData> onButtonClicked)
        {
            onSaveButtonClicked -= onButtonClicked;
            onSaveButtonClicked += onButtonClicked;
        }

        protected virtual void OnButtonClick()
        {
            // 새로운 데이터 저장
            if (SaveData == null)
            {
                SaveData newSave = SaveDatas.CloneData(_defaultData);
                newSave.saveIndex = Index;
                newSave.saveYear = DateTime.Now.Year;
                newSave.saveMonth = DateTime.Now.Month;
                newSave.saveDay = DateTime.Now.Day;
                newSave.saveHour = DateTime.Now.Hour;
                newSave.saveMinute = DateTime.Now.Minute;

                onSaveButtonClicked?.Invoke(newSave);
                
                SaveData = newSave;
            }
            
            // 게임 씬으로 진입
            TempSetGameScene(SaveData);

            // SaveData를 게임 컨트롤러에 데이터 적용
        }
        
        private void TempSetGameScene(SaveData save)
        {
            Managers.Stage.LoadSceneAsync("GameScene", save);
        }
    }
}