using System;
using System.Collections;
using System.Collections.Generic;
using ManagerSystem;
using UnityEngine;
using UnityEngine.Rendering;

namespace Test
{
    public class DataViewer : MonoBehaviour
    {
        public SerializedDictionary<string, SoundData> soundDataDict = new();
        public SerializedDictionary<EAudioSituation, List<(int, SoundData)>> soundGroupDict = new();

        [ContextMenu("Load Data")]
        public void LoadData()
        {
            soundDataDict.Clear();
            soundGroupDict.Clear();

            var datas = DataContainer.SoundDatas.Data;
            foreach (var data in datas)
            {
                soundDataDict.Add(data.Key, data.Value);
            }

            var groupData = DataContainer.SoundDatas.GroupData;
            foreach (var group in groupData)
            {
                List<(int, SoundData)> soundList = new();
                foreach (var sound in group.Value)
                {
                    soundList.Add((sound.Key, sound.Value));
                }
                soundGroupDict.Add(group.Key, soundList);
            }
            Debug.Log("Data loaded into viewer");
        }
    }
}