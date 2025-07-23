using System.Linq;
using Attributes;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using ManagerSystem;
using UIs.Base;
using UnityEngine;

namespace Obstacles
{
    public class Obstacle : BindUI
    {
        [Bind("broken_particle")] ParticleSystem _broken_particle;

        [SerializeField] public float Width = 1;
        [SerializeField] public float Height = 1;

        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;
        private LayerMask _characterLayer;

        protected override void Initialize()
        {
            base.Initialize();

            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _characterLayer = LayerMask.GetMask("character");

            // 파티클 종료 후에 콜백 설정 
            var particle_main = _broken_particle.main;
            particle_main.stopAction = ParticleSystemStopAction.Callback;

            Width = _spriteRenderer.sprite.bounds.size.x;
            Height = _spriteRenderer.sprite.bounds.size.y;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsCharacterCollision(collision))
            {
                DestroyEffect().Forget();
            }
        }

        private async UniTaskVoid DestroyEffect()
        {
            // prevent duplicate triggers
            _collider.enabled = false;
            // await the fade tween completion via DOTween Async extension
            await _spriteRenderer.DOFade(0f, 1f).AsyncWaitForCompletion();
            // after fade completes, destroy this game object
            Managers.Resource.Destroy(this.gameObject);
        }

        private void OnParticleSystemStopped()
        {
            // 파티클 재생이 종료되면, Managers.Resource.Destroy(this);
            Managers.Resource.Destroy(this.gameObject);
        }

        private bool IsCharacterCollision(Collision2D collision)
        {
            // 캐릭터가 무적 상태(invisible_character 레이어)일 때는 충돌하지 않음
            int characterLayer = LayerMask.NameToLayer("character");
            int invisibleCharacterLayer = LayerMask.NameToLayer("invisible_character");

            bool isCharacterLayer = collision.gameObject.layer == characterLayer;
            bool isInvisibleCharacterLayer = collision.gameObject.layer == invisibleCharacterLayer;

            // invisible_character 레이어면 충돌 무시
            if (isInvisibleCharacterLayer) return false;

            return isCharacterLayer &&
                   collision.contacts.Any(contact => contact.otherCollider == _collider);
        }
    }
}