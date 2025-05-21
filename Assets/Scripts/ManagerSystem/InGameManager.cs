using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EnumFiles;
using Panels;
using UnityEngine;

namespace ManagerSystem
{
    [Serializable]
    public class InGameStatus
    {
        public bool IsPlaying { get; private set; } = false;
        public bool IsPaused { get; private set; } = false;
        
        public int Score;
        public int Life;
        public float Velocity;
        public List<IngredientData> CollectedIngredients = new();

        public void ClearData()
        {
            IsPlaying = false;
            IsPaused = false;
            
            Score = 0;
            Life = 0;
            Velocity = 0;
            CollectedIngredients.Clear();
        }

        public void StartGame()
        {
            IsPlaying = true;
            IsPaused = false;

            InitVelocity();
        }

        public void InitVelocity()
        {
            Velocity = 2f;
        }

        public void PauseGame()
        {
            IsPaused = true;
        }

        public void ResumeGame()
        {
            IsPaused = false;
        }
    }
    
    public class InGameManager : BaseManager
    {
        private InGameStatus _inGameStatus = new InGameStatus();
        private InputController _inputController;

        private InGamePanelController _inGamePanel;

        public void SetInputController(InputController inputController)
        {
            _inputController = inputController;
            _inputController?.Setup(_inGameStatus);
            _inputController?.InitSettings();
        }

        public void AddEventOnInput(EInputType inType, Action action)
        {
            switch (inType)
            {
                case EInputType.JUMP:
                    _inputController.OnJumped -= action;
                    _inputController.OnJumped += action;
                    break;
                case EInputType.PAUSE:
                    _inputController.OnPause -= action;
                    _inputController.OnPause += action;
                    break;
                case EInputType.RESUME:
                    _inputController.OnResume -= action;
                    _inputController.OnResume += action;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 게임씬으로 변경된 이후에 호출해주세요.
        /// </summary>
        public async UniTaskVoid PlayGame(InGamePanelController controller)
        {
            await UniTask.WaitUntil(() => _inputController != null);
            
            // 초기화 
            _inGameStatus.ClearData();
            _inputController?.InitSettings();
            
            _inGamePanel = controller;
            _inGamePanel.Setup(_inGameStatus);
            
            // key settings
            AddEventOnInput(EInputType.JUMP, _inGamePanel.characterPanel.OnJump);
            
            _inGameStatus.StartGame();
        }
    }
}