using System;
using System.Collections.Generic;
using System.IO;
using EnumFiles;
using JsonData;
using UnityEngine;
using Utils;

namespace ManagerSystem
{
    public class SaveManager : BaseManager
    {
        private static readonly string SaveDirectory = Path.Combine(Application.persistentDataPath, BaseValues.SaveDataDirectory);
        private const string SaveFileExtension = ".json";
        private const string SaveFilePrefix = "SaveData";

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