using System.Collections.Generic;
using UnityEngine;

namespace Utils
{

    public static class ListExtensions
    {
        /// <summary>
        /// Fisher–Yates 셔플을 이용해 리스트를 셔플하고, 앞쪽 n개를 반환합니다.
        /// 원본 리스트를 변경하고 싶지 않으면 내부에서 복사본을 만들어 쓰세요.
        /// </summary>
        public static List<T> GetRandomElementsShuffle<T>(this List<T> list, int n)
        {
            if (n < 0 || n > list.Count)
                throw new System.ArgumentOutOfRangeException(nameof(n), "n은 0 이상 리스트 개수 이하이어야 합니다.");

            // 1) 리스트 복사 (원본 보존용)
            var tempList = new List<T>(list);

            int count = tempList.Count;
            for (int i = 0; i < n; i++)
            {
                // i부터 끝까지 중 하나를 랜덤으로 선택
                int r = Random.Range(i, count);
                // swap tempList[i]와 tempList[r]
                (tempList[i], tempList[r]) = (tempList[r], tempList[i]);
            }

            // 2) 셔플된 앞쪽 n개를 잘라서 반환
            return tempList.GetRange(0, n);
        }
    }
}