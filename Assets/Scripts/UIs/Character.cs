using Attributes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EnumFiles;
using GameDatas;
using InGame;
using UnityEngine;

namespace UIs
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public class Character : Prap
    {
        // 컴포넌트
        [Bind("Body")] Transform body;
        [Bind("Body")] SpriteRenderer bodyRenderer;
        [Bind("Body")] Collider2D bodyCollider;
        [Bind("HPbarUI")] HPbarUI hpbarUI;
        [Bind("Ingredients")] IngredientsInKimbapUI innerIngredients;
        
        private Rigidbody2D _rigidbody2D;
        private Animator _animator;
        
        // 개방 데이터
        public Collider2D BodyCollider => bodyCollider;
        public bool IsGrounded = false;
        
        // UI 가져오기
        public HPbarUI HPbarUI => hpbarUI;
        public Transform innerParent  => innerIngredients?.transform;

        // DI
        private RaceStatus _raceInfo;
        private CharacterStatus _charInfo;
        
        // 계산용 데이터들
        private float normalGravity;
        private float addedGravity;
        private const float _rotationSpeed = 50f;
        private const float _maxRotationSpeed = 360f; 
        private const float _groundCheckDistance = 0.05f;
        private const float _jumpForce = 25f;
        private const float _addedGravityForce = 2f;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            
            normalGravity = _rigidbody2D.gravityScale;
            addedGravity = _rigidbody2D.gravityScale + _addedGravityForce;
            
            WaitForStart();
        }
        
        public void OnStart(RaceStatus raceStatus, CharacterStatus characterStatus)
        {
            body.gameObject.layer = LayerMask.NameToLayer("invisable_character");
            _charInfo = characterStatus;
            _raceInfo = raceStatus;
            Rebirth(false);
        }

        private void WaitForStart()
        {
            // 위치 초기화
            transform.localPosition = Vector3.zero;
            transform.DOLocalMoveZ(0f, 0);
            
            // 리지드바디 비활성화 
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.simulated = false;
            
            // 콜라이더 Off
            body.gameObject.layer = LayerMask.NameToLayer("invisable_character");
        }

        public void Rebirth(bool setRebirthValue = true)
        {
            // 위치 초기화
            transform.localPosition = Vector3.zero;
            transform.DOLocalMoveZ(0f, 0);

            // 리지드바디 비활성화 
            EnableRigidbody(false);
            
            // 콜라이더 Off
            body.gameObject.layer = LayerMask.NameToLayer("invisable_character");
            
            // 애니메이션 value 세팅 
            if (setRebirthValue) _animator.SetTrigger("rebirth");
            
            // 깜빡이는 애니메이션 시작
            Color baseColor = bodyRenderer.color;
            bodyRenderer.DOFade(0.2f, 0.2f).SetLoops(-1, LoopType.Yoyo).SetId("Blink");
        }

        public void EnableRigidbody(bool isOn)
        {
            if (isOn)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                _rigidbody2D.simulated = true;
            }
            else
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                _rigidbody2D.simulated = false;
            }
        }

        public async UniTask OnDamaged()
        {
            body.gameObject.layer = LayerMask.NameToLayer("invisable_character");
            Color baseColor = bodyRenderer.color;
            baseColor.a = 1f;
            Tween tween = bodyRenderer.DOFade(0.2f, 0.2f).SetLoops(5, LoopType.Yoyo);

            await tween.AsyncWaitForCompletion();
            bodyRenderer.color = baseColor;
            body.gameObject.layer = LayerMask.NameToLayer("character");
        }

        public async UniTask OnDied()
        {
            // 소지 재료 초기화
            ClearIngredients();
            
            // 회전값 초기화 
            body.localRotation = Quaternion.identity;
            
            // 애니메이션이 가리지 않도록 위치 조정 
            transform.DOLocalMoveZ(-2f, 0);
            
            // 애니메이션 재생 
            _animator.SetTrigger("onDied");
            
            // 리지드바디 비활성화 
            EnableRigidbody(false);
            
            // 애니메이션이 완료될 때까지 대기
            await UniTask.WaitWhile(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);

            await UniTask.WaitForSeconds(1);
        }

        public void Recover()
        {
            // 이전 효과 제거
            DOTween.Kill("Blink");
            Color finalColor = bodyRenderer.color;
            finalColor.a = 1f;
            bodyRenderer.color = finalColor;
            
            // 리지드바디 정상화
            EnableRigidbody(true);
            
            // 콜라이더 On
            body.gameObject.layer = LayerMask.NameToLayer("character");
            
            // 무적 시간동안만 깜빡임 유지 및 장애물 충돌 무시
            OnDamaged().Forget();
        }

        public void OnJump()
        {
            // 애니메이션 방향이 이상해지지 않도록 로테이션 고정
            body.localRotation = Quaternion.identity;
            
            // 속도를 제겋해서 점프 일관성 유지 
            Vector2 velocity = _rigidbody2D.linearVelocity;
            velocity.y = 0f;
            _rigidbody2D.linearVelocity = velocity;
            
            // 점프력 적용 
            _rigidbody2D.AddForce(new Vector2(0f, _jumpForce), ForceMode2D.Impulse);
            
            // 그라운드 false
            SetGrounded(false);
            
            // 애니메이션 재생
            _animator.SetTrigger("onJump");
        }

        public void SetGrounded(bool isGrounded)
        {
            IsGrounded = isGrounded;
            // 땅에 붙어있는지 애니메이터 변수로 적용 
            _animator.SetBool("isGround", IsGrounded);
        }

        public void SetGravity(int inputJump)
        {
            if (!IsGrounded)
            {
                if (inputJump > 0)
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

        public void Rolling()
        {
            if (!IsGrounded || _charInfo.State == ECharacterState.DIED) return;
            
            float velocity;
            velocity = _raceInfo?.Velocity is null ? 0f : _raceInfo.Velocity;
            
            if (velocity == 0) return;
            
            // 회전 속도 계산
            float rotationSpeed = Mathf.Clamp(velocity * _rotationSpeed, 0f, _maxRotationSpeed);
            rotationSpeed = velocity >= 0 ? -rotationSpeed : rotationSpeed;
            // Z축으로 회전 적용
            body.Rotate(0f, 0f, rotationSpeed * Time.fixedDeltaTime);
        }

        public bool AddIngredient(CollectedIngredient innerObj)
        {
            return innerIngredients.AddIngredient(innerObj);
        }

        public void ClearIngredients()
        {
            innerIngredients.ClearIngredients();
        }

        protected override void FixedUpdate()
        {
            
        }
    }
}