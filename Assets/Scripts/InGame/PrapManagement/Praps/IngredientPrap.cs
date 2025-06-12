using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InGame.Combination;
using ManagerSystem;
using UnityEngine;

namespace InGame.PrapManagement.Praps
{
    public class IngredientPrap : Prap
    {
        // Components
        
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;
        
        // DI
        private IngredientData data;
        private SpawnedIngredient spawnedInfo;
        
        // values
        private LayerMask _characterLayer;
        private bool isTriggered = false;
        
        public event Action<IngredientData> OnTriggerd;
        public event Action<SpawnedIngredient> OnDestroyed; 

        protected override void Initialize()
        {
            base.Initialize();
            
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _characterLayer = LayerMask.GetMask("character");
        }
        
        public override void OnSpawned(params object[] args)
        {
            foreach (var arg in args)
            {
                if (arg is IngredientData ingredientData)
                {
                    data = ingredientData;
                }
                else if (arg is SpawnedIngredient info)
                {
                    spawnedInfo = info;
                }
            }
            
            base.OnSpawned(args);
        }

        private void DestroyEffect()
        {
            _collider.enabled = false;
            
            // 페이드 애니메이션 대기
            // 시퀀스 생성
            var seq = DOTween.Sequence();

            // 1) 약간 팝업(확대)
            seq.Append(transform.DOScale(2f, 0.15f)
                .SetEase(Ease.OutBack));

            // 2) 축소와 페이드아웃을 동시에
            seq.Append(transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InQuad));
            seq.Join(_spriteRenderer.DOFade(0f, 0.3f));

            // 완료 콜백
            seq.OnComplete(() =>
            {
                // 프랍 제거
                Managers.Resource.Destroy(this.gameObject);
            });
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isTriggered) return;
            // LayerMask 검사
            if ((_characterLayer.value & (1 << collision.gameObject.layer)) == 0)
                return;
            isTriggered = true;
            
            OnTriggerd?.Invoke(data);
            DestroyEffect();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            OnDestroyed?.Invoke(spawnedInfo);
        }
    }
}