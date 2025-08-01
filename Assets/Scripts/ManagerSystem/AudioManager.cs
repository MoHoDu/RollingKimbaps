using ManagerSystem.Base;


public enum EAudioType
{
    BGM,
    SFX
}

public enum EAudioSituation
{
    Character_Jump,
    Character_Land,
    Character_Damaged,
    Character_Died,
    Collect_Ingredient,
    Success_Submit,
    Fail_Submit,
    Break_Obstacle,
    Game_Over,
    System_Error,
    System_Notice,
    System_Alert,
}

namespace ManagerSystem
{
    public class AudioManager : BaseManager
    {
        public override void Initialize()
        {

        }

        public void PlayAudioFromSystem(EAudioType audioType, EAudioSituation audioSituation)
        {

        }

        public void PlayAudioFromEmitter()
        {

        }
    }
}