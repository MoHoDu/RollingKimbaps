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

        private async UniTask DestroyEffect()
        {
            _collider.enabled = false;
            
            // 페이드 애니메이션 대기
            await _spriteRenderer.DOFade(0f, 1f).AsyncWaitForCompletion();
            
            // 프랍 제거
            Managers.Resource.Destroy(this.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isTriggered) return;
            
            if (IsCharacterCollision(collision))
            {
                isTriggered = true;
                
                OnTriggerd?.Invoke(data);
                DestroyEffect().Forget();
            }
        }
        
        private bool IsCharacterCollision(Collider2D collision)
        {
            return ((1 << collision.gameObject.layer) & _characterLayer) != 0;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            OnDestroyed?.Invoke(spawnedInfo);
        }
    }
}