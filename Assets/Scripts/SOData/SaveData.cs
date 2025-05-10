using System;
using System.Collections.Generic;
using UnityEngine;
using EnumFiles;


[CreateAssetMenu(menuName = "Data/SaveData")]
public class SaveData : SOData
{
    // 게임 타입
    public GameType gameType;
    public bool isDefault = false;

    // 저장 시간 및 게임 내 일자
    public int saveYear;
    public int saveMonth;
    public int saveDay;
    public int saveHour;
    public int saveMinute;
    public DateTime saveTime => new DateTime(saveYear, saveMonth, saveDay, saveHour, saveMinute, 0);

    public int inGameMonth;
    public int inGameWeek;
    public (int, int) inGameDate => (inGameMonth, inGameWeek);

    // 재료, 레시피, 스킬
    public List<IngredientData> ingredientList;
    public List<RecipeData> recipeList;
    public List<SkillData> skillList;
}