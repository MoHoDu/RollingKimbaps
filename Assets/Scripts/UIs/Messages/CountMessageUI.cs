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

        private IEnumerator Popup(float inTime)
        {
            _text.text = "";

            int curWaitTime = 0;
            int maxTime = Mathf.FloorToInt(inTime);
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

            Close();
        }
    }
}