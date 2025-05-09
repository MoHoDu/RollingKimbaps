using System.Collections.Generic;
using System.Linq;

namespace GameDatas
{
    [DataName("Skill", true)]
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