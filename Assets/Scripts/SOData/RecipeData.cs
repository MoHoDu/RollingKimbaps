using System.Collections.Generic;
using UnityEngine;
using EnumFiles;

[CreateAssetMenu(menuName = "Data/Recipe")]
public class RecipeData : SOData
{
    public string groupId;
    public int grade;
    public int price;
    public int satisfy;
    public List<string> requiredIngredients;
}