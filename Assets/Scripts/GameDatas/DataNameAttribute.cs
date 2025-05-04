using System;

namespace GameDatas
{
    public class DataNameAttribute : Attribute
    {
        public string Name;
        public bool IsScriptableObject;

        public DataNameAttribute(string inName, bool inIsScriptableObject = false)
        {
            Name = inName;
            IsScriptableObject = inIsScriptableObject;
        }
    }
}