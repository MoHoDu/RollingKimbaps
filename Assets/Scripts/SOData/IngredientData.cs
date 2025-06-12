using UnityEngine;
using EnumFiles;

[CreateAssetMenu(menuName = "Data/Ingredient")]
public class IngredientData : SOData
{
    public EIngredientIndex groupId;
    public string prapID;
    public IngredientType type; // 예: Vegetables, Meats
    public int grade;
    public int satisfy;
    
    // 리소스 폴더 내 프리팹 경로
    public string placedPath;
    public string innerPath;
    
    // In game values
    public Vector3 position;
}