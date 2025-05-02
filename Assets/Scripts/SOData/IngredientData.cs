using UnityEngine;
using EnumFiles;

[CreateAssetMenu(menuName = "Data/Ingredient")]
public class IngredientData : SOData
{
    public string groupId;
    public IngredientType type; // 예: Vegetables, Meats
    public int grade;
    public int satisfy;
}