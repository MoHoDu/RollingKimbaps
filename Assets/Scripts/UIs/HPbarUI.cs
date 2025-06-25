using Attributes;
using DG.Tweening;
using UIs.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UIs
{
    public class HPbarUI : CanvasUI
    {
        [Bind("BG")] private CanvasGroup bg;
        [Bind("HP")] private Image _hp;

        private Sequence seq;

        protected override void Initialize()
        {
            base.Initialize();
            HideHP(0f, 0f);
        }
        
        public void SetHP(float value)
        {
            _hp.fillAmount = value;
            // 데미지를 입은 경우에만 체력바 활성화
            if (_hp.fillAmount >= 1f)
            {
                HideHP(0f);
            }
            else
            {
                // 빈 경우에는 대기 후 꺼짐
                if (_hp.fillAmount <= 0f)
                {
                    HideHP(0.2f);
                }
                else
                {
                    ShowHP(0f);
                }
            }
        }

        public void ShowHP(float waitTime, float duration = 0.2f)
        {
            if (Mathf.Approximately(bg.alpha, 1f)) return;
            
            KillSequence();
            
            seq = DOTween.Sequence();
            seq.AppendInterval(waitTime).Join(bg.DOFade(1f, duration));
            
            seq.Play();
        }

        public void HideHP(float waitTime, float duration = 0.2f)
        {
            if (Mathf.Approximately(bg.alpha, 0f)) return;

            KillSequence();
            
            seq = DOTween.Sequence();
            seq.AppendInterval(waitTime).Join(bg.DOFade(0f, duration));
            
            seq.Play();
        }

        private void KillSequence()
        {
            if (seq != null)
            {
                seq.Kill();
                seq = null;
            }
        }

        private void OnDestroy()
        {
            if (seq != null) seq.Kill();
            seq = null;
        }
    }
}