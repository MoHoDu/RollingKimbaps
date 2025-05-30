using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class SortedListExtensions
    {
        public static int GetGreaterOrEqualIndex<TValue>(
            this SortedList<float, TValue> list, float threshold)
        {
            float[] keysArray = list.Keys.ToArray();
            int index = Array.BinarySearch(keysArray, threshold);
            if (index < 0)
                index = ~index;

            return index;
        }
    }
}