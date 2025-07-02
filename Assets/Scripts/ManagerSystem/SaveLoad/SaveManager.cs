using System;
using System.Collections.Generic;
using System.IO;
using EnumFiles;
using JsonData;
using UnityEngine;
using Utils;
using ManagerSystem.Base;

namespace ManagerSystem.SaveLoad
{
    public class SaveManager : BaseManager
    {
        public PlayerSettingsController PlayerSettings { get; private set; } = new PlayerSettingsController();

        private static readonly string SaveDirectory = Path.Combine(Application.persistentDataPath, BaseValues.SaveDataDirectory);
        private const string SaveFileExtension = ".json";
        private const string SaveFilePrefix = "SaveData";

        public override void Initialize(params object[] args)
        {
            base.Initialize(args);

            PlayerSettings.LoadSettings();
        }

        public void SaveData(SaveData saveData)
        {
            int maxCount = DataContainer.SaveFiles.MaxCount;
            if (saveData.saveIndex < 0 || saveData.saveIndex > maxCount) return;

            // DataContainer에 저장
            bool isSuccess = DataContainer.SaveFiles.Save(saveData, true);
            if (isSuccess)
                Debug.Log("Finished saving data");
            else
                Debug.LogWarning("Error occured while saving data");
        }

        /// <summary>
        /// gameType에 맞는 세이브 데이터를 가져옴
        /// </summary>
        /// <param name="gameType">가져올 세이브 데이터의 게임 타입</param>
        /// <param name="saveIndex">null인 경우 시작 데이터를 지칭</param>
        /// <returns>세이브 데이터 or null</returns>
        public SaveData LoadData(GameType gameType, int? saveIndex)
        {
            return DataContainer.SaveFiles.GetSaveData(gameType, saveIndex);
        }

        public bool RemoveData(SaveData saveData)
        {
            if (saveData is null || saveData.isDefault) return false;
            
            return DataContainer.SaveFiles.DeleteSaveData(saveData);
        }
        
        public void SaveFile(GameType gameType, SaveData saveData)
        {
            if (string.IsNullOrEmpty(saveData.FileName))
            {
                saveData.FileName = $"{SaveFilePrefix}_" + GetSaveTime(saveData).ToString("yyyyMMdd-hhmmss");
            }
            
            if (!Directory.Exists(Path.Combine(SaveDirectory, gameType.ToString())))
            {
                Directory.CreateDirectory(Path.Combine(SaveDirectory, gameType.ToString()));
            }
            
            string savePath = Path.Combine(SaveDirectory, gameType.ToString(), $"{saveData.FileName}{SaveFileExtension}");
#if UNITY_EDITOR
            Debug.Log($"Try to save file: {savePath}");
#endif
            string json = JsonUtility.ToJson(saveData);
            string encryptedBase64 = AndroidAESBidge.Encrypt(json);
            File.WriteAllText(savePath, encryptedBase64);
        }

        public List<SaveData> LoadFiles(GameType gameType)
        {
            List<SaveData> saveList = new List<SaveData>();
            string folderPath = Path.Combine(SaveDirectory, gameType.ToString());

            if (!Directory.Exists(folderPath))
                return saveList;

            string[] files = Directory.GetFiles(folderPath, $"*{SaveFileExtension}");

            foreach (var file in files)
            {
                try
                {
                    string base64 = File.ReadAllText(file);
                    string json = AndroidAESBidge.Decrypt(base64);
                    SaveData data = JsonUtility.FromJson<SaveData>(json);
                    saveList.Add(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to load file: {file}\n{e.Message}");
                }
            }

            return saveList;
        }

        public bool RemoveFile(GameType gameType, SaveData saveData)
        {
            string filePath = Path.Combine(SaveDirectory, gameType.ToString(), $"{saveData.FileName}{SaveFileExtension}");

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Debug.Log($"[SaveManager] Removed save file: {filePath}");
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to delete save file: {filePath}\n{e.Message}");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Save file not found: {filePath}");
                return false;
            }
        }
        
        public DateTime GetSaveTime(in SaveData inData)
        {
            try
            {
                DateTime dateTime = new DateTime(inData.saveYear, inData.saveMonth, inData.saveDay, inData.saveHour, inData.saveMinute, 0);
                return dateTime;
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public (int Month, int Week) GetInGameWeek(in SaveData inData)
        {
            int inGameMonth = Mathf.Max(1, inData.inGameMonth);
            inGameMonth = Mathf.Min(12, inGameMonth);
            
            int inGameWeek = Mathf.Max(1, inData.inGameWeek);
            inGameWeek = Mathf.Min(5, inGameWeek);
            
            return (inGameMonth, inGameWeek);
        }
    }
}