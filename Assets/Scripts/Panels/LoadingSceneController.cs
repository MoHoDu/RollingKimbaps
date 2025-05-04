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
            float percentage = (float)inLoadedCount / inMaxCount;
            float maxWidth = _rect.rect.width;
            statusBar.sizeDelta = new Vector2(maxWidth * percentage, statusBar.sizeDelta.y);
            statusText.text = $"게임에 필요한 데이터를 로드하는 중... ({inLoadedCount}/{inMaxCount})";
        }

        public async UniTaskVoid StartLoadData()
        {
            await UniTask.Delay(1000);

            DataContainer.LoadDataFromSO(SetBarStatusOnLoadSO, TempSetMainScene).Forget();
        }

        private void TempSetMainScene()
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}