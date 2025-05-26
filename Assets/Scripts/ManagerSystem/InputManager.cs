using System;
using Cysharp.Threading.Tasks;
using GameDatas;
using Panels.Base;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerSystem
{
    public class InputManager : MonoBaseManager
    {
        private RaceStatus _status;
        
        private static CanvasManager instance;

        public static CanvasManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("[Manager Error] InputManager is null. Please check the inspector.");
                }

                return instance;
            }
        }
        
        public Action OnPause;
        public Action OnResume;
        public Action OnJumped;

        protected void Awake()
        {
            Initialize();
            Managers.InGame.SetInputController(this);
        }
        
        public void Setup(RaceStatus status)
        {
            _status = status;
        }

        public override void Initialize()
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
            // Managers.InGame.DebugPraps();
        }
    }
}