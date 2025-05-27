using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using JetBrains.Annotations;

namespace GameDatas
{
    public struct PrapDatas
    {
        public List<PrapData> DataList;
        public Dictionary<string, PrapData> DataDict;
        
        public PrapDatas(List<PrapData> dataList = null)
        {
            DataList = new ();
            DataDict = new();

            if (dataList == null) return;
            
            foreach (PrapData data in dataList)
            {
                Add(data);
            }
        }
        
        [CanBeNull]
        public PrapData GetFirstOrNull()
        {
            if (DataList is not null && DataList.Count > 0)
                return DataList.FirstOrDefault();
            
            return null;
        }

        public PrapData Get(string id)
        {
            return DataDict.GetValueOrDefault(id, null);
        }
        
        public void Add(PrapData data)
        {
            if (DataList.Contains(data)) return;
            DataList.Add(data);
            DataDict.TryAdd(data.id, data);
        }
        
        public void Remove(PrapData data)
        {
            DataList.Remove(data);
            DataDict.Remove(data.id);
        }
    }
    
    [DataName("Prap", EFileType.SO)]
    public class PrapContainer : BaseData<PrapData>
    {
        public Dictionary<EPrapType, SortedList<float, PrapDatas>> Data { get; private set; } = new();
        
        protected override void Set(List<PrapData> inList)
        {
            Data.Clear();

            foreach (var item in inList)
            {
                EPrapType prapType = item.Type;
                if (!Data.ContainsKey(prapType)) 
                    Data.TryAdd(prapType, new SortedList<float, PrapDatas>());

                SortedList<float, PrapDatas> prapDataList = Data[prapType];
                if (prapDataList.TryGetValue(item.AppearanceDistance, out var prapDatas))
                {
                    prapDatas.Add(item);
                }
                else
                {
                    prapDataList.Add(item.AppearanceDistance, new PrapDatas(new List<PrapData> { item }));
                }
            }
        }

        public PrapDatas? Get(EPrapType prapType, float distance = 0f)
        {
            if (Data.TryGetValue(prapType, out var prapDatas))
            {
                // 바이너리 서치로 인덱스 찾기
                int index = prapDatas.Keys.ToList().BinarySearch(distance);
                
                // 만약 0 이하면, 값이 없어 새롭게 추가할 때의 인덱스를 가리킴
                // ex) -2 (값은 없으나 들어간다면, 2번째 인덱스에 있게 될 것
                // 즉, ~로 비트 반전 연산을 하고, 그 이전의 값을 얻기 위해 -1
                if (index < 0) index = ~index - 1;

                if (index >= 0 && index < prapDatas.Count)
                    return prapDatas.Values[index];
                else
                    return null;
            }

            return null;
        }
    }
}