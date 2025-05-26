using GameDatas;
using UnityEngine;

namespace Panels.Base
{
    public interface IFlowPanel
    {
        public Coroutine StartFlow(RaceStatus status);
    }
}