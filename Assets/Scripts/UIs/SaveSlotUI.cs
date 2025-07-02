using System;
using Attributes;
using TMPro;
using UnityEngine.UI;
using EnumFiles;
using JsonData;
using ManagerSystem;
using UnityEngine.Events;

namespace UIs
{
    public class SaveSlotUI : BaseSaveSlotUI
    {
        [Bind("NameText")] private TextMeshProUGUI nameText;
        [Bind("TimeText")] private TextMeshProUGUI timeText;
        [Bind("RoundText")] private TextMeshProUGUI roundText;
        [Bind("DeleteButton")] private Button deleteButton;
        
        public override void Setup(SaveData inData, SaveData defaultData)
        {
            base.Setup(inData, defaultData);
            if (inData == null)
            {
                nameText.text = "";
                timeText.text = "새로운 시작";
                timeText.fontSize = 45;
                roundText.text = "";
                deleteButton.gameObject.SetActive(false);
                return;
            }

            (int month, int week) inGameTime = Managers.Save.GetInGameWeek(inData);
            DateTime saveTime = Managers.Save.GetSaveTime(inData);
            
            nameText.text = SaveData.displayName;
            timeText.text = saveTime.ToString("yyyy년 MM월 dd일 hh시 mm분");
            roundText.text = $"영업 일자 : {((EMonth)inGameTime.month).ToString()}, {inGameTime.week}주차";
        }

        public void SetDeleteButtonEvent(UnityAction<SaveSlotUI> onDelete)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => onDelete(this));
        }
    }
}