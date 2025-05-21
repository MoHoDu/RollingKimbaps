using System;
using Cysharp.Threading.Tasks;
using Panels.Base;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerSystem
{
    public class InputController : BindUI
    {
        private InGameStatus _status;
        
        public Action OnPause;
        public Action OnResume;
        public Action OnJumped;

        protected override void Initialize()
        {
            base.Initialize();
            InitSettings();
            Managers.InGame.SetInputController(this);
        }
        
        public void Setup(InGameStatus status)
        {
            _status = status;
        }

        public void InitSettings()
        {
            OnPause = null;
            OnResume = null;
            OnJumped = null;
        }

        public void OnJump(InputValue context)
        {
            if (_status != null && _status.IsPlaying && !_status.IsPaused)
            {
                OnJumped?.Invoke();
            }
        }

        public void OnLog(InputValue context)
        {
            Managers.InGame.DebugPraps();
        }
    }
}