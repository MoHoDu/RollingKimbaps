using System.Linq;
using Attributes;
using DG.Tweening;
using ManagerSystem;
using Panels.Base;
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
                DestroyEffect();
            }
        }

        private void DestroyEffect()
        {
            // 콜라이더를 없앰 or 충돌하지 않게 함 
            _collider.enabled = false;
            
            // Dotween을 사용하여 파티클 재생과 동시에 1초동안 스프라이트의 알파 값을 0으로 서서히 줄임
            _spriteRenderer.DOFade(0f, 1f);
            _broken_particle.Play();
        }

        private void OnParticleSystemStopped()
        {
            // 파티클 재생이 종료되면, Managers.Resource.Destroy(this);
            Managers.Resource.Destroy(this.gameObject);
        }

        private bool IsCharacterCollision(Collision2D collision)
        {
            return ((1 << collision.gameObject.layer) & _characterLayer) != 0 && 
                   collision.contacts.Any(contact => contact.otherCollider == _collider);
        }
    }
}