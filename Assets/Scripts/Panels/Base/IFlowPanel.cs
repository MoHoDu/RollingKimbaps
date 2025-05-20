using ManagerSystem;
using UnityEngine;

namespace Panels.Base
{
    public interface IFlowPanel
    {
        public Coroutine StartFlow(InGameStatus status);
    }
}