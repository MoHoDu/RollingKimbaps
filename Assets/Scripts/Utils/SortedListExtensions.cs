using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class SortedListExtensions
    {
        public static IEnumerable<KeyValuePair<float, TValue>> GetGreaterOrEqual<TValue>(
            this SortedList<float, TValue> list, float threshold)
        {
            float[] keysArray = list.Keys.ToArray();
            int index = Array.BinarySearch(keysArray, threshold);
            if (index < 0)
                index = ~index;

            for (int i = index; i < list.Count; i++)
            {
                yield return new KeyValuePair<float, TValue>(list.Keys[i], list.Values[i]);
            }
        }
    }
}