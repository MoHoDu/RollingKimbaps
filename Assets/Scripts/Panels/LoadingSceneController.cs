using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using ManagerSystem;
using Attributes;
using TMPro;
using Panels.Base;


namespace Panels
{
    public class LoadingSceneController : BindUI
    {
        [Bind("Bar")] private RectTransform statusBar;
        [Bind("StatusText")] private TextMeshProUGUI statusText;

        private RectTransform _rect;

        protected override void Initialize()
        {
            _rect = GetComponent<RectTransform>();

            StartLoadData().Forget();
        }

        private void SetBarStatusOnLoadSO(int inLoadedCount, int inMaxCount)
        {
            SetBarStatusOnLoad(inLoadedCount, inMaxCount, "게임에 필요한 데이터를 로드하는 중...");
        }
        
        private void SetBarStatusOnRequestPermissions(int inLoadedCount, int inMaxCount)
        {
            SetBarStatusOnLoad(inLoadedCount, inMaxCount, "권한 요청 중...");
        }

        private async UniTaskVoid QuitApp(int seconds)
        {
            await UniTask.Delay(seconds * 1000);
            Application.Quit();
        }

        private void SetUIOnDeniedPermission(string permission)
        {
            statusText.text = $"{permission} 권한 요청을 거절하였습니다. 앱은 10초 뒤에 종료됩니다.";

            // QuitApp(10).Forget();
        }

        private void SetUIOnDeniedPermissionAndDoNotAskAgain(string permission)
        {
            statusText.text = $"권한 묻지 않음 설정으로 {permission} 권한 요청을 거절하였습니다. 앱은 10초 뒤에 종료됩니다.";
            
            // QuitApp(10).Forget();
        }
        
        private void SetBarStatusOnLoad(int inLoadedCount, int inMaxCount, string inLoadingText)
        {
            float percentage = (float)inLoadedCount / inMaxCount;
            float maxWidth = _rect.rect.width;
            statusBar.sizeDelta = new Vector2(maxWidth * percentage, statusBar.sizeDelta.y);
            statusText.text = $"{inLoadingText} ({inLoadedCount}/{inMaxCount})";
        }

        public async UniTaskVoid StartLoadData()
        {
            AndroidPermissionChecker.SetCallbacks(SetUIOnDeniedPermission, SetUIOnDeniedPermissionAndDoNotAskAgain);
            
            await UniTask.Delay(1000);

            AndroidPermissionChecker.InitPermission(SetBarStatusOnRequestPermissions, (isAccepted) =>
            {
                if (isAccepted)
                    DataContainer.LoadDataFromSO(SetBarStatusOnLoadSO, TempSetMainScene).Forget();
            }).Forget();
        }

        private void TempSetMainScene()
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}