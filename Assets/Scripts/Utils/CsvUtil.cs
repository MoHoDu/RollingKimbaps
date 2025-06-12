using System;
using System.Text.RegularExpressions;

namespace Utils
{

    public static class CsvUtil
    {
        /// <summary>
        /// CSV 한 줄을 필드 단위로 분리합니다. 큰따옴표 안의 쉼표는 무시합니다.
        /// </summary>
        public static string[] SplitCsvLine(string line)
        {
            // 쉼표(,) 앞뒤로 짝이 맞는 큰따옴표 영역 밖에 있는 쉼표만 매칭
            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";
            var regex = new Regex(pattern);

            // 분리
            var fields = regex.Split(line);

            // 앞뒤 공백·쌍따옴표 제거, 그리고 "" → " 치환
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i].Trim();
                if (f.Length >= 2 && f.StartsWith("\"") && f.EndsWith("\""))
                {
                    f = f.Substring(1, f.Length - 2)
                        .Replace("\"\"", "\"");
                }
                fields[i] = f;
            }

            return fields;
        }
    }
}