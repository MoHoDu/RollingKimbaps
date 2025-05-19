using System.Collections.Generic;
using System.Linq;
using EnumFiles;

namespace GameDatas
{
    [DataName("Skill", EFileType.SO)]
    public class SkillDatas : BaseData<SkillData>
    {
        public Dictionary<string, SkillData> Data { get; private set; } = new Dictionary<string, SkillData>();

        protected override void Set(List<SkillData> inList)
        {
            Data.Clear();
            Data = inList.ToDictionary(x => x.id);
        }   
    }
}