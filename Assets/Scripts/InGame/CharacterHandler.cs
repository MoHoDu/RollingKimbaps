using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using EnumFiles;
using ManagerSystem;
using ManagerSystem.InGame;
using UIs;
using UnityEngine;
using ManagerSystem.Base;

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

            if (_characterState == ECharacterState.WAITFORREVIE)
            {
                _inputJumped = 2;
                character.Recover();
                _statusManager.CharacterStatus.OnPlay();
            }
            else if (_characterState == ECharacterState.NORMAL)
            {
                GetJump();
            }
        }
        
        public void InputSubmitKey()
        {
#if UNITY_EDITOR || DEBUG_SERVING
            Debug.Log($"[SERVING] InputSubmitKey called at {Time.time}, GameStatus: {_gameStatus}, CharacterState: {_characterState}");
#endif
            if (_gameStatus is not EGameStatus.PLAY) return;

            if (_characterState == ECharacterState.NORMAL)
            {
#if UNITY_EDITOR || DEBUG_SERVING
                Debug.Log($"[SERVING] Attempting serving with collected ingredients...");
#endif
                // 서빙 처리
                _combinationManager.OnTryServing();

#if UNITY_EDITOR || DEBUG_SERVING
                Debug.Log($"[SERVING] Clearing ingredients after serving attempt");
#endif
                // 서빙 후 재료 제거
                character.ClearIngredients();
            }
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
                character.Rebirth();
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
                    character.Rebirth();
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
            if (_gameStatus is not EGameStatus.PLAY) return;
            character?.Rolling();
            character?.SetGravity(_inputJumped);
        }
    }
}