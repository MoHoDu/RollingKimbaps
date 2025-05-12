using System.Collections.Generic;
using Attributes;
using JsonData;
using ManagerSystem;
using Panels.Base;
using UnityEngine;
using UnityEngine.Events;

namespace Panels
{
    public class SavePanel : CanvasUI
    {
        [Bind("Content")] private RectTransform contentsBox;
        [Bind("NewSaveData")] private BaseSaveSlotUI newGameData;
        
        private List<SaveSlotUI> slots = new List<SaveSlotUI>();
        private UnityAction<SaveData> onDeleteData;

        public override void SetInfoInPanel(object info)
        {
            foreach (var slot in slots)
            {
                Managers.Resource.Destroy(slot.gameObject);
            }
            slots.Clear();
            
            if (info is List<SaveData> saveList && saveList.Count > 0)
            {
                bool setDefault = false;
                // 나머지 데이터들은 하나씩 돌면서 새로운 슬롯을 생성하여 적용
                for (int i = 0; i < saveList.Count; i++)
                {
                    if (saveList[i].isDefault)
                    {
                        if (!setDefault)
                        {
                            setDefault = true;
                            
                            // 새로운 게임 용 디폴트 데이터 세팅
                            SaveData defaultData = saveList[i];
                            newGameData.Setup(defaultData);
                        }
                        continue;
                    }
                    AddSaveSlotUI(saveList[i]);
                }
            }

            if (slots.Count == 0)
            {
                AddSaveSlotUI(null);
            }
        }

        public void AddEventOnDeleteData(UnityAction<SaveData> onDelete)
        {
            this.onDeleteData = onDelete;
        }

        private void AddSaveSlotUI(SaveData newData)
        {
            // 리소스에서 이름으로 UI프리팹 검색, 없다면 정지
            string uiPath = $"{BaseValues.CanvasUIDirectory}/{"SaveData"}";
            GameObject go = Managers.Resource.Instantiate(uiPath, transform);
            if (go is null)
            {
                Debug.LogWarning($"[SavePanel Warn] Fail to find prefab called by 'SaveData' in 'Resources/{uiPath}'");
                return;
            }
            
            // component 가져오기 
            SaveSlotUI slotUI = go.GetComponent<SaveSlotUI>();
            if (slotUI is null)
            {
                Debug.LogWarning("[SavePanel Warn] Fail to get component called by 'SaveSlotUI' in prefab");
                Managers.Resource.Destroy(go);
                return;
            }
            
            // 콘텐츠 UI 안에 넣기
            go.transform.SetParent(contentsBox, false);
            
            // 이름 설정
            go.name = $"SaveData_{slots.Count}";

            // 세이브 데이터 적용
            slotUI.Setup(newData);
            slotUI.SetDeleteButtonEvent(RemoveSaveSlot);
            
            // 슬롯 리스트에 넣기 
            slots.Add(slotUI);
        }

        private void RemoveSaveSlot(SaveSlotUI slot)
        {
            slots.Remove(slot);
            onDeleteData?.Invoke(slot.SaveData);
            Managers.Resource.Destroy(slot.gameObject);
        }
    }
}