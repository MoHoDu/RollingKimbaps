using System;
using EnumFiles;

namespace GameDatas
{
    public class DataNameAttribute : Attribute
    {
        public string Name;
        public EFileType FileType;

        public DataNameAttribute(string inName, EFileType fileType = EFileType.None)
        {
            Name = inName;
            FileType = fileType;
        }
    }
}