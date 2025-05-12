using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnumFiles;
using JsonData;
using ManagerSystem;
using UnityEngine;


namespace GameDatas
{
    [DataName("SaveFile", EFileType.Json)]
    public class SaveDatas : BaseData<SaveData>
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
                cloneData.ingredientsID = defaultData.ingredientsID;
                cloneData.recipeID = defaultData.recipeID;
                cloneData.skillID = defaultData.skillID;
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

            Managers.Save.RemoveFile(inData.gameType, inData);
        }
        
        public override void LoadJson(EDataType dataType)
        {
            foreach (GameType gameType in Enum.GetValues(typeof(GameType)))
            {
                List<SaveData> loadDatas = Managers.Save.LoadFiles(gameType);
                if (loadDatas == null || loadDatas.Count == 0) continue;
                Set(loadDatas);
            }

            if (DefaultData.Count == 0)
            {
                LoadSO(dataType);
            }

            foreach (var saveList in Data.Values)
            {
                if (saveList != null)
                {
                    saveList.ForEach(s => Managers.Save.SaveFile(s.gameType, s));
                }
            }
        }

        public override void LoadSO(EDataType dataType)
        {
            string directoryName = Path.Combine(BaseValues.SoDataBaseDirectory, dataType.ToString());

            try
            {
                SaveDataSO[] objs = Resources.LoadAll<SaveDataSO>(directoryName);

                List<SaveData> lodedAsstets = objs.Select(s => new SaveData
                {
                    FileName = s.FileName,
                    gameType = s.gameType,
                    isDefault = s.isDefault,
                    saveYear = s.saveYear,
                    saveMonth = s.saveMonth,
                    saveDay = s.saveDay,
                    saveHour = s.saveHour,
                    saveMinute = s.saveMinute,
                    inGameMonth = s.inGameMonth,
                    inGameWeek = s.inGameWeek,
                    ingredientsID = s.ingredientList?.Select(i => i.id).ToList(),
                    recipeID = s.recipeList?.Select(i => i.id).ToList(),
                    skillID = s.skillList?.Select(i => i.id).ToList(),
                }).ToList();
                if (lodedAsstets.Count == 0)
                {
                    Debug.LogWarning($"No SaveDataSO assets found in directory: {directoryName}");
                    return;
                }

                Set(lodedAsstets);
            }
            catch
            {
                Debug.LogWarning($"Directory not found: {directoryName}");
            }
        }
    }
}