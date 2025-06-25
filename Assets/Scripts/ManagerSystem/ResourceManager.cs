using System.IO;
using JsonData;
using UnityEngine;
using ManagerSystem.Base;


namespace ManagerSystem
{
    public class ResourceManager : MonoBaseManager
    {
        /// <summary>
        /// 프리팹 이름을 가지고, Resources 폴더에서 프리팹을 생성
        /// </summary>
        /// <param name="inAssetName">프리팹 이름</param>
        /// <param name="inParent">부모 오브젝트</param>
        /// <returns>생성된 프리팹 오브젝트</returns>
        public GameObject Instantiate(string inAssetName, Transform inParent = null, bool worldPositionStays = false)
        {
            GameObject go = null;

            GameObject resource = Resources.Load(inAssetName) as GameObject;

            if (resource)
            {
                go = Instantiate(resource);
                if (inParent)
                {
                    go.transform.SetParent(inParent, worldPositionStays);
                }
                go.name = inAssetName;
            }
            else
            {
                Debug.LogWarning($"[Resource Warn] {inAssetName} not found");
            }

            return go;
        }

        public T GetResource<T>(string inAssetName) where T : Object
        {
            try
            {
                T resource = Resources.Load(inAssetName) as T;
                if (resource != null)
                {
                    return resource;
                }
                else
                {
                    Debug.LogWarning($"[Resource Warn] {inAssetName} not found");
                }
            }
            catch
            {
                Debug.LogWarning($"[Resource Warn] {inAssetName} not found");
            }

            return null;
        }

        /// <summary>
        /// 실제 세이브 데이터 파일을 제거
        /// </summary>
        /// <param name="saveData">삭제 할 SaveData</param>
        /// <returns>파일 유무 및 파일이 성공적으로 제거되었는지 여부</returns>
        public bool RemoveSaveData(SaveData saveDataSo)
        {
            return true;
        }

        public void Destroy(GameObject inObj)
        {
            Object.Destroy(inObj);
        }
    }
}
