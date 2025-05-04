using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class TransformExtentionMethods
{
    /// <summary>
    /// 주어진 Transform의 모든 자식 Transform을 재귀적으로 탐색하여 제공된 사전에 캐시.
    /// </summary>
    /// <param name="childTransform">캐시된 자식 Transform을 저장할 사전. 이 메서드에 의해 채워짐.</param>
    public static void CachedChildTransform(this Transform tr, Dictionary<string, Transform> childTransform)
    {
        foreach (Transform child in tr)
        {
            childTransform.TryAdd(child.name, child);

            if (child.childCount > 0)
                CachedChildTransform(child, childTransform);
        }
    }
}