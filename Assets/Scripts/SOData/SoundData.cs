using System.Collections.Generic;
using UnityEngine;
using EnumFiles;

[CreateAssetMenu(menuName = "Data/Sound")]
public class SoundData : SOData
{
    public EAudioSituation id;
    public string path;
    public AudioClip clip;
}