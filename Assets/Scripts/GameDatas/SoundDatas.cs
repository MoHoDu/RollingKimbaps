using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using UnityEngine;

namespace GameDatas
{
    [DataName("Sound", EFileType.SO)]
    public class SoundDatas : BaseData<SoundData>
    {
        public Dictionary<string, SoundData> Data { get; private set; } = new Dictionary<string, SoundData>();
        public Dictionary<EAudioSituation, SortedList<int, SoundData>> GroupData { get; private set; } = new();

        protected override void Set(List<SoundData> inList)
        {
            Data.Clear();
            Data = inList.ToDictionary(x => x.id);

            GroupData.Clear();
            foreach (var item in inList)
            {
                try
                {
                    int id = int.Parse(item.id);
                    if (!GroupData.ContainsKey(item.situation))
                    {
                        GroupData[item.situation] = new SortedList<int, SoundData>();
                    }
                    GroupData[item.situation].TryAdd(id, item);
                }
                catch
                {
                    Debug.LogWarning($"[SoundDatas] Invalid ID format for sound data: {item.id}. Expected an integer.");
                    continue;
                }
            }
        }

        /// <summary>
        /// 상황과 인덱스 ID를 사용하여 사운드 클립을 가져옵니다.
        /// 만약 해당 상황이나 ID가 존재하지 않으면 null을 반환합니다.
        /// </summary>
        /// <param name="situation">상황</param>
        /// <param name="id">인덱스 ID</param>
        /// <returns>AudioClip or null</returns>
        public AudioClip GetClip(EAudioSituation situation, int id)
        {
            if (GroupData.TryGetValue(situation, out SortedList<int, SoundData> soundList))
            {
                if (soundList.TryGetValue(id, out SoundData soundData))
                {
                    return soundData.clip;
                }
            }

            Debug.LogWarning($"[SoundDatas] Sound clip not found for situation: {situation}, ID: {id}");
            return null;
        }
    }
}