using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using EnumFiles;
using ManagerSystem;
using ManagerSystem.InGame;
using UIs;
using UnityEngine;
using ManagerSystem.Base;
using UIs.Base;

namespace InGame
{
    [RequireComponent(typeof(Character))]
    public class CharacterHandler : MonoBaseManager
    {
        // 컴포넌트 
        public Character character { get; private set; }

        // DI
        InGameManager _inGameManager;
        StatusManager _statusManager;
        CombinationManager _combinationManager;
        private EGameStatus _gameStatus => _statusManager.GameStatus;
        private ECharacterState _characterState => _statusManager.CharacterStatus.State;
        private bool _isGrounded => character.IsGrounded;

        // 계산용 값들
        private LayerMask _groundLayer;
        private LayerMask _obstacleLayer;
        private LayerMask _deadzoneLayer;
        private LayerMask _ingredientLayer;
        private int _inputJumped = 0;
        private readonly int _maxJumped = 2;

        // 이벤트 
        public event Action OnDeath;
        public event Action OnRevive;

        private void Awake()
        {
            character = GetComponent<Character>();
            if (character) character.OnRecorvered += OnRecorvered;

            _groundLayer = LayerMask.GetMask("ground");
            _obstacleLayer = LayerMask.GetMask("obstacle");
            _deadzoneLayer = LayerMask.GetMask("deadzone");
            _ingredientLayer = LayerMask.GetMask("ingredient");
        }

        public override void Initialize(params object[] datas)
        {
            foreach (var data in datas)
            {
                if (data is InGameManager inGameManager)
                {
                    _inGameManager = inGameManager;
                    _statusManager = _inGameManager.Status;
                    _combinationManager = _inGameManager.Combination;
                }
            }
        }

        public override void OnStartGame()
        {
            if (character.HPbarUI != null)
            {
                _statusManager?.CharacterStatus?.AddEventOnHPChanged(character.HPbarUI.SetHP);
            }

            character.OnStart(_statusManager.RaceStatus, _statusManager.CharacterStatus);
        }

        public void InputJumpKey()
        {
            if (_gameStatus is not EGameStatus.PLAY) return;

            if (_characterState == ECharacterState.NORMAL)
            {
                GetJump();
            }
        }

        public void InputSubmitKey()
        {
            if (_gameStatus is not EGameStatus.PLAY) return;

            if (_characterState == ECharacterState.NORMAL)
            {
                // 서빙 처리
                _combinationManager.OnTryServing();

                // 서빙 후 재료 제거
                character.ClearIngredients();
            }
        }

        public async UniTask PlayServingAnimation(bool isSuccessed)
        {
            // 연출을 위한 복제 캐릭터 생성
            Character cloneCharacter = GameObject.Instantiate(character);
            cloneCharacter.transform.SetParent(character.transform.parent);
            cloneCharacter.transform.position = character.transform.position;

            // 서빙 애니메이션 실행
            Vector3 targetPos = new Vector3(0f, 0f, -8.5f); // 기본 위치
            if (isSuccessed)
            {
                CanvasUI scoreUI = Managers.UI.GetUI<ScoreUI>();
                if (scoreUI != null)
                {
                    targetPos = Camera.main.ScreenToWorldPoint(scoreUI.transform.position);
                    targetPos += new Vector3(3f, -0.5f, 0f); // 좌상단 기준이므로 살짝 조정
                }
                else
                {
                    float speed = _statusManager.RaceStatus.Velocity == 0 ? 0f : _statusManager.RaceStatus.Velocity / _statusManager.RaceStatus.MaxVelocity;
                    float speedPercent = Mathf.Clamp(speed, 0f, 1f);
                    float targetX = Mathf.Lerp(5f, 15f, speedPercent);
                    targetPos = new Vector3(targetX, 15f, -8.5f);
                }
            }
            else
            {
                CanvasUI trashUI = Managers.UI.GetUI<TrashPointUI>();
                if (trashUI != null)
                {
                    targetPos = Camera.main.ScreenToWorldPoint(trashUI.transform.position);
                    targetPos = new Vector3(targetPos.x, targetPos.y, -8.5f);
                }
                else
                {
                    float speed = _statusManager.RaceStatus.Velocity == 0 ? 0f : _statusManager.RaceStatus.Velocity / _statusManager.RaceStatus.MaxVelocity;
                    float speedPercent = Mathf.Clamp(speed, 0f, 1f);
                    float targetX = Mathf.Lerp(-6f, -2f, speedPercent);
                    targetPos = new Vector3(targetX, -15f, -8.5f);
                }
            }

            // 애니메이션 실행
            await cloneCharacter.PlayServingAnimation(isSuccessed, 1f, targetPos);
        }

        private void GetJump()
        {
            bool isDead = _characterState != ECharacterState.NORMAL;

            if (_gameStatus is not EGameStatus.PLAY)
                return;

            if (isDead || _inputJumped >= _maxJumped) return;
            // 처음 점프는 반드시 땅에서만 실행 되도록 함 
            if (_inputJumped == 0 && !_isGrounded) return;
            _inputJumped++;

            character.OnJump();
        }

        private void OnRecorvered()
        {
            _inputJumped = 2;
            _statusManager.CharacterStatus.OnPlay();
        }

        private async UniTaskVoid OnDied()
        {
            if (_characterState is not ECharacterState.NORMAL) return;

            // 이벤트 실행 
            OnDeath?.Invoke();

            // 캐릭터 애니메이션
            await character.OnDied();

            if (_statusManager.CharacterStatus.Life > 0)
            {
                OnRevive?.Invoke();
                character.Rebirth().Forget();
            }
        }

        private async UniTaskVoid OnDamaged(float inDamage)
        {
            if (_characterState is not ECharacterState.NORMAL) return;

            _statusManager.CharacterStatus.OnDamaged(inDamage);
            if (_statusManager.CharacterStatus.State == ECharacterState.DIED)
            {
                // 캐릭터 애니메이션
                await character.OnDied();

                if (_statusManager.CharacterStatus.Life > 0)
                {
                    OnRevive?.Invoke();
                    character.Rebirth().Forget();
                }
            }
            else
            {
                // 캐릭터 깜빡임 + 잠시 장애물 통과
                await character.OnDamaged();
            }
        }

        public void OnPaused()
        {
            character?.EnableRigidbody(false);
        }

        public void OnResumed()
        {
            character?.EnableRigidbody(true);
        }

        public void OnCollectedIngredient(IngredientData data)
        {
            string prefabPath = data.innerPath;
            GameObject go = Managers.Resource.Instantiate(prefabPath, character.innerParent);
            if (go.TryGetComponent<CollectedIngredient>(out var ci))
            {
                ci.Data = data;
                if (!character.AddIngredient(ci))
                {
                    Managers.Resource.Destroy(go);
                }
            }
            else Managers.Resource.Destroy(go);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_gameStatus is not EGameStatus.PLAY) return;

            // 장애물 및 지형 충돌 여부 확인
            if (InDeadZone(collision))
            {
                OnDied().Forget();
            }
            else if (IsObstacleCollision(collision))
            {
                OnDamaged(0.5f).Forget();
            }
            else if (IsGroundCollision(collision))
            {
                foreach (var contact in collision.contacts)
                {
                    // 바닥 판정
                    if (contact.otherCollider == character.BodyCollider && contact.normal.y > 0.5f)
                    {
                        // Debug.Log("🟢 바닥에 닿았어요!");
                        character.SetGrounded(true);

                        // 땅에 닿으면 점프 상태 취소 
                        if (_inputJumped > 0) _inputJumped = 0;
                    }

                    // 왼쪽 벽 판정
                    else if (contact.otherCollider == character.BodyCollider && contact.normal.x > 0.5f)
                    {
                        // Debug.Log("🟡 왼쪽 벽에 닿았어요!");
                        // 왼쪽 벽 충돌 처리
                        OnDied().Forget();
                    }

                    // 오른쪽 벽 판정
                    else if (contact.otherCollider == character.BodyCollider && contact.normal.x < -0.5f)
                    {
                        // Debug.Log("🔵 오른쪽 벽에 닿았어요!");
                        // 오른쪽 벽 충돌 처리
                        OnDied().Forget();
                    }
                }
            }
            else
            {
                character.SetGrounded(false);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (_gameStatus is not EGameStatus.PLAY) return;

            if (IsGroundCollision(collision))
            {
                character.SetGrounded(false);
            }
        }

        private bool IsGroundCollision(Collision2D collision)
        {
            return ((1 << collision.gameObject.layer) & _groundLayer) != 0 &&
                   collision.contacts.Any(contact => contact.otherCollider == character.BodyCollider);
        }

        private bool IsObstacleCollision(Collision2D collision)
        {
            // 캐릭터가 무적 상태(invisible_character 레이어)일 때는 장애물과 충돌하지 않음
            if (character.BodyCollider.gameObject.layer == LayerMask.NameToLayer("invisible_character"))
                return false;

            return ((1 << collision.gameObject.layer) & _obstacleLayer) != 0 &&
                   collision.contacts.Any(contact => contact.otherCollider == character.BodyCollider);
        }

        private bool InDeadZone(Collision2D collision)
        {
            return ((1 << collision.gameObject.layer) & _deadzoneLayer) != 0 &&
                   collision.contacts.Any(contact => contact.otherCollider == character.BodyCollider);
        }

        public override void FixedUpdate()
        {
            if (_statusManager == null) return;
            if (_gameStatus is not EGameStatus.PLAY) return;
            character?.Rolling();
            character?.SetGravity(_inputJumped);
        }
    }
}