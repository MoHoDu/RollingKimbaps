using System.Collections;
using UnityEngine;
using Attributes;
using DG.Tweening;
using TMPro;
using UIs.Base;
using UIs.Panels;


namespace UIs.Messages
{
    public class CountMessageUI : PanelUI
    {
        [Bind("Text")] private TextMeshProUGUI _text;

        private readonly float showSpeed = 0.02f;

        protected override void Initialize()
        {
            base.Initialize();

            _text.text = "";
        }

        public override void SetInfosInPanel(params object[] infos)
        {
            float lifeTime = 3f;
            foreach (var info in infos)
            {
                if (info is float time)
                {
                    lifeTime = time;
                }
            }

            StartCoroutine(Popup(lifeTime));
        }

        private Tween TweenShowText(string text)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_text.DOFade(0f, showSpeed))
                    .AppendCallback(() => _text.text = text)
                    .Append(_text.DOFade(1f, showSpeed));
            
            return sequence;
        }

        private Tween ShowTextSlide(string startText = "", float duration = 0.5f)
        {
            Sequence sequence = DOTween.Sequence();

            // 좌측 화면 밖에서 화면 중앙으로 슬라이드 하면서 글자 등장
            if (_text.TryGetComponent<RectTransform>(out RectTransform rect))
            {
                float width = rect.rect.width;

                // 시작 위치: 화면 왼쪽 밖
                rect.pivot = new Vector2(1, 0.5f);
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.anchoredPosition = Vector3.zero;

                // pivot = (0.5, 0.5) / anchorMin = (0.5, 0.5) / anchorMax = (0.5, 0.5)로 서서히 이동
                sequence.Append(rect.DOAnchorPosX(width, duration).SetEase(Ease.OutCubic))
                .AppendCallback(() =>
                {
                    // 텍스트를 중앙으로 이동
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector3.zero;
                });
            }

            return sequence;
        }

        private Tween DisappearTextSlide(float duration = 0.5f)
        {
            Sequence sequence = DOTween.Sequence();

            // 좌측 화면 밖에서 화면 중앙으로 슬라이드 하면서 글자 등장
            if (_text.TryGetComponent<RectTransform>(out RectTransform rect))
            {
                float width = rect.rect.width;

                // 시작 위치: 화면 중앙
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector3.zero;

                // pivot = (1, 0.5) / anchorMin = (1, 0.5) / anchorMax = (1, 0.5)로 서서히 이동
                sequence.Append(rect.DOAnchorPosX(width, duration).SetEase(Ease.OutCubic))
                .AppendCallback(() =>
                {
                    // 텍스트를 화면 우측으로 이동
                    rect.pivot = new Vector2(0f, 0.5f);
                    rect.anchorMin = new Vector2(1f, 0.5f);
                    rect.anchorMax = new Vector2(1f, 0.5f);
                    rect.anchoredPosition = Vector3.zero;
                });
            }

            return sequence;
        }

        private IEnumerator Popup(float inTime)
        {
            _text.text = "";

            int curWaitTime = 0;
            int maxTime = Mathf.FloorToInt(inTime);

            _text.text = (maxTime - curWaitTime).ToString();
            yield return ShowTextSlide().WaitForCompletion();

            while (curWaitTime < maxTime)
            {
                // Update text
                // 보여지는 시간이 0.4초 이므로
                yield return TweenShowText((maxTime - curWaitTime).ToString()).WaitForCompletion();

                curWaitTime++;

                // 1 - 0.4 = 0.6초 대기
                yield return new WaitForSeconds(0.6f);
            }

            yield return TweenShowText("GO!").WaitForCompletion();
            yield return new WaitForSeconds(0.3f);
            yield return DisappearTextSlide().WaitForCompletion();

            Close();
        }
    }
}