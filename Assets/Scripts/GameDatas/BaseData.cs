using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManagerSystem;
using UnityEngine;


namespace GameDatas
{
    public abstract class BaseData<T> where T : ScriptableObject
    {
        public void Load(EDataType dataType)
        {
            string directoryName = Path.Combine(BaseValues.SoDataBaseDirectory, dataType.ToString());

            try
            {
                T[] lodedAsstets = Resources.LoadAll<T>(directoryName);
                if (lodedAsstets == null || lodedAsstets.Length == 0)
                {
                    Debug.LogWarning($"No {typeof(T).Name} assets found in directory: {directoryName}");
                    return;
                }

                Set(lodedAsstets.ToList());
            }
            catch
            {
                Debug.LogWarning($"Directory not found: {directoryName}");
            }
        }

        protected abstract void Set(List<T> inList);
    }
}