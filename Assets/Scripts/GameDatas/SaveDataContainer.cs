using System;
using System.Collections.Generic;
using System.Linq;
using EnumFiles;


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
    }
}