using System;
using System.Collections;
using System.Linq;
using Attributes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameDatas;
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
        private int _inputJumped = 0;
        private bool _isDead = true;
        private readonly int _maxJumped = 2;

        private float normalGravity;
        private float addedGravity;

        private float _maxRotationSpeed = 360f; 
        private float _groundCheckDistance = 0.05f;
        private float _jumpForce = 25f;
        private float _addedGravityForce = 2f;
        
        private LayerMask _groundLayer;
        private LayerMask _obstacleLayer;

        public Action OnDeath;

        protected override void Initialize()
        {
            base.Initialize();
            
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _groundLayer = LayerMask.GetMask("ground");
            _obstacleLayer = LayerMask.GetMask("obstacle");
            
            normalGravity = _rigidbody2D.gravityScale;
            addedGravity = _rigidbody2D.gravityScale + _addedGravityForce;
            
            WaitForStart();
        }

        public void Setup(InGameStatus inGameStatus)
        {
            bodyCollider.enabled = false;
            _inGameStatus = inGameStatus;
            Rebirth(false);
        }

        private void WaitForStart()
        {
            if (_inGameStatus != null) return;
            
            // 위치 초기화
            transform.localPosition = Vector3.zero;
            transform.DOLocalMoveZ(0f, 0);
            
            // 리지드바디 비활성화 
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.simulated = false;
            
            // 콜라이더 Off
            bodyCollider.enabled = false;
        }

        public void Rebirth(bool setRebirthValue = true)
        {
            // 위치 초기화
            transform.localPosition = Vector3.zero;
            transform.DOLocalMoveZ(0f, 0);
            
            // 리지드바디 비활성화 
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.simulated = false;
            
            // 콜라이더 Off
            bodyCollider.enabled = false;
            
            // 애니메이션 value 세팅 
            if (setRebirthValue) _animator.SetTrigger("rebirth");
            
            // 깜빡이는 애니메이션 시작
            Color baseColor = bodyRenderer.color;
            bodyRenderer.DOFade(0.2f, 0.2f).SetLoops(-1, LoopType.Yoyo).SetId("Blink");
            
            // 점프 입력 초기화 
            _inputJumped = 0;
            
            // 점프 입력 대기 
            StartCoroutine(WaitForJumpThenRecover());
            
            // 죽음 상태 = false
            _isDead = false;
        }

        public async UniTaskVoid OnDied()
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
            
            // 애니메이션이 완료될 때까지 대기
            await UniTask.WaitWhile(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
            
            // 연결되어 있는 이벤트 실행
            OnDeath?.Invoke();
        }

        private IEnumerator WaitForJumpThenRecover()
        {
            yield return new WaitUntil(() => _inputJumped > 0);
            
            // 깜빡이기 멈추고 알파 복구
            DOTween.Kill("Blink");
            Color finalColor = bodyRenderer.color;
            finalColor.a = 1f;
            bodyRenderer.color = finalColor;
            
            // 리지드바디 정상화
            _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody2D.simulated = true;
            
            // 콜라이더 On
            bodyCollider.enabled = true;
        }

        public void OnJump()
        {
            if (_isDead || _inputJumped >= _maxJumped) return;
            _inputJumped++;
            
            // 애니메이션 방향이 이상해지지 않도록 로테이션 고정
            body.localRotation = Quaternion.identity;
            
            // 속도를 제겋해서 점프 일관성 유지 
            Vector2 velocity = _rigidbody2D.linearVelocity;
            velocity.y = 0f;
            _rigidbody2D.linearVelocity = velocity;
            
            // 점프력 적용 
            _rigidbody2D.AddForce(new Vector2(0f, _jumpForce), ForceMode2D.Impulse);
            
            // 애니메이션 재생 
            SetGrounded(false);
            _animator.SetTrigger("onJump");
        }

        private void Rolling()
        {
            if (!_isGrounded || _isDead) return;
            
            float velocity = _inGameStatus.Velocity;
            if (velocity == 0) return;
            
            // 회전 속도 계산
            float rotationSpeed = Mathf.Clamp(velocity * _maxRotationSpeed, 0f, _maxRotationSpeed);
            // Z축으로 회전 적용
            body.Rotate(0f, 0f, rotationSpeed * -1f * Time.fixedDeltaTime);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_inGameStatus is not { IsPlaying: true } || _inGameStatus.IsPaused) return;
            
            if (IsObstacleCollision(collision))
            {
                OnDied().Forget();
            }
            else if (IsGroundCollision(collision))
            {
                foreach (var contact in collision.contacts)
                {
                    // 바닥 판정
                    if (contact.otherCollider == bodyCollider && contact.normal.y > 0.5f)
                    {
                        // Debug.Log("🟢 바닥에 닿았어요!");
                        SetGrounded(true);
                
                        // 땅에 닿으면 점프 상태 취소 
                        if (_inputJumped > 0) _inputJumped = 0;
                    }

                    // 왼쪽 벽 판정
                    else if (contact.otherCollider == bodyCollider && contact.normal.x > 0.5f)
                    {
                        // Debug.Log("🟡 왼쪽 벽에 닿았어요!");
                        // 왼쪽 벽 충돌 처리
                        OnDied().Forget();
                    }

                    // 오른쪽 벽 판정
                    else if (contact.otherCollider == bodyCollider && contact.normal.x < -0.5f)
                    {
                        // Debug.Log("🔵 오른쪽 벽에 닿았어요!");
                        // 오른쪽 벽 충돌 처리
                        OnDied().Forget();
                    }
                }
            }
            else
            {
                SetGrounded(false);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (_inGameStatus is not { IsPlaying: true } || _inGameStatus.IsPaused) return;
            
            if (IsGroundCollision(collision))
            {
                SetGrounded(false);
            }
        }
        
        private void SetGrounded(bool isGrounded)
        {
            _isGrounded = isGrounded;
            // 땅에 붙어있는지 애니메이터 변수로 적용 
            _animator.SetBool("isGround", _isGrounded);
        }

        private bool IsGroundCollision(Collision2D collision)
        {
            return ((1 << collision.gameObject.layer) & _groundLayer) != 0 && 
                   collision.contacts.Any(contact => contact.otherCollider == bodyCollider);
        }

        private bool IsObstacleCollision(Collision2D collision)
        {
            return ((1 << collision.gameObject.layer) & _obstacleLayer) != 0 && 
                   collision.contacts.Any(contact => contact.otherCollider == bodyCollider);
        }
        
        private void FixedUpdate()
        {
            if (_inGameStatus == null) return;
            if (!_inGameStatus.IsPlaying || _inGameStatus.IsPaused) return;
            
            Rolling();

            if (!_isGrounded)
            {
                if (_inputJumped > 0)
                {
                    if (_rigidbody2D.linearVelocity.y < 0f)
                        _rigidbody2D.gravityScale = addedGravity;
                }
            }
            else
            {
                _rigidbody2D.gravityScale = normalGravity;
            }
        }
    }
}