using Cysharp.Threading.Tasks;
using EnumFiles;
using ManagerSystem;
using Panels;
using UnityEngine;

namespace Utils
{
    [RequireComponent(typeof(GameManager))]
    public class TestGame : MonoBehaviour
    {
        private void Start()
        {
            DataContainer.LoadDataFromSO(null, () => PlayGames().Forget()).Forget();
        }

        private async UniTaskVoid PlayGames()
        {
            await UniTask.WaitUntil(() => Managers.DoneInitialized);
            
            Managers.UI.FindCanvasAndGamePanel();
            
            await UniTask.WaitUntil(() => InputManager.Instance != null);
            
            Managers.InGame.InitManagers();
            Managers.InGame.Input.AddEvent(EInputType.TEST, OnTestLogic);
        }

        public void OnTestLogic()
        {
            OnSpawnPrapsAuto();
        }

        private void OnSpawnPrapsAuto()
        {
            Managers.InGame.Prap.TestSpawnLayer();
        }
    }
}