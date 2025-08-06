using UIs.Base;
using Attributes;
using UIs.Panels;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ManagerSystem;
using Unity.VisualScripting;
using DG.Tweening;

namespace UIs
{
    public class ResultUI : PanelUI
    {
        // Components
        [Bind("ScoreText")] private TextMeshProUGUI _scoreText; // 점수 텍스트
        [Bind("Content")] private Transform _scrollContentBox; // 스크롤 컨텐츠 박스

        private List<ResultKimbapUI> _resultKimbapUIs = new List<ResultKimbapUI>();

        protected override void Initialize()
        {
            base.Initialize();

            // 초기화
            _scoreText.text = $"{0:N0}원";
            _resultKimbapUIs = _scrollContentBox?.GetComponentsInChildren<ResultKimbapUI>()?.ToList();

            foreach (var kimbapUI in _resultKimbapUIs)
            {
                kimbapUI.gameObject.SetActive(false);
            }

            // 서빙된 김밥 UI 설정
            SetSurvedKimbaps();
        }

        public void SetScoreText(int score)
        {
            if (_scoreText == null || score < 0) return;

            // DOTween을 사용하여 점수 텍스트 애니메이션
            // 숫자가 증가하는 애니메이션: $"{score:N0}원"
            _scoreText.text = $"{0:N0}원";

            // 0부터 목표 점수까지 3초 동안 증가하는 애니메이션
            DOVirtual.Int(0, score, 3f, (value) =>
            {
                _scoreText.text = $"{value:N0}원";
            })
            .SetEase(Ease.OutQuart); // 부드러운 감속 효과
        }

        private void SetSurvedKimbaps()
        {
            List<RecipeData> recipes = Managers.InGame.Combination.ServedList;

            int index = 0;
            foreach (RecipeData recipe in recipes)
            {
                if (recipe == null) continue;

                ResultKimbapUI kimbapUI = null;
                if (index >= _resultKimbapUIs.Count)
                {
                    Debug.Log("ResultKimbapUI 개수가 부족하여 생성합니다.");
                    GameObject obj = Managers.Resource.Instantiate("UI/InnerUI/ResultKimbapUI", inParent: _scrollContentBox);
                    kimbapUI = obj.GetOrAddComponent<ResultKimbapUI>();
                    _resultKimbapUIs.Add(kimbapUI);
                }
                else
                {
                    kimbapUI = _resultKimbapUIs[index];
                }

                kimbapUI.gameObject.SetActive(true);
                kimbapUI.SetIngredients(recipe);
                index++;
            }
        }
    }
}