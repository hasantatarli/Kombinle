namespace Kombinle.Core.Domain
{
    public enum Category
    {
        Jacket,
        Blazer,
        Dress,
        Shirt,
        Blouse,
        Tshirt,
        Sweater,
        Pants,
        Skirt,
        Shoes,
        Sneakers,
        Coat,
        Hoodie,
        Cardigan,
        Jeans,
        Bag,
        Tie
    }

    public enum ColorFamily
    {
        Black,
        White,
        Navy,
        Brown,
        Grey,
        Beige,
        Blue,
        Red
    }

    public enum Formality
    {
        Casual = 1,
        Smart = 2,
        Formal = 3
    }

    public enum Slot
    {
        Anchor,
        Top,
        Bottom,
        Shoes,
        Outerwear,
        Accessory
    }

    public enum RequirementLevel
    {
        Hard = 0,      // Olmazsa kombin yok
        Soft = 1,      // Olmazsa olur ama riskli
        Optional = 2   // Olmasa da sorun değil
    }

    public enum ContextHealthLevel
    {
        Good,
        Okay,
        Poor
    }
    public enum ColorCompatibility
    {
        StrongMatch,
        Acceptable,
        WeakMatch,
        Clash
    }
    public enum LayerRole
    {
        None,
        Comfort,
        Structure,
        Protection
    }
}
