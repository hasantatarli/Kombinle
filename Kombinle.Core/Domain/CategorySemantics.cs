namespace Kombinle.Core.Domain;

public static class CategorySemantics
{
    public static LayerRole GetLayerRole(Category category)
    {
        if (IsComfortLayer(category))
            return LayerRole.Comfort;

        if (IsStructuredLayer(category))
            return LayerRole.Structure;

        if (IsProtectionLayer(category))
            return LayerRole.Protection;

        return LayerRole.None;
    }
    public static bool IsComfortLayer(Category category)
    {
        return category == Category.Hoodie ||
               category == Category.Cardigan;
    }

    public static bool IsStructuredLayer(Category category)
    {
        return category == Category.Jacket;
    }

    public static bool IsProtectionLayer(Category category)
    {
        return category == Category.LightOuterwear ||
               category == Category.Coat;
    }

    public static bool IsLightLayer(Category category)
    {
        return category == Category.Cardigan ||
               category == Category.Hoodie;
    }

    public static bool IsHeavyLayer(Category category)
    {
        return category == Category.Coat;
    }



    public static bool IsLayer(Category category)
    {
        return GetLayerRole(category) != LayerRole.None;
    }

    public static bool IsTopCategory(Category category)
    {
        return category == Category.Shirt ||
               category == Category.Blouse ||
               category == Category.Tshirt ||
               category == Category.Sweater ||
               category == Category.Hoodie ||
               category == Category.Cardigan;
    }

    public static bool IsBottomCategory(Category category)
    {
        return category == Category.Pants ||
               category == Category.Skirt ||
               category == Category.Jeans;
    }

    public static bool IsFootwearCategory(Category category)
    {
        return category == Category.Shoes ||
               category == Category.Sneakers;
    }

    public static bool IsCorePair(Category a, Category b, bool isDressMode)
    {
        if (isDressMode)
        {
            return (a == Category.Dress && IsFootwearCategory(b)) ||
                   (b == Category.Dress && IsFootwearCategory(a));
        }

        return (IsTopCategory(a) && IsBottomCategory(b)) ||
               (IsTopCategory(b) && IsBottomCategory(a));
    }

    public static bool IsSupportPair(Category a, Category b, bool isDressMode)
    {
        if (isDressMode)
            return false;

        return (IsBottomCategory(a) && IsFootwearCategory(b)) ||
               (IsBottomCategory(b) && IsFootwearCategory(a));
    }
}