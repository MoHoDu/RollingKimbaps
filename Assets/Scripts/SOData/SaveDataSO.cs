using System;
using System.Collections.Generic;
using UnityEngine;
using EnumFiles;


[CreateAssetMenu(menuName = "Data/SaveData")]
public class SaveDataSO : SOData
{
    // 파일 이름
    public string FileName = string.Empty;

    // 게임 타입
    public GameType gameType;
    public bool isDefault = false;

    // 저장 시간 및 게임 내 일자
    public int saveYear;
    public int saveMonth;
    public int saveDay;
    public int saveHour;
    public int saveMinute;
    public DateTime saveTime
    {
        get
        {
            try
            {
                DateTime dateTime = new DateTime(saveYear, saveMonth, saveDay, saveHour, saveMinute, 0);
                return dateTime;
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }

    public int inGameMonth;
    public int inGameWeek;
    public (int, int) inGameDate
    {
        get
        {
            inGameMonth = Mathf.Max(1, inGameMonth);
            inGameMonth = Mathf.Min(12, inGameMonth);
            inGameWeek = Mathf.Max(1, inGameWeek);
            inGameWeek = Mathf.Min(5, inGameWeek);
            
            return (inGameMonth, inGameWeek);
        }
    }

    // 재료, 레시피, 스킬
    public List<IngredientData> ingredientList;
    public List<RecipeData> recipeList;
    public List<SkillData> skillList;
}