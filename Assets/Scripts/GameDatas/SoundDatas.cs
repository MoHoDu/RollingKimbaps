using System.Collections.Generic;
using System.Linq;
using EnumFiles;

namespace GameDatas
{
    [DataName("Sound", EFileType.SO)]
    public class SoundDatas : BaseData<SoundData>
    {
        public Dictionary<string, SoundData> Data { get; private set; } = new Dictionary<string, SoundData>();

        protected override void Set(List<SoundData> inList)
        {
            Data.Clear();
        }
    }
}