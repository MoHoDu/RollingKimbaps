using Attributes;
using TMPro;
using UnityEngine.UI;
using EnumFiles;
using UnityEngine.Events;

namespace Panels
{
    public class SaveSlotUI : BaseSaveSlotUI
    {
        [Bind("NameText")] private TextMeshProUGUI nameText;
        [Bind("TimeText")] private TextMeshProUGUI timeText;
        [Bind("RoundText")] private TextMeshProUGUI roundText;
        [Bind("DeleteButton")] private Button deleteButton;
        
        public override void Setup(SaveData inData)
        {
            base.Setup(inData);
            if (inData == null)
            {
                nameText.text = "";
                timeText.text = "세이브 데이터가 없습니다.";
                timeText.fontSize = 45;
                roundText.text = "";
                saveButton.interactable = false;
                deleteButton.gameObject.SetActive(false);
                return;
            }
            
            (int, int) inGameTime = saveData.inGameDate;
            
            nameText.text = saveData.displayName;
            timeText.text = saveData.saveTime.ToString("yyyy년 MM월 dd일 hh시 mm분");
            roundText.text = $"영업 일자 : {((EMonth)inGameTime.Item1).ToString()}, {inGameTime.Item2}주차";
        }

        public void SetDeleteButtonEvent(UnityAction<SaveSlotUI> onDelete)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => onDelete(this));
        }
    }
}