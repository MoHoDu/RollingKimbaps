using EnumFiles;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Prap")]
public class PrapData : SOData
{
    public EPrapType Type;
    public string Path;
    public float AppearanceDistance;
}