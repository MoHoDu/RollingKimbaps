using System;
using System.Collections.Generic;
using EnumFiles;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using ManagerSystem.Base;

namespace ManagerSystem
{
    public class InputManager : MonoBaseManager
    {
        private static InputManager instance;

        public static InputManager Instance
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
        
        // input 시의 이벤트 저장 
        public Dictionary<EInputType, UnityAction> OnInputCallbacks = new();

        // current input info
        private InputValue _currentInput;
        
        protected void Awake()
        {
            instance = this;
            
            Initialize();
        }

        public override void Initialize()
        {
            OnInputCallbacks.Clear();
            foreach (EInputType inputType in Enum.GetValues(typeof(EInputType)))
            {
                OnInputCallbacks.TryAdd(inputType, new UnityAction(() => OnInputDefault(inputType)));
            }
        }

        public void AddEvent(EInputType inputType, UnityAction action)
        {
            if (OnInputCallbacks.TryGetValue(inputType, out var callbacks))
            {
                callbacks -= action; // 중복 방지
                callbacks += action;
                OnInputCallbacks[inputType] = callbacks;
            }
        }
        
        public void RemoveEvent(EInputType inputType, UnityAction action)
        {
            if (OnInputCallbacks.TryGetValue(inputType, out var callbacks))
            {
                callbacks -= action; 
                OnInputCallbacks[inputType] = callbacks;
            }
        }

        private UnityAction GetCallbacks(EInputType inputType)
        {
            return OnInputCallbacks?.GetValueOrDefault(inputType, null);
        }

        public void OnPrimaryTouch(InputValue context)
        {
            bool isReversed = Managers.Save.PlayerSettings.Data.ReverseTouch;

            Vector2 currentTouchPosition = context.Get<Vector2>();
            // 터치 위치가 스크린의 좌측과 우측 중에 어디인지 판별
            if (currentTouchPosition.x < Screen.width / 2)
            {
                // 왼쪽 터치: 점프
                if (isReversed)
                {
                    // 터치가 반전된 경우, 오른쪽 터치로 점프
                    GetCallbacks(EInputType.SUBMIT)?.Invoke();
                }
                else GetCallbacks(EInputType.JUMP)?.Invoke();
            }
            else
            {
                // 오른쪽 터치: 서빙
                if (isReversed)
                {
                    // 터치가 반전된 경우, 왼쪽 터치로 서빙
                    GetCallbacks(EInputType.JUMP)?.Invoke();
                }
                else GetCallbacks(EInputType.SUBMIT)?.Invoke();
            }
        }

        public void OnJump(InputValue context)
        {
            GetCallbacks(EInputType.JUMP)?.Invoke();
        }

        public void OnSubmit(InputValue context)
        {
            GetCallbacks(EInputType.SUBMIT)?.Invoke();
        }

        public void OnPause(InputValue context)
        {
            GetCallbacks(EInputType.PAUSE)?.Invoke();
        }

        public void OnResume(InputValue context)
        {
            GetCallbacks(EInputType.RESUME)?.Invoke();
        }
        
        public void OnLog(InputValue context)
        {
            // Managers.InGame.DebugPraps();
        }

        public void OnTest(InputValue context)
        {
            GetCallbacks(EInputType.TEST)?.Invoke();
        }

        private void OnInputDefault(EInputType inputType)
        {
            #if UNITY_EDITOR
            Debug.Log($"[Info] Get input: {inputType.ToString()}");
            #endif
        }

        public override void OnDestroy()
        {
            OnInputCallbacks.Clear();
        }
    }
}