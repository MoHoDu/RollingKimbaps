using System.Collections.Generic;
using EnumFiles;

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

            foreach (PrapData data in DataList)
            {
                Add(data);
            }
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
    }
}