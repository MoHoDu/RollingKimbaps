using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnumFiles;
using JsonData;
using ManagerSystem;
using UnityEngine;
using Utils;


namespace GameDatas
{
    [DataName("SaveFile", EFileType.PlayerPref)]
    public class SaveDatas : BaseData<SaveData>
    {
        public Dictionary<GameType, SaveData[]> Data { get; private set; } = new();
        private Dictionary<GameType, SaveData> DefaultData = new();

        private const int DataMaxCount = 10;
        public int MaxCount => DataMaxCount;
        
        protected override void Set(List<SaveData> inList)
        {
            Data.Clear();
            DefaultData.Clear();

            foreach (var item in inList)
            {
                Save(item);
            }
        }

        protected void SetAndSaveDatas(List<SaveData> inList)
        {
            Data.Clear();
            DefaultData.Clear();

            foreach (var item in inList)
            {
                Save(item, true);
            }
        }

        public bool Save(SaveData saveData, bool forceSave = false)
        {
            if (saveData is null || saveData.saveIndex is >= DataMaxCount or < 0) 
                return false;
            
            if (saveData.isDefault)
            {
                DefaultData.TryAdd(saveData.gameType, saveData);
            }
            else
            {
                if (!Data.ContainsKey(saveData.gameType))
                {
                    Data[saveData.gameType] = new SaveData[DataMaxCount];
                }
                
                SaveData[] inDatas = Data[saveData.gameType];
                inDatas[saveData.saveIndex] = saveData;
            }
            
            // PlayerPref에 저장
            if (forceSave)
            {
                try
                {
                    string json = JsonUtility.ToJson(saveData);
#if UNITY_EDITOR
                    Debug.Log("Try to save file on PlayerPrefs");
                    Debug.Log($"Try to encrypt file: \n{json}");
#endif
            
                    string encryptedBase64 = AndroidAESBidge.Encrypt(json);
                    string playerPrefKey = "";
                    if (saveData.isDefault)
                        playerPrefKey = $"{PlayerPrefsKey_Prefix.SAVE_DATA}_{saveData.gameType}_default";
                    else
                        playerPrefKey = $"{PlayerPrefsKey_Prefix.SAVE_DATA}_{saveData.gameType}_{saveData.saveIndex:D2}";
#if UNITY_EDITOR
                    Debug.Log($"Success to encrypt file: {encryptedBase64}");
                    Debug.Log($"Set PlayerPrefs key is: {playerPrefKey}");
#endif
            
                    PlayerPrefManager.SetString(playerPrefKey, encryptedBase64);
                }
                catch
                {
                    Debug.LogWarning($"");
                    return false;
                }
            }

            if (saveData.isDefault)
                Debug.Log($"Success to Save data: {saveData.gameType} / default data");
            else
                Debug.Log($"Success to Save data: {saveData.gameType} / Slot: {saveData.saveIndex:D2}");
            return true;
        }

        public SaveData[] Get(GameType gameType)
        {
            return Data.GetValueOrDefault(gameType, new SaveData[10]);
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

        public bool DeleteSaveData(SaveData inData)
        {
            // 디폴트 데이터는 삭제 불가
            if (inData.isDefault) return false;
            
            int saveIndex = inData.saveIndex;
            if (saveIndex is >= DataMaxCount or < 0) return false;
            
            GameType gameType = inData.gameType;
            if (Data.TryGetValue(gameType, out var DataArray))
            {
                if (DataArray[saveIndex] == inData)
                {
                    Data[gameType][saveIndex] = null;
                    if (Data[gameType].Length == 0) 
                        Data.Remove(gameType);
                    
                    // PlayerPrefs에서도 제거
                    string playerPrefKey = $"{PlayerPrefsKey_Prefix.SAVE_DATA}_{gameType}_{inData.saveIndex:D2}";
                    PlayerPrefManager.DeleteKey(playerPrefKey);

                    return true;
                }
            }

            return false;
        }

        public override void LoadPlayerPref(EDataType dataType)
        {
            List<SaveData> inDataList = new();
            foreach (GameType gameType in Enum.GetValues(typeof(GameType)))
            {
                // gameType마다의 디폴트 데이터를 가져옴
                string playerPrefKey = $"{PlayerPrefsKey_Prefix.SAVE_DATA}_{gameType}_default";
                string saveText = PlayerPrefManager.GetString(playerPrefKey);
                if (!string.IsNullOrEmpty(saveText))
                {
                    // 복호화 하여 데이터를 저장
                    DecryptData(saveText, in inDataList);                
                }
                
                // gameType의 0~9번째 데이터를 가져옴
                for (int i = 0; i < DataMaxCount; i++)
                {
                    playerPrefKey = $"{PlayerPrefsKey_Prefix.SAVE_DATA}_{gameType}_{i:D2}";
                    saveText = PlayerPrefManager.GetString(playerPrefKey);
                    if (string.IsNullOrEmpty(saveText)) continue;

                    // 복호화 하여 데이터를 저장
                    DecryptData(saveText, in inDataList);
                }
            }
            
            if (inDataList.Count > 0)
                Set(inDataList);
            else 
                LoadSO(dataType);
        }

        public SaveData GetSaveData(GameType gameType, int? saveIndex)
        {
            if (!saveIndex.HasValue)
            {
                return GetDefaultData(gameType);
            }
            
            int index = saveIndex.Value;
            if (index is < 0 or >= DataMaxCount)
                return null;
            
            SaveData[] saveDataArray = Get(gameType);
            if (saveDataArray is null)
                return null;
            
            if (saveDataArray.Length > index)
                return saveDataArray[index];
            
            return null;
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

            // foreach (var saveList in Data.Values)
            // {
            //     if (saveList != null)
            //     {
            //         saveList.ForEach(s => Managers.Save.SaveFile(s.gameType, s));
            //     }
            // }
        }

        public override void LoadSO(EDataType dataType)
        {
            string directoryName = Path.Combine(BaseValues.SoDataBaseDirectory, dataType.ToString());

            try
            {
                SaveDataSO[] objs = Resources.LoadAll<SaveDataSO>(directoryName);

                List<SaveData> lodedAsstets = objs.Select(s => new SaveData
                {
                    FileName = string.Empty,
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

                SetAndSaveDatas(lodedAsstets);
            }
            catch
            {
                Debug.LogWarning($"Directory not found: {directoryName}");
            }
        }

        public static SaveData CloneData(in SaveData original)
        {
            if (original == null) return null;
            
            SaveData newSave = new SaveData()
            {
                saveIndex = original.saveIndex,
                gameType = original.gameType,
                isDefault = false,
                saveYear = original.saveYear,
                saveMonth = original.saveMonth,
                saveDay = original.saveDay,
                saveHour = original.saveHour,
                saveMinute = original.saveMinute,
                inGameMonth = original.inGameMonth,
                inGameWeek = original.inGameWeek,
                ingredientsID = original.ingredientsID,
                recipeID = original.recipeID,
                skillID = original.skillID,
            };
            
            return newSave;
        }

        private void DecryptData(string saveText, in List<SaveData> inDataList)
        {
            try
            {
                string json = AndroidAESBidge.Decrypt(saveText);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                inDataList.Add(data);
                
                Debug.Log($"Success to decrypt save: {data.gameType} / Slot: {data.saveIndex:D2}");
            }
            catch
            {
                Debug.LogWarning($"Failed to decrypt save: {saveText}");
            }
        }
    }
}