using UIs.Base;
using Attributes;
using TMPro;
using ManagerSystem.InGame;
using DG.Tweening;
using UnityEngine;

namespace UIs
{
    public class ScoreUI : CanvasUI
    {
        [Bind("ScoreText")] private TextMeshProUGUI _scoreText; // 점수 텍스트
        [Bind("priceText")] private TextMeshProUGUI _priceText; // 추가 가격 텍스트
        [Bind("tipText")] private TextMeshProUGUI _tipText; // 추가 팁 텍스트
        [Bind("priceTextBG")] private RectTransform _priceTextBG; // 가격 텍스트 배경
        [Bind("tipTextBG")] private RectTransform _tipTextBG; // 팁

        private Tween _scoreTween;
        private Sequence _priceSequence;
        private Sequence _tipSequence;
        private int _currentScore = 0;

        public override void SetInfoInUI(params object[] infos)
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
                else if (info is CombinationManager combinationManager)
                {
                    // 이벤트 연결
                    combinationManager.onSuccessedServing += (score) => ViewAddPriceAnim(score.rewards, score.tips);
                }
            }

            // 초기화
            _priceText.text = "";
            _tipText.text = "";

            _priceText.rectTransform.pivot = new Vector2(0f, 1f);
            _tipText.rectTransform.pivot = new Vector2(0f, 1f);
        }

        public void OnChangedReverseUI(bool isReversed)
        {
            // 애니메이션 Kill
            _scoreTween?.Kill(false);
            _priceSequence?.Kill(false);
            _tipSequence?.Kill(false);

            if (!isReversed)
            {
                _priceTextBG.anchorMin = new Vector2(1f, 0.5f);
                _priceTextBG.anchorMax = new Vector2(1f, 0.5f);
                _priceTextBG.pivot = new Vector2(0f, 0.5f);
                _priceTextBG.anchoredPosition = new Vector2(25f, 25f);
                _priceText.alignment = TextAlignmentOptions.Left;

                _tipTextBG.anchorMin = new Vector2(1f, 0.5f);
                _tipTextBG.anchorMax = new Vector2(1f, 0.5f);
                _tipTextBG.pivot = new Vector2(0f, 0.5f);
                _tipTextBG.anchoredPosition = new Vector2(25f, -25f);
                _tipText.alignment = TextAlignmentOptions.Left;
            }
            else
            {
                _priceTextBG.anchorMin = new Vector2(0f, 0.5f);
                _priceTextBG.anchorMax = new Vector2(0f, 0.5f);
                _priceTextBG.pivot = new Vector2(1f, 0.5f);
                _priceTextBG.anchoredPosition = new Vector2(-25f, 25f);
                _priceText.alignment = TextAlignmentOptions.Right;

                _tipTextBG.anchorMin = new Vector2(0f, 0.5f);
                _tipTextBG.anchorMax = new Vector2(0f, 0.5f);
                _tipTextBG.pivot = new Vector2(1f, 0.5f);
                _tipTextBG.anchoredPosition = new Vector2(-25f, -25f);
                _tipText.alignment = TextAlignmentOptions.Right;
            }
        }

        private void UpdateScore(int newScore)
        {
            _scoreTween?.Kill();

            int startScore = _currentScore;
            int endScore = newScore;
            _scoreTween = DOVirtual.Int(startScore, endScore, 1.5f, value =>
            {
                _currentScore = value;
                _scoreText.text = FormatScore(value);
            });
        }

        private void ViewAddPriceAnim(int price, int tip)
        {
            _priceText.text = FormatScore(price);
            _tipText.text = FormatScore(tip);

            // 가격과 팁 텍스트 애니메이션 Kill
            _priceSequence?.Kill(false);
            _tipSequence?.Kill(false);

            // 가격과 팁 텍스트 준비
            string priceText = $"+ 판매 {FormatScore(price)}";
            string tipText = $"+ 팁 보너스 {FormatScore(tip)}";

            // 가격과 팁 텍스트 애니메이션
            _priceSequence = DOTween.Sequence();
            _priceSequence
                .AppendCallback(() => _priceText.text = priceText)
                .Append(_priceText.rectTransform.DOPivotY(0f, 0.3f).SetEase(Ease.OutBack))
                // 딜레이 0.3초
                .AppendInterval(0.3f)
                .Append(_priceText.rectTransform.DOPivotY(1f, 0.3f).SetEase(Ease.InBack));

            _tipSequence = DOTween.Sequence();
            _tipSequence
                .AppendCallback(() => _tipText.text = tipText)
                .Append(_tipText.rectTransform.DOPivotY(0f, 0.3f).SetEase(Ease.OutBack))
                // 딜레이 0.3초
                .AppendInterval(0.3f)
                .Append(_tipText.rectTransform.DOPivotY(1f, 0.3f).SetEase(Ease.InBack));
        }

        private string FormatScore(int score)
        {
            string s = score.ToString();
            int len = s.Length;

            if (len > 9)
                return $"{s.Substring(0, len - 9)}.{s.Substring(len - 9, 2)}B";
            else if (len > 6)
                return $"{s.Substring(0, len - 6)}.{s.Substring(len - 6, 2)}M";
            else if (len > 3)
                return $"{s.Substring(0, len - 3)}.{s.Substring(len - 3, 2)}K";
            else
                return s;
        }

        private void OnDestroy()
        {
            _scoreTween?.Kill();
            _scoreTween = null;

            _priceSequence?.Kill(false);
            _tipSequence?.Kill(false);
        }
    }
}