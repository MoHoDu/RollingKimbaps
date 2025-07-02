using System.Collections;
using UnityEngine;
using Attributes;
using DG.Tweening;
using TMPro;
using UIs.Base;
using UIs.Panels;


namespace UIs.Messages
{
    public class PopupMessageUI : PanelUI
    {
        [Bind("Text")] private TextMeshProUGUI _text;

        private RectTransform _rect;
        private readonly float animSpeed = 0.5f;
        private readonly float typingSpeed = 0.05f;

        protected override void Initialize()
        {
            base.Initialize();

            _text.text = "";
            _rect = GetComponent<RectTransform>();
        }

        public override void SetInfosInPanel(params object[] infos)
        {
            float lifeTime = 3f;
            string text = "";
            foreach (var info in infos)
            {
                if (info is float time)
                {
                    lifeTime = time;
                }
                else if (info is string message)
                {
                    text = message;
                }
            }

            StartCoroutine(Popup(text, lifeTime));
        }

        private Tween OnOffUIAnim(bool isOn)
        {
            Vector3 lastPosition = _rect.anchoredPosition;
            if (isOn)
            {
                _rect.anchoredPosition = new Vector2(0f, _rect.sizeDelta.y);
            }
            else
            {
                lastPosition = new Vector2(0f, _rect.sizeDelta.y);
            }

            return _rect.DOAnchorPos(lastPosition, animSpeed);
        }

        private IEnumerator Popup(string inText, float inTime)
        {
            _text.text = "";

            Tween openAnim = OnOffUIAnim(true);
            openAnim.Play();
            yield return openAnim.WaitForCompletion();

            char[] characters = inText.ToCharArray();
            int index = 0;
            while (_text.text.Length < inText.Length)
            {
                if (characters.Length == 0 || index == characters.Length - 1)
                {
                    _text.text = inText;
                    break;
                }
                _text.text += characters[index];
                index++;

                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(inTime);

            Tween endAnim = OnOffUIAnim(false);
            endAnim.Play();
            yield return endAnim.WaitForCompletion();

            Close();
        }
    }
}