using Attributes;
using Panels.Base;
using TMPro;
using UnityEngine.UI;
using EnumFiles;

namespace Panels
{
    public class BaseSaveSlotUI : BindUI
    {
        protected Button saveButton;
        public SaveData saveData { get; protected set; }

        protected override void Initialize()
        {
            base.Initialize();
            saveButton = GetComponent<Button>();
            saveButton.onClick.AddListener(OnButtonClick);
        }
        
        public virtual void Setup(SaveData inData)
        {
            saveData = inData;
        }

        protected virtual void OnButtonClick()
        {
            // 게임 씬으로 진입
            
            // SaveData를 게임 컨트롤러에 데이터 적용
        }
    }
}