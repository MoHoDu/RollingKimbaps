using System;
using Cysharp.Threading.Tasks;
using EnumFiles;
using GameDatas;
using JsonData;
using Panels;
using UnityEngine;
using Utils;
using GameDatas;

namespace ManagerSystem
{
    public class InGameManager : BaseManager
    {
        public GroundBuilder GroundBuilder => _groundBuilder;
        
        private InGameStatus _inGameStatus = new InGameStatus();
        private InputController _inputController;

        private InGamePanelController _inGamePanel; 
        private GroundBuilder _groundBuilder = new GroundBuilder();

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
        public async UniTaskVoid PlayGame(InGamePanelController controller, Transform groundTr)
        {
            await UniTask.WaitUntil(() => _inputController != null);
            
            // 초기화 
            _inGameStatus.ClearData();
            _inputController?.InitSettings();
            
            _inGamePanel = controller;
            _inGamePanel.Setup(_inGameStatus);
            
            // key settings
            AddEventOnInput(EInputType.JUMP, _inGamePanel.characterPanel.OnJump);
            
            _inGameStatus.StartGame(groundTr);
        }

        public void SetData(SaveData saveData)
        {
            foreach (var id in saveData.recipeID)
            {
                RecipeData recipe = DataContainer.RecipeDatas.Get(id);

                if (recipe != null) _inGameStatus.CollectedRecipes.Add(recipe);
            }
        }
    }
}