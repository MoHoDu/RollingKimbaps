namespace EnumFiles
{
    public enum EFileType
    {
        None,
        SO,
        Json,
        PlayerPref,
    }
    
    public enum EDataType
    {
        SaveFile,
        Ingredient,
        Recipe,
        Skill,
        Prap
    }

    public enum IngredientType
    {
        Vegetable,
        Meat,
        Seafood,
        Egg,
        Dairy,
        Grains,
        Fruit,
        Spice,
        Herb,
        Condiment,
        Seasoning,
        Sauce,
        Beverage,
        Snack,
        Dessert,
        Staple,
    }

    public enum Rarity
    {
        Common = 100,
        Uncommon = 500,
        Rare = 800,
        Epic = 1000,
        Legendary = 2000,
    }

    public enum SkillKey
    {
        LeftSwipe,
        RightSwipe,
        DownSwipe,
    }
}