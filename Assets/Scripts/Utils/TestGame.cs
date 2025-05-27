using Cysharp.Threading.Tasks;
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
            // await UniTask.WaitUntil(() => CanvasManager.Instance != null && InGamePanel.Instance != null);
            
            Managers.UI.FindCanvasAndGamePanel();
        }
    }
}