using System;
using System.Collections;
using System.Linq;
using Attributes;
using DG.Tweening;
using ManagerSystem;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public class Character : BindUI
    {
        [Bind("Body")] Transform body;
        [Bind("Body")] SpriteRenderer bodyRenderer;
        [Bind("Body")] Collider2D bodyCollider;
        
        private Rigidbody2D _rigidbody2D;
        private Animator _animator;
        private InGameStatus _inGameStatus;
        
        private bool _isGrounded = false;
        private bool _inputJumped = false;
        private bool _isDead = true;

        [SerializeField] private float _maxRotationSpeed = 360f; 
        [SerializeField] private float _groundCheckDistance = 0.05f;
        [SerializeField] private float _jumpForce = 20f;
        [SerializeField] private LayerMask _groundLayer;

        public Action OnDeath;

        protected override void Initialize()
        {
            base.Initialize();
            
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _groundLayer = LayerMask.GetMask("ground");
        }

        public void Setup(InGameStatus inGameStatus)
        {
            _inGameStatus = inGameStatus;

            Rebirth();
        }

        public void Rebirth()
        {
            // 위치 초기화
            transform.localPosition = Vector3.zero;
            
            // 리지드바디 비활성화 
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.simulated = false;
            
            // 깜빡이는 애니메이션 시작
            _animator.SetTrigger("Rebirth");
            Color baseColor = bodyRenderer.color;
            bodyRenderer.DOFade(0.2f, 0.2f).SetLoops(-1, LoopType.Yoyo).SetId("Blink");
            
            // 점프 입력 대기 
            StartCoroutine(WaitForJumpThenRecover());
        }

        public void OnDied()
        {
            // 회전값 초기화 
            body.localRotation = Quaternion.identity;
            
            // 애니메이션이 가리지 않도록 위치 조정 
            transform.DOLocalMoveZ(-2f, 0);
            
            // 애니메이션 재생 
            _animator.SetTrigger("onDied");
            
            // 리지드바디 비활성화 
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.simulated = false;
            
            // 연결되어 있는 이벤트 실행
            OnDeath?.Invoke();
        }

        private IEnumerator WaitForJumpThenRecover()
        {
            yield return new WaitUntil(() => _inputJumped);
            
            // 깜빡이기 멈추고 알파 복구
            DOTween.Kill("Blink");
            Color finalColor = bodyRenderer.color;
            finalColor.a = 1f;
            bodyRenderer.color = finalColor;
            
            // 리지드바디 정상화
            _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody2D.simulated = true;
            
            // 죽음 상태 = false
            _isDead = false;
        }

        public void OnJump()
        {
            if (_inputJumped) return;
            _inputJumped = true;

            if (_isGrounded)
            {
                // 속도를 제겋해서 점프 일관성 유지 
                Vector2 velocity = _rigidbody2D.linearVelocity;
                velocity.y = 0f;
                _rigidbody2D.linearVelocity = velocity;
                
                // 점프력 적용 
                _rigidbody2D.AddForce(new Vector2(0f, _jumpForce), ForceMode2D.Impulse);
                
                // 애니메이션 재생 
                _isGrounded = false;
                _animator.SetTrigger("onJump");
            }
        }

        private void Rolling()
        {
            if (!_isGrounded || _isDead) return;
            
            float velocity = _inGameStatus.Velocity;
            
            // 회전 속도 계산
            float rotationSpeed = Mathf.Clamp(velocity * _maxRotationSpeed, 0f, _maxRotationSpeed);
            // Z축으로 회전 적용
            body.Rotate(0f, 0f, rotationSpeed * -1f * Time.fixedDeltaTime);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsGroundCollision(collision))
            {
                foreach (var contact in collision.contacts)
                {
                    // 바닥 판정
                    if (contact.otherCollider == bodyCollider && contact.normal.y > 0.5f)
                    {
                        // Debug.Log("🟢 바닥에 닿았어요!");
                        _isGrounded = true;
                
                        // 땅에 닿으면 점프 상태 취소 
                        if (_inputJumped) _inputJumped = false;
                    }

                    // 왼쪽 벽 판정
                    else if (contact.otherCollider == bodyCollider && contact.normal.x > 0.5f)
                    {
                        // Debug.Log("🟡 왼쪽 벽에 닿았어요!");
                        // 왼쪽 벽 충돌 처리
                        OnDied();
                    }

                    // 오른쪽 벽 판정
                    else if (contact.otherCollider == bodyCollider && contact.normal.x < -0.5f)
                    {
                        // Debug.Log("🔵 오른쪽 벽에 닿았어요!");
                        // 오른쪽 벽 충돌 처리
                        OnDied();
                    }
                }
            }
            else
            {
                _isGrounded = false;
            }
            
            // 땅에 붙어있는지 애니메이터 변수로 적용 
            _animator.SetBool("isGround", _isGrounded);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (IsGroundCollision(collision))
            {
                _isGrounded = false;
                
                // 땅에 붙어있는지 애니메이터 변수로 적용 
                _animator.SetBool("isGround", _isGrounded);
            }
        }

        private bool IsGroundCollision(Collision2D collision)
        {
            return ((1 << collision.gameObject.layer) & _groundLayer) != 0 && 
                   collision.contacts.Any(contact => contact.otherCollider == bodyCollider);
        }
        
        private void FixedUpdate()
        {
            if (_inGameStatus == null) return;
            if (!_inGameStatus.IsPlaying || _inGameStatus.IsPaused) return;
            
            Rolling();
        }
    }
}