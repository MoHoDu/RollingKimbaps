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

        private event Action _onDamaged;
        private event Action _onDeath;

        private event Action<float> _onHPChanged;
        
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
            else
            {
                _onDamaged?.Invoke();
                _onHPChanged?.Invoke(HP);
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
            _onHPChanged?.Invoke(0f);
        }

        public void OnRevived()
        {
            HP = 1f;
            State = ECharacterState.WAITFORREVIE;
            _onHPChanged?.Invoke(HP);
        }

        public void OnPlay()
        {
            State = ECharacterState.NORMAL;
        }
        
        public void AddEventOnDeath(Action eventAction)
        {
            _onDeath -= eventAction;
            _onDeath += eventAction;
        }
        
        public void AddEventOnDamaged(Action eventAction)
        {
            _onDamaged -= eventAction;
            _onDamaged += eventAction;
        }
        
        public void AddEventOnHPChanged(Action<float> eventAction)
        {
            _onHPChanged -= eventAction;
            _onHPChanged += eventAction;
        }
    }
}