using GameDatas;
using UnityEngine;

namespace UIs.Base
{
    public interface IFlowPanel
    {
        public Coroutine StartFlow(RaceStatus status);
    }
}