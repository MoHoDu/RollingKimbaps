

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct InnerPosition
{
    [SerializeField] public int ingredientCount;
    [SerializeField] public List<Vector3> placePositions;

    public InnerPosition(int count)
    {
        ingredientCount = count;
        placePositions = new List<Vector3>(ingredientCount);
        for (int i = 0; i < count; i++)
        {
            placePositions.Add(Vector3.zero);
        }
    }
}

[CreateAssetMenu(menuName = "Settings/InnerPlaceSO")]
public class InnerPlacePositionSO : SOData
{
    [SerializeField] public List<InnerPosition> infos = new();

    public InnerPosition? GetPositionInfo(int count)
    {
        foreach (InnerPosition info in infos)
        {
            if (info.ingredientCount == count)
                return info;
        }

        return null;
    }
}