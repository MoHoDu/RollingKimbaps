using System.Collections.Generic;
using EnumFiles;


namespace JsonData
{
    [System.Serializable]
    public class SaveData
    {
        // 기본 정보
        public string FileName = string.Empty;
        public string displayName = string.Empty;
        public GameType gameType;
        public bool isDefault = false;
        
        // 저장 시간 및 게임 내 일자
        public int saveYear;
        public int saveMonth;
        public int saveDay;
        public int saveHour;
        public int saveMinute;
        public int inGameMonth;
        public int inGameWeek;
        
        // 보유한 재료, 레시피, 스킬
        public List<string> ingredientsID = new List<string>();
        public List<string> recipeID = new List<string>();
        public List<string> skillID = new List<string>();
    }
}