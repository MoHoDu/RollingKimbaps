using Panels.Base;
using Attributes;
using TMPro;
using ManagerSystem.InGame;

namespace Panels
{
    public class ScoreUI : CanvasUI
    {
        [Bind("ScoreText")] private TextMeshProUGUI _scoreText; // 점수 텍스트
        [Bind("wonText")] private TextMeshProUGUI _wonText;     // 원 라벨

        public override void SetInfoInPanel(params object[] infos)
        {
            if (!IsBindingDone)
                Awake();

            foreach (var info in infos)
            {
                if (info is StatusManager statusManager)
                {
                    // 상태 매니저의 캐릭터 상태를 통해 점수 업데이트
                    statusManager.OnScoreChanged += UpdateScore;
                    UpdateScore(statusManager.Score);
                }
            }
        }

        private void UpdateScore(int score)
        {
            if (score >= 10000)
            {
                // 점수가 1만점 이상인 경우 wonText를 '만원'으로 설정
                _wonText.text = "만원";
                // 점수를 1만점 단위로 나누어 표시
                float won = score / 10000;
                _scoreText.text = $"{won:0.00}";
            }
            else
            {
                // 점수가 1만점 미만인 경우 wonText를 "원"으로 설정
                _wonText.text = "원";
            }

            // 점수 텍스트 업데이트
            _scoreText.text = $"{score:0,0}";
        }
    }
}