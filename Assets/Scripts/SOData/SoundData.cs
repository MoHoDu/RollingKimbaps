using System.Collections.Generic;
using UnityEngine;
using EnumFiles;
using System;

[Serializable]
[CreateAssetMenu(menuName = "Data/Sound")]
public class SoundData : SOData
{
    public EAudioSituation situation;
    public string path;
    public AudioClip clip;
}