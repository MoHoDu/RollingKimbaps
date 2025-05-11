using System;
using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using ManagerSystem;


namespace GameDatas
{
    [DataName("SaveFile", true)]
    public class SaveDataContainer : BaseData<SaveData>
    {
        public Dictionary<GameType, List<SaveData>> Data { get; private set; } = new();
        private Dictionary<GameType, SaveData> DefaultData = new();

        protected override void Set(List<SaveData> inList)
        {
            Data.Clear();
            DefaultData.Clear();

            foreach (var item in inList)
            {
                if (!Data.ContainsKey(item.gameType))
                {
                    Data[item.gameType] = new List<SaveData>();
                }
                Data[item.gameType].Add(item);

                if (item.isDefault && !DefaultData.ContainsKey(item.gameType))
                {
                    DefaultData.Add(item.gameType, item);
                }
            }
        }

        public List<SaveData> Get(GameType gameType)
        {
            return Data.GetValueOrDefault(gameType, new List<SaveData>());
        }

        public SaveData GetDefaultData(GameType gameType)
        {
            SaveData cloneData = new();
            cloneData.gameType = gameType;
            cloneData.saveYear = DateTime.Now.Year;
            cloneData.saveMonth = DateTime.Now.Month;
            cloneData.saveDay = DateTime.Now.Day;
            cloneData.saveHour = DateTime.Now.Hour;
            cloneData.saveMinute = DateTime.Now.Minute;
            cloneData.inGameMonth = 1;
            cloneData.inGameWeek = 1;

            SaveData defaultData = null;
            DefaultData.TryGetValue(gameType, out defaultData);

            if (defaultData != null)
            {
                cloneData.ingredientList = defaultData.ingredientList;
                cloneData.recipeList = defaultData.recipeList;
                cloneData.skillList = defaultData.skillList;
            }
            return cloneData;
        }

        public void DeleteSaveData(SaveData inData)
        {
            // 디폴트 데이터는 삭제 불가
            if (inData.isDefault) return;
            
            GameType gameType = inData.gameType;
            if (Data.TryGetValue(gameType, out _))
            {
                Data[gameType].Remove(inData);
                if (Data[gameType].Count == 0) 
                    Data.Remove(gameType);
            }

            Managers.Resource.RemoveSaveData(inData);
        }
    }
}