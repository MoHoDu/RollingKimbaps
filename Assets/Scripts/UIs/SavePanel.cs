using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using EnumFiles;
using JsonData;
using ManagerSystem;
using UIs.Base;
using UnityEngine;
using UnityEngine.Events;

namespace UIs
{
    public class SavePanel : CanvasUI
    {
        [Bind("Content")] private RectTransform contentsBox;
        // [Bind("NewSaveData")] private BaseSaveSlotUI newGameData;

        private GameType gameType;
        private SaveData defaultData;
        private List<SaveSlotUI> slots = new List<SaveSlotUI>();
        
        public UnityAction<SaveData> OnDeleteData;
        public Func<GameType, SaveData[]> OnRefreshData;

        public override void SetInfoInUI(params object[] infos)
        {
            if (infos == null || infos.Length < 3) return;
            
            foreach (var slot in slots)
            {
                Managers.Resource.Destroy(slot.gameObject);
            }
            slots.Clear();
            
            // 게임 타입 세팅
            if (infos[0] is GameType inType)
                gameType = inType;
            
            // 디폴트 데이터 세팅
            if (infos[1] is SaveData saveData)
                defaultData = saveData;

            // 유저 데이터 세팅
            if (infos[2] is SaveData[] { Length: > 0 } saveList)
            {
                // 데이터들을 하나씩 돌면서 새로운 슬롯을 생성하여 적용
                for (int i = 0; i < saveList.Length; i++)
                {
                    if (saveList[i] != null)
                        saveList[i].saveIndex = i;
                    AddSaveSlotUI(saveList[i]);
                }

                for (int i = 0; i < slots.Count; i++)
                {
                    SaveSlotUI s = slots[i];
                    s.Index = i;
                }
            }
        }

        public void AddEventOnDeleteData(UnityAction<SaveData> onDelete)
        {
            this.OnDeleteData = onDelete;
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
            slotUI.Setup(newData, defaultData);
            slotUI.LinkToEventOnClicked((save) =>
            {
                Managers.Save.SaveData(save);
                RefreshDatas();
            });
            slotUI.SetDeleteButtonEvent(RemoveSaveSlot);
            
            // 슬롯 리스트에 넣기 
            slots.Add(slotUI);
        }

        private void RemoveSaveSlot(SaveSlotUI slot)
        {
            // slots.Remove(slot);
            OnDeleteData?.Invoke(slot.SaveData);
            RefreshDatas();
        }

        private void RefreshDatas()
        {
            SaveData[] saveDatas = OnRefreshData?.Invoke(gameType);
            SetInfoInUI(gameType, defaultData, saveDatas);
        }
    }
}