using System;
using EnumFiles;

namespace GameDatas
{
    [Serializable]
    public class CharacterStatus
    {
        public int Life { get; private set; }  = 5;
        public float HP { get; private set; } = 1f;
        public ECharacterState State { get; private set; } = ECharacterState.WAITFORREVIE;
        
        private Action _onDeath;
        
        public CharacterStatus()
        {
            Initialize();
        }

        public void Initialize()
        {
            Life = 5;
            HP = 1f;
        }

        public void OnDamaged(float damage)
        {
            HP -= damage;
            if (HP <= 0)
            {
                HP = 1f;
                OnDied();
            }
        }

        public void OnDied()
        {
            Life--;
            State = ECharacterState.DIED;
            if (Life <= 0)
            {
                _onDeath?.Invoke();
            }
        }

        public void OnRevived()
        {
            HP = 1f;
            State = ECharacterState.WAITFORREVIE;
        }

        public void OnPlay()
        {
            State = ECharacterState.NORMAL;
        }
        
        public void AddEventOnDeath(Action onDeath)
        {
            _onDeath -= onDeath;
            _onDeath += onDeath;
        }
    }
}