using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManagerSystem;
using UnityEngine;
using EnumFiles;


namespace GameDatas
{
    public abstract class BaseData<T>
    {
        public virtual void LoadSO(EDataType dataType)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
            {
                Debug.LogWarning($"{typeof(T).Name} is not a ScriptableObject type.");
                return;
            }
            
            string directoryName = Path.Combine(BaseValues.SoDataBaseDirectory, dataType.ToString());

            try
            {
                ScriptableObject[] objs = Resources.LoadAll<ScriptableObject>(directoryName);
                if (objs == null || objs.Length == 0)
                {
                    Debug.LogWarning($"No {typeof(T).Name} assets found in directory: {directoryName}");
                    return;
                }
                
                T[] lodedAsstets = objs.OfType<T>().ToArray();
                if (lodedAsstets.Length == 0)
                {
                    Debug.LogWarning($"No {typeof(T).Name} assets found in directory: {directoryName}");
                    return;
                }

                // 만약 SaveData assets을 로드했다면, 각각의 FileName에 실제 파일 이름을 넣음
                foreach (var asset in lodedAsstets)
                {
                    if (asset is SaveDataSO sd)
                    {
                        sd.FileName = sd.name;
                    }
                }

                Set(lodedAsstets.ToList());
            }
            catch
            {
                Debug.LogWarning($"Directory not found: {directoryName}");
            }
        }

        public virtual void LoadPlayerPref(EDataType dataType) { }
        
        public virtual void LoadJson(EDataType dataType) { }
        
        public virtual void Load(EDataType dataType) { }

        protected abstract void Set(List<T> inList);
    }
}