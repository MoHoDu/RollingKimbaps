using UIs.Base;
using Attributes;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ManagerSystem;
using GameDatas;
using ManagerSystem.InGame;

namespace UIs
{
    public class LifeUI : CanvasUI
    {
        [Bind("BG")] private RectTransform _background;         // 생명 아이콘 배경

        private List<Image> _lifeIcons = new List<Image>();     // 생명 아이콘 리스트
        private int _currentLifeCount = 0;                      // 현재 생명 아이콘 개수

        // 고정 값
        private const int _maxLife = 8;                         // 최대 생명 아이콘 개수

        protected override void Initialize()
        {
            base.Initialize();

            // 생명 아이콘을 가져온 뒤 비활성화
            _lifeIcons = _background.GetComponentsInChildren<Image>().ToList();
            for (int i = 0; i < _lifeIcons.Count; i++)
            {
                // 아이콘이 최대 개수를 넘기는 경우 제거
                if (i > _maxLife)
                {
                    Managers.Resource.Destroy(_lifeIcons[i].gameObject);
                    return;
                }

                _lifeIcons[i].gameObject.SetActive(false);
            }
        }

        public override void SetInfoInUI(params object[] infos)
        {
            foreach (var info in infos)
            {
                if (info is StatusManager statusManager)
                {
                    // 의존성 주입
                    statusManager.CharacterStatus.AddEventOnLifeChanged(OnLifeChanged);

                    // characterStatus의 생명 개수에 따라 아이콘을 활성화 
                    int lifeCount = Mathf.Clamp(statusManager.CharacterStatus.Life, 0, _maxLife);
                    OnLifeChanged(lifeCount);
                }
            }
        }

        private void OnLifeChanged(int lifeCount)
        {
            _currentLifeCount = lifeCount;
            // 생명 아이콘을 활성화
            for (int i = 0; i < _lifeIcons.Count; i++)
            {
                if (i < lifeCount)
                {
                    _lifeIcons[i].gameObject.SetActive(true);
                }
                else
                {
                    _lifeIcons[i].gameObject.SetActive(false);
                }
            }
        }
    }    
}