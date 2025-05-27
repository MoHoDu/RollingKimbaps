using System;
using System.Linq;
using EnumFiles;
using ManagerSystem;
using ManagerSystem.InGame;
using Panels;
using UnityEngine;

namespace InGame
{
    [RequireComponent(typeof(Prap), typeof(Character))]
    public class CharacterHandler : MonoBaseManager
    {
        // 컴포넌트 
        public Prap characterPrap { get; private set; }
        public Character character { get; private set; }
        
        // DI
        InGameManager _inGameManager;
        StatusManager _statusManager;
        private EGameStatus _gameStatus => _statusManager.GameStatus;
        private ECharacterState _characterState => _statusManager.CharacterStatus.State;
        private bool _isGrounded => character.IsGrounded;
        
        // 계산용 값들
        private LayerMask _groundLayer;
        private LayerMask _obstacleLayer;
        private int _inputJumped = 0;
        private readonly int _maxJumped = 2;
        
        // 이벤트 
        public Action OnDeath;

        private void Awake()
        {
            characterPrap = GetComponent<Prap>();
            character = GetComponent<Character>();
            
            _groundLayer = LayerMask.GetMask("ground");
            _obstacleLayer = LayerMask.GetMask("obstacle");
        }

        public override void Initialize(params object[] datas)
        {
            foreach (var data in datas)
            {
                if (data is InGameManager inGameManager)
                {
                    _inGameManager = inGameManager;
                    _statusManager = _inGameManager.Status;
                }
            }
        }

        public void OnStartGame()
        {
            character.OnStart(_statusManager.RaceStatus, _statusManager.CharacterStatus);
        }
        
        public void InputJumpKey()
        {
            if (_gameStatus is not EGameStatus.PLAY) return;

            if (_characterState == ECharacterState.WAITFORREVIE)
            {
                _inputJumped = 0;
                character.Recover();
                _statusManager.CharacterStatus.OnRevived();
            }
            else if (_characterState == ECharacterState.NORMAL)
            {
                GetJump();
            }
        }

        private void GetJump()
        {
            bool isDead = _characterState != ECharacterState.NORMAL;
            
            if (_gameStatus is not EGameStatus.PLAY)
                return;

            if (isDead || _inputJumped >= _maxJumped) return;
            if (!_isGrounded) return;
            _inputJumped++;
            
            character.OnJump();
        }

        private void OnDied()
        {
            // 캐릭터 애니메이션
            character.OnDied().Forget();
            
            // 이벤트 실행 
            OnDeath?.Invoke();
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_gameStatus is not EGameStatus.PLAY) return;
            
            if (IsObstacleCollision(collision))
            {
                OnDied();
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
                        OnDied();
                    }

                    // 오른쪽 벽 판정
                    else if (contact.otherCollider == character.BodyCollider && contact.normal.x < -0.5f)
                    {
                        // Debug.Log("🔵 오른쪽 벽에 닿았어요!");
                        // 오른쪽 벽 충돌 처리
                        OnDied();
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
        
        public override void FixedUpdate()
        {
            if (_gameStatus is not EGameStatus.PLAY) return;
            character?.Rolling();
            character?.SetGravity(_inputJumped);
        }
    }
}