using System.Collections.Generic;
using EnumFiles;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Recipe")]
public class RecipeData : SOData
{
    public string groupId;
    public int grade;
    public int price;
    public int satisfy;
    public List<EIngredientIndex> requiredIngredients;
    public float appearanceMinDistance;
}