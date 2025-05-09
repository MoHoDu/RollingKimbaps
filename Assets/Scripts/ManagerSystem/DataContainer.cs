using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using GameDatas;
using UnityEngine;

namespace ManagerSystem
{
    public enum EDataType
    {
        Ingredient,
        Recipe,
        Skill,
    }

    public static class DataContainer
    {
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
            List<EDataType> dataTypes = ExtractDataTypesFromProperties();

            if (dataTypes.Count == 0)
            {
                Debug.LogWarning("No data types found");
                return;
            }

            await AssignAssetsToProperties(dataTypes, onLoading);
            Debug.Log("Data loaded complete");

            IsLoadedComplete = true;
            onFinished?.Invoke();
        }

        /// <summary>
        /// DataContainer의 프로퍼티를 읽어와서 각 데이터 저장 클래스에 알맞은 데이터들을 로드
        /// </summary>
        /// <param name="dataTypes">로드할 데이터 타입 리스트</param>
        /// <param name="onLoading">로드 중 호출되는 콜백</param>
        private static async UniTask AssignAssetsToProperties(List<EDataType> dataTypes, Action<int, int> onLoading = null)
        {
            PropertyInfo[] properties = typeof(DataContainer).GetProperties();

            PropertyLoadedCount = 0;
            PropertyMaxCount = dataTypes.Count;
            onLoading?.Invoke(PropertyLoadedCount, PropertyMaxCount);

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType.GetMethod(METHOD_NAME) is null) continue;

                if (TryGetAttributeName(property, out var name) && !string.IsNullOrEmpty(name))
                {
                    if (Enum.TryParse(name, out EDataType dataType))
                    {
                        var obj = property.GetValue(typeof(DataContainer), null);

                        try
                        {
                            var method = property.PropertyType.GetMethod(METHOD_NAME);
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
        private static List<EDataType> ExtractDataTypesFromProperties()
        {
            List<EDataType> dataTypes = new List<EDataType>();
            PropertyInfo[] properties = typeof(DataContainer).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType.GetMethod(METHOD_NAME) is null) continue;

                if (TryGetAttributeName(property, out var name))
                {
                    if (string.IsNullOrEmpty(name)) continue;

                    if (Enum.TryParse(name, out EDataType dataType))
                    {
                        dataTypes.Add(dataType);
                    }
                }
            }

            return dataTypes;
        }

        /// <summary>
        /// 프로퍼티 내에 DataNameAttribute를 읽어와서 데이터 타입을 추출
        /// </summary>
        /// <param name="inPropertyInfo">프로퍼티 정보</param>
        /// <param name="outAttributeName">추출된 데이터 타입</param>
        /// <returns>추출 성공 여부</returns>
        private static bool TryGetAttributeName(PropertyInfo inPropertyInfo, out string outAttributeName)
        {
            var attributes = Attribute.GetCustomAttributes(inPropertyInfo.PropertyType);

            if (attributes.Length == 0)
            {
                Debug.LogWarning($"Attribute {inPropertyInfo.Name} has no {inPropertyInfo.PropertyType.Name}");
                outAttributeName = string.Empty;

                return false;
            }

            var attribute = attributes[0] as DataNameAttribute;

            if (attribute is null)
            {
                Debug.LogWarning($"Attribute {inPropertyInfo.Name} has no {inPropertyInfo.PropertyType.Name}");
                outAttributeName = string.Empty;

                return false;
            }

            outAttributeName = attribute.Name;
            return true;
        }
    }
}
