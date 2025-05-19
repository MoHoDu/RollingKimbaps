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
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
    }

    public enum SkillKey
    {
        LeftSwipe,
        RightSwipe,
        DownSwipe,
    }
}