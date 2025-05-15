using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using GameDatas;
using UnityEngine;
using EnumFiles;

namespace ManagerSystem
{
    public static class DataContainer
    {
        public static SaveDatas SaveFiles { get; private set; } = new();
        public static IngredientDatas IngredientDatas { get; private set; } = new();
        public static RecipeDatas RecipeDatas { get; private set; } = new();
        public static SkillDatas SkillDatas { get; private set; } = new();

        public static bool IsLoadedComplete { get; private set; } = false;
        private static readonly string METHOD_NAME = "Load";

        private static int PropertyMaxCount = 1;
        private static int PropertyLoadedCount = 0;

        /// <summary>
        /// DataContainer의 프로퍼티를 읽어와서 각 데이터 저장 클래스에 알맞은 데이터들을 로드
        /// </summary>
        /// <param name="onLoading">로드 중 호출되는 콜백</param>
        /// <param name="onFinished">로드가 완료된 후 호출되는 콜백</param>
        public static async UniTaskVoid LoadDataFromSO(Action<int, int> onLoading, Action onFinished = null)
        {
            List<(EFileType, EDataType)> fileAndDatatypes = ExtractDataInfosFromProperties();

            if (fileAndDatatypes.Count == 0)
            {
                Debug.LogWarning("No data types found");
                return;
            }

            await AssignAssetsToProperties(fileAndDatatypes, onLoading);
            Debug.Log("Data loaded complete");

            IsLoadedComplete = true;
            onFinished?.Invoke();
        }

        /// <summary>
        /// DataContainer의 프로퍼티를 읽어와서 각 데이터 저장 클래스에 알맞은 데이터들을 로드
        /// </summary>
        /// <param name="dataInfos">로드할 데이터 정보 리스트</param>
        /// <param name="onLoading">로드 중 호출되는 콜백</param>
        private static async UniTask AssignAssetsToProperties(List<(EFileType, EDataType)> dataInfos, Action<int, int> onLoading = null)
        {
            PropertyInfo[] properties = typeof(DataContainer).GetProperties();

            PropertyLoadedCount = 0;
            PropertyMaxCount = dataInfos.Count;
            onLoading?.Invoke(PropertyLoadedCount, PropertyMaxCount);

            foreach (PropertyInfo property in properties)
            {
                if (TryGetAttributeValues(property, out var name, out EFileType type) && !string.IsNullOrEmpty(name))
                {
                    string methodName = GetMethodName(type);
                    if (property.PropertyType.GetMethod(methodName) is null) continue;
                    
                    if (Enum.TryParse(name, out EDataType dataType))
                    {
                        var obj = property.GetValue(typeof(DataContainer), null);

                        try
                        {
                            var method = property.PropertyType.GetMethod(methodName);
                            method?.Invoke(obj, new object[] { dataType });
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Failed to load data type {dataType}: {ex.Message}");
                            throw;
                        }
                        PropertyLoadedCount++;
                        onLoading?.Invoke(PropertyLoadedCount, PropertyMaxCount);
                    }
                }

                await UniTask.Delay(100);
            }
        }
        
        /// <summary>
        /// DataContainer의 프로퍼티를 읽어와서 각 데이터 저장 클래스의 알맞은 데이터 타입을 추출
        /// </summary>
        /// <returns>추출된 데이터 타입 리스트</returns>
        private static List<(EFileType, EDataType)> ExtractDataInfosFromProperties()
        {
            List<(EFileType, EDataType)> dataInfos = new List<(EFileType, EDataType)>();
            PropertyInfo[] properties = typeof(DataContainer).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (TryGetAttributeValues(property, out var name, out EFileType type))
                {
                    if (string.IsNullOrEmpty(name)) continue;
                    if (property.PropertyType.GetMethod(GetMethodName(type)) is null) continue;

                    if (Enum.TryParse(name, out EDataType dataType))
                    {
                        dataInfos.Add((type, dataType));
                    }
                }
            }

            return dataInfos;
        }

        /// <summary>
        /// 프로퍼티 내에 DataNameAttribute를 읽어와서 데이터 타입을 추출
        /// </summary>
        /// <param name="inPropertyInfo">프로퍼티 정보</param>
        /// <param name="outAttributeName">추출된 데이터 타입</param>
        /// <param name="outFileType">추출된 데이터 파일 타입</param>
        /// <returns>추출 성공 여부</returns>
        private static bool TryGetAttributeValues(PropertyInfo inPropertyInfo, out string outAttributeName, out EFileType outFileType)
        {
            var attributes = Attribute.GetCustomAttributes(inPropertyInfo.PropertyType);

            if (attributes.Length == 0)
            {
                Debug.LogWarning($"Attribute {inPropertyInfo.Name} has no {inPropertyInfo.PropertyType.Name}");
                outAttributeName = string.Empty;
                outFileType = EFileType.None;

                return false;
            }

            var attribute = attributes[0] as DataNameAttribute;

            if (attribute is null)
            {
                Debug.LogWarning($"Attribute {inPropertyInfo.Name} has no {inPropertyInfo.PropertyType.Name}");
                outAttributeName = string.Empty;
                outFileType = EFileType.None;

                return false;
            }

            outAttributeName = attribute.Name;
            outFileType = attribute.FileType;
            return true;
        }

        private static string GetMethodName(EFileType fileType) => fileType switch
        {
            EFileType.None => METHOD_NAME,
            EFileType.SO => $"{METHOD_NAME}SO",
            EFileType.Json => $"{METHOD_NAME}Json",
            EFileType.PlayerPref => $"{METHOD_NAME}PlayerPref",
            _ => METHOD_NAME
        };
    }
}
