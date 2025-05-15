using System;
using UnityEngine;

namespace Utils
{
    public enum PlayerPrefsKey_Prefix
    {
        SAVE_DATA,
    }
    
    public enum CLIENT_FLAG
    {
        FLAG_LOCAL_TEST,        // 로컬 빌드, 어드레서블 로컬
        FLAG_SHOW_DEV_UI,       // 개발용 UI 출력
        FLAG_BLOCK_DEBUG_LOG,   // 디버그 로그 출력 여부 플래그
        FLAG_REMOVE_RESOURCES,  // 어드레서블 일괄 삭제 플래그
        FLAG_TUTORIAL_PLAY,     // 튜토리얼 플레이 여부 플래그
    }
    
    public static class PlayerPrefManager
    {
        public static void SetFlag(string key, bool value)
        {
            int applyValue = value ? 1 : 0;
            PlayerPrefs.SetInt(key, applyValue);
        }
        
        public static bool IsFlagOn(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue) == 1;
        }      
        
        public static string GetString(string key, string defaultValue = "")
        {
            if (!HasPrefixInKey(key)) return defaultValue; 
            
            string value = null;
            value = PlayerPrefs.GetString(key.ToString(), defaultValue);
            return value;
        }

        public static void SetString(string key, string value)
        {
            if (HasPrefixInKey(key)) PlayerPrefs.SetString(key.ToString(), value);
        }

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        private static bool HasPrefixInKey(string key)
        {
            foreach (var prefix in Enum.GetValues(typeof(PlayerPrefsKey_Prefix)))
            {
                if (key.StartsWith(prefix.ToString()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}