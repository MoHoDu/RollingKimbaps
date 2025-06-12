using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System;
using Unity.EditorCoroutines.Editor;
using EnumFiles;
using System.Collections.Generic;
#endif

public enum SheetType
{
    All,
    Ingredient,
    Recipe,
    Skill,
    Prap,
}

#if UNITY_EDITOR
public class SheetToSODataImporter : EditorWindow
{
    // Base URL for CSV export of a specific sheet via Google’s gviz API
    private string csvUrl = "https://docs.google.com/spreadsheets/d/1H3YBSqqe-uq7tG-rQWrHeaIxmsuK3nrIjAMwh_gAiIc/export?format=csv";
    private const string outputFolder = "Assets/Resources/DataObjects";
    private SheetType sheetType = SheetType.Ingredient;
    private SheetType currentSheetType = SheetType.Ingredient;
    private static readonly Dictionary<SheetType, string> sheetTypeToSheetGids = new Dictionary<SheetType, string>
    {
        { SheetType.Ingredient, "0" },
        { SheetType.Recipe, "141497470" },
        { SheetType.Skill, "1804540317" },
        { SheetType.Prap, "1960134854" }
    };

    // Constructs the full CSV URL for the selected sheet
    private string BuildSheetCsvUrl()
    {
        if (currentSheetType == SheetType.All) return csvUrl;

        string url = $"{csvUrl}&gid={sheetTypeToSheetGids[currentSheetType]}";
        return url;
    }

    private SOData CreateSO()
    {
        switch (currentSheetType)
        {
            case SheetType.Ingredient:
                return ScriptableObject.CreateInstance<IngredientData>();
            case SheetType.Recipe:
                return ScriptableObject.CreateInstance<RecipeData>();
            case SheetType.Skill:
                return ScriptableObject.CreateInstance<SkillData>();
            case SheetType.Prap:
                return ScriptableObject.CreateInstance<PrapData>();
            default:
                return null;
        }
    }

    [MenuItem("Tools/Import SO Datas from sheet")]
    public static void ShowWindow()
    {
        GetWindow<SheetToSODataImporter>("Sheet -> SOData");
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheet to ScriptableObjects", EditorStyles.boldLabel);
        csvUrl = EditorGUILayout.TextField("CSV URL", csvUrl);
        sheetType = (SheetType)EditorGUILayout.EnumPopup("Sheet Type", sheetType);

        GUILayout.Space(20);

        if (GUILayout.Button("Import from URL and Generate SOs"))
        {
            if (sheetType == SheetType.All)
                EditorCoroutineUtility.StartCoroutineOwnerless(DownloadAllAndGenerateFromUrl());
            else
                EditorCoroutineUtility.StartCoroutineOwnerless(DownloadAndGenerateFromUrl());
        }

        GUILayout.Space(5);

        if (sheetType != SheetType.All)
        {
            if (GUILayout.Button("Select CSV File and Generate SOs"))
            {
                string path = EditorUtility.OpenFilePanel("Select CSV File", Application.dataPath, "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    if (!path.Contains(sheetType.ToString()))
                    {
                        Debug.LogError("Selected file is not the correct sheet type.");
                        return;
                    }

                    try
                    {
                        string csvText = File.ReadAllText(path, Encoding.UTF8);
                        GenerateSOAssets(csvText);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to read CSV file: " + ex.Message);
                    }
                }
            }
        }
    }

    private IEnumerator DownloadAndGenerateFromUrl()
    {
        currentSheetType = sheetType;
        using (var www = UnityWebRequest.Get(BuildSheetCsvUrl()))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download CSV: " + www.error);
                yield break;
            }

            string csvText = www.downloadHandler.text;
            GenerateSOAssets(csvText);
        }
    }

    private IEnumerator DownloadAllAndGenerateFromUrl()
    {
        foreach (SheetType type in Enum.GetValues(typeof(SheetType)))
        {
            if (type == SheetType.All) continue;

            currentSheetType = type;
            using (var www = UnityWebRequest.Get(BuildSheetCsvUrl()))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download CSV: " + www.error);
                    yield break;
                }

                string csvText = www.downloadHandler.text;
                GenerateSOAssets(csvText);
            }
        }
    }

    private void GenerateSOAssets(string csvText)
    {
        string outputDataFolder = outputFolder + "/" + currentSheetType.ToString();
        Debug.Log("outputDataFolder: " + outputDataFolder);
        if (!Directory.Exists(outputDataFolder))
            Directory.CreateDirectory(outputDataFolder);

        // Clean existing assets
        var guids = AssetDatabase.FindAssets("t:SOData", new[] { outputDataFolder });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
        }

        var lines = csvText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            Debug.LogWarning("CSV contains no data.");
            return;
        }

        // Parse header
        var headers = lines[0].Split(',');
        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < headers.Length) continue;

            var so = CreateSO();
            for (int j = 0; j < headers.Length; j++)
            {
                var header = headers[j].Trim();
                var value = cols[j].Trim();
                SetValuesInSO(so, header, value);
            }

            var assetPathOut = Path.Combine(outputDataFolder, currentSheetType.ToString() + "Data" + "_" + so.id + ".asset").Replace("\\", "/");
            AssetDatabase.CreateAsset(so, assetPathOut);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(currentSheetType.ToString() + " ScriptableObjects generated successfully.");
    }

    private void SetValuesInSO(SOData so, string header, string value)
    {
        try
        {
            switch (header)
            {
                case "id": so.id = value; break;
                case "displayName": so.displayName = value; break;
                case "rarity": so.rarity = (Rarity)Enum.Parse(typeof(Rarity), value); break;
                case "path":
                    {
                        string assetPath = Path.Combine("Assets/Resources", value) + ".png";
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                        so.icon = sprite;
                    }
                    break;
                case "description": so.description = value; break;
            }

            if (so is IngredientData ingredient)
            {
                switch (header)
                {
                    case "type": ingredient.type = (IngredientType)Enum.Parse(typeof(IngredientType), value); break;
                    case "groupId": ingredient.groupId = value; break;
                    case "grade": ingredient.grade = int.Parse(value); break;
                    case "satisfy": ingredient.satisfy = int.Parse(value); break;
                }
            }
            else if (so is RecipeData recipe)
            {
                switch (header)
                {
                    case "groupId": recipe.groupId = value; break;
                    case "grade": recipe.grade = int.Parse(value); break;
                    case "price": recipe.price = int.Parse(value); break;
                    case "satisfy": recipe.satisfy = int.Parse(value); break;
                    case "requiredIngredients":
                        recipe.requiredIngredients = new List<string>();
                        string[] ingredientIds = value.Split(',');
                        foreach (string ingredientId in ingredientIds)
                        {
                            recipe.requiredIngredients.Add(ingredientId);
                        }
                        break;
                }
            }
            else if (so is SkillData skill)
            {
                switch (header)
                {
                    case "key": skill.key = (SkillKey)Enum.Parse(typeof(SkillKey), value); break;
                }
            }
            else if (so is PrapData prap)
            {
                switch (header)
                {
                    case "type":
                        prap.Type = (EPrapType)Enum.Parse(typeof(EPrapType), value); 
                        break;
                    case "appearanceDistance": 
                        prap.AppearanceDistance = int.Parse(value); break;
                    case "path":
                        prap.Path = value; break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to set values in SO: " + ex.Message);
        }
    }
#else
public class SheetToSODataImporter
{
#endif
}
