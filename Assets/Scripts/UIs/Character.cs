using System;
using System.Threading;
using Attributes;
using Audio;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EnumFiles;
using GameDatas;
using InGame;
using UnityEngine;

namespace UIs
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(AudioEmitter))]
    public class Character : Prap
    {
        // 컴포넌트
        [Bind("Body")] Transform body;
        [Bind("Body")] SpriteRenderer bodyRenderer;
        [Bind("Body")] Collider2D bodyCollider;
        [Bind("HPbarUI")] HPbarUI hpbarUI;
        [Bind("Ingredients")] IngredientsInKimbapUI innerIngredients;
        [Bind("coinParticle")] ParticleSystem coinParticle;

        private Rigidbody2D _rigidbody2D;
        private Animator _animator;
        private AudioEmitter _audioEmitter;

        // 개방 데이터
        public Collider2D BodyCollider => bodyCollider;
        public bool IsGrounded = false;

        // UI 가져오기
        public HPbarUI HPbarUI => hpbarUI;
        public Transform innerParent => innerIngredients?.transform;

        // DI
        private RaceStatus _raceInfo;
        private CharacterStatus _charInfo;

        // Cancelation
        private CancellationTokenSource _cancellationOnDamaged;

        // 계산용 데이터들
        private Vector2 rebirthDefaultPosition = new Vector2(0f, -3.8f);
        private readonly float blinkSeconds = 2f;
        private readonly float blinkDuration = 0.2f;
        private int _blinkLoopTime => Mathf.RoundToInt(blinkSeconds * (1f / blinkDuration));
        private float normalGravity;
        private float addedGravity;
        private const float _rotationSpeed = 50f;
        private const float _maxRotationSpeed = 360f;
        private const float _groundCheckDistance = 0.05f;
        private const float _jumpForce = 25f;
        private const float _addedGravityForce = 2f;

        // 이벤트
        public event Action OnRecorvered;

        protected override void Initialize()
        {
            base.Initialize();

            _rigidbody2D = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();

            // 오디오 세팅
            _audioEmitter = GetComponent<AudioEmitter>();
            _audioEmitter.SetAudioType(EAudioType.SFX);

            normalGravity = _rigidbody2D.gravityScale;
            addedGravity = _rigidbody2D.gravityScale + _addedGravityForce;

            WaitForStart();
        }

        public void OnStart(RaceStatus raceStatus, CharacterStatus characterStatus)
        {
            ChangeLayer(false);
            _charInfo = characterStatus;
            _raceInfo = raceStatus;
            Rebirth(setRebirthValue: false).Forget();
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
            ChangeLayer(false);
        }

        public async UniTask Rebirth(Vector3? targetPosition = null, bool setRebirthValue = true)
        {
            // 위치 조정이 될 때까지 대기
            await UniTask.WaitUntil(() => _raceInfo.readyToRevive);

            // 위치 초기화
            if (targetPosition == null)
            {
                targetPosition = rebirthDefaultPosition;
            }
            Vector2 pos = targetPosition.Value;

            transform.position = new Vector3(ScreenScaler.CAM_LEFTSIDE.x - 2f, pos.y, 0f); // 화면 왼쪽 밖에서 시작
            transform.DOLocalMoveZ(0f, 0);

            // 리지드바디 비활성화 
            EnableRigidbody(false);

            // 콜라이더 Off
            ChangeLayer(false);

            // 애니메이션 value 세팅 
            if (setRebirthValue) _animator.SetTrigger("rebirth");

            // 애니메이션이 완료될 때까지 대기
            await UniTask.WaitWhile(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);

            // 다음 부활 지점까지 포물선 이동
            float jumpDuration = 0.5f; // 포물선 이동 시간
            float jumpHeight = 3f; // 포물선 높이
            Vector3 startPos = transform.position;
            Vector3 endPos = pos;

            await DOTween.To(() => 0f, x =>
            {
                float yOffset = jumpHeight * 4 * x * (1 - x); // 포물선 계산
                transform.position = Vector3.Lerp(startPos, endPos, x) + new Vector3(0, yOffset, 0);
            }, 1f, jumpDuration).SetEase(Ease.Linear).AsyncWaitForCompletion();

            _cancellationOnDamaged?.Cancel();
            _cancellationOnDamaged = null;

            // 자동으로 리지드바디 활성화 및 굴러감
            Recover();
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

        public async UniTask OnDamaged(bool restoreLayerAfterBlink = true)
        {
            ChangeLayer(false);
            Color baseColor = bodyRenderer.color;
            baseColor.a = 1f;

            // 2초간 무적 상태 유지 
            Tween tween = bodyRenderer.DOFade(0.2f, 0.2f).SetLoops(_blinkLoopTime, LoopType.Yoyo);

            await tween.AsyncWaitForCompletion();
            bodyRenderer.color = baseColor;

            // 레이어 복구 여부를 매개변수로 결정
            if (restoreLayerAfterBlink)
            {
                ChangeLayer(true);
            }
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
            Color finalColor = bodyRenderer.color;
            finalColor.a = 1f;
            bodyRenderer.color = finalColor;

            // 리지드바디 정상화
            EnableRigidbody(true);

            // 무적 시간동안만 깜빡임 유지 및 장애물 충돌 무시
            ChangeLayer(false);

            _cancellationOnDamaged = new CancellationTokenSource();
            OnDamaged(restoreLayerAfterBlink: true).AttachExternalCancellation(_cancellationOnDamaged.Token).Forget();

            OnRecorvered?.Invoke();
        }

        public void EndInvincibility()
        {
            // 무적 상태 해제 - 레이어를 다시 character로 변경
            ChangeLayer(true);

            // 깜빡임 효과 취소
            _cancellationOnDamaged?.Cancel();
            _cancellationOnDamaged = null;

            // 투명도 정상화
            Color finalColor = bodyRenderer.color;
            finalColor.a = 1f;
            bodyRenderer.color = finalColor;
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

        // 장애물 콜라이더 관리
        private void SetObstacleCollisionIgnore(bool ignore)
        {
            // obstacle 레이어의 모든 콜라이더 찾기
            Collider2D[] allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            int obstacleLayer = LayerMask.NameToLayer("obstacle");

            foreach (Collider2D collider in allColliders)
            {
                if (collider.gameObject.layer == obstacleLayer)
                {
                    Physics2D.IgnoreCollision(bodyCollider, collider, ignore);
                }
            }
        }

        private void ChangeLayer(bool visable = true)
        {
            if (visable)
            {
                gameObject.layer = LayerMask.NameToLayer("character");
                body.gameObject.layer = LayerMask.NameToLayer("character");
                // 장애물과의 물리적 충돌 활성화
                SetObstacleCollisionIgnore(false);
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("invisible_character");
                body.gameObject.layer = LayerMask.NameToLayer("invisible_character");
                // 장애물과의 물리적 충돌 비활성화 (완전히 통과)
                SetObstacleCollisionIgnore(true);
            }
        }

        protected override void FixedUpdate()
        {

        }

        #region Copied Kimbap Animations
        public async UniTask PlayServingAnimation(bool isSuccessed, float duration, Vector3 targetPos)
        {
            // 타겟 위치로 이동
            Vector3 startPos = transform.position;
            startPos = new Vector3(startPos.x, startPos.y, -8.5f); // z 값을 -9로 설정 (카메라가 -10f에 위치하므로)
            targetPos = new Vector3(targetPos.x, targetPos.y, -8.5f); // z 값을 -9로 설정 (재료가 -0.5f ~ -1f 이므로 카메라에 보이도록 함)

            // 포물선 높이 계산 (타겟이 시작점보다 높으면 위로, 낮으면 아래로)
            float heightDifference = targetPos.y - startPos.y;
            float arcHeight = Mathf.Abs(heightDifference) * 0.5f + 2f; // 기본 아크 높이 추가
            if (heightDifference < 0) arcHeight = -arcHeight; // 아래로 가는 경우 음수로

            // 시퀀스 생성
            Sequence sequence = DOTween.Sequence();

            // DOTween을 사용한 포물선 이동
            Tween anim = DOTween.To(() => 0f, x =>
            {
                // 포물선 계산 (y = 4 * height * x * (1 - x))
                float yOffset = arcHeight * 4 * x * (1 - x);
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, x);
                currentPos.y += yOffset;
                transform.position = currentPos;
            }, 1f, duration).SetEase(Ease.Linear);

            sequence.Append(anim);

            // 실패 시에는 점차 사라지는 효과 추가
            if (!isSuccessed)
            {
                sequence.Join(bodyRenderer.DOFade(0f, duration * 0.9f).SetEase(Ease.InQuad));
            }

            // 애니메이션 대기 
            await sequence.AsyncWaitForCompletion();

            // 애니메이션 이후 제거
            this.OnDestroy();
        }
        #endregion
    }
}