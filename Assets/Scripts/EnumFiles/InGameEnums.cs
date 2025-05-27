namespace EnumFiles
{
    public enum GameType
    {
        Story,
        Infinite,
    }
    
    public enum EMonth
    {
        January = 1,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    public enum EInputType
    {
        JUMP = 0,   // 점프 입력
        SUBMIT = 1, // 서빙
        PAUSE = 2,  // 멈춤
        RESUME = 3, // 재개
    }

    public enum EPrapType
    {
        CHARACTER,
        GROUND,
        OBSTACLE,
        INGREDIENT,
        BACKGROUND,
        FOREOBJECT,
        MIDDLEOBJECT,
        BACKOBJECT
    }

    public enum ECharacterState
    {
        WAITFORREVIE,
        NORMAL,
        DIED
    }
}
