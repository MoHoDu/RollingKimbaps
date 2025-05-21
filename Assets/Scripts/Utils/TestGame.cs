using Cysharp.Threading.Tasks;
using ManagerSystem;
using UnityEngine;

namespace Utils
{
    [RequireComponent(typeof(GameManager))]
    public class TestGame : MonoBehaviour
    {
        [SerializeField] private GameObject gamePanel;
        
        private void Start()
        {
            PlayGames().Forget();
        }

        private async UniTaskVoid PlayGames()
        {
            await UniTask.WaitUntil(() => Managers.DoneInitialized);
            gamePanel.SetActive(true);
        }
    }
}