namespace Kombinle.Core.Domain;

public static class CategorySemantics
{
    // TODO:
    // This semantic map mirrors category_catalog.json.
    // Next step: load immutable category semantics from catalog
    // instead of maintaining this map manually.
    public static ICategorySemanticsProvider Provider { get; set; } = new DefaultCategorySemanticsProvider();

    private static readonly Dictionary<Category, CategorySemanticInfo> Map = new()
    {
        [Category.Blouse] = new("Top", ["Top"], [Slot.Top]),
        [Category.Shirt] = new("Top", ["Top"], [Slot.Top]),
        [Category.Tshirt] = new("Top", ["Top", "Casual"], [Slot.Top]),
        [Category.Sweater] = new("Top", ["Top", "Warm"], [Slot.Top]),
        [Category.Hoodie] = new("Top", ["Top", "Layer", "Comfort", "Light", "Casual"], [Slot.Top, Slot.Anchor]),

        [Category.Cardigan] = new("Layer", ["Layer", "Comfort", "Light"], [Slot.Anchor, Slot.Outerwear]),

        [Category.Pants] = new("Bottom", ["Bottom"], [Slot.Bottom]),
        [Category.Skirt] = new("Bottom", ["Bottom"], [Slot.Bottom]),
        [Category.Jeans] = new("Bottom", ["Bottom", "Casual"], [Slot.Bottom]),

        [Category.Shoes] = new("Shoes", ["Shoes"], [Slot.Shoes]),
        [Category.Sneakers] = new("Shoes", ["Shoes", "Casual"], [Slot.Shoes]),

        [Category.Dress] = new("Dress", ["OnePiece"], [Slot.Anchor]),

        [Category.Jacket] = new("Layer", ["Layer", "Structure"], [Slot.Anchor, Slot.Outerwear]),
        [Category.LightOuterwear] = new("Layer", ["Layer", "Light", "Protection"], [Slot.Outerwear, Slot.Anchor]),
        [Category.Coat] = new("Layer", ["Layer", "Protection", "Heavy"], [Slot.Outerwear]),

        [Category.Bag] = new("Accessory", ["Accessory"], [Slot.Accessory])
    };

    private sealed record CategorySemanticInfo(
        string Group,
        string[] Traits,
        Slot[] Slots
    );

    public static bool HasTrait(Category category, string trait)
    {
        return Map.TryGetValue(category, out var info)
               && info.Traits.Contains(trait, StringComparer.OrdinalIgnoreCase);
    }

    public static string? GetGroup(Category category)
    {
        return Map.TryGetValue(category, out var info)
            ? info.Group
            : null;
    }

    public static string? GetCategoryGroup(Category category)
    {
        return Provider.GetGroup(category);
    }

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
        return Provider.HasTrait(category, "Comfort");
    }

    public static bool IsStructuredLayer(Category category)
    {
        return Provider.HasTrait(category, "Structure");
    }

    public static bool IsProtectionLayer(Category category)
    {
        return Provider.HasTrait(category, "Protection");
    }

    public static bool IsLightLayer(Category category)
    {
        return Provider.HasTrait(category, "Light");
    }

    public static bool IsHeavyLayer(Category category)
    {
        return Provider.HasTrait(category, "Heavy");
    }



    public static bool IsLayer(Category category)
    {
        return GetLayerRole(category) != LayerRole.None;
    }

    public static bool CanFillTopSlot(Category category)
    {
        return Provider.HasSlot(category, Slot.Top);
    }

    public static bool CanFillBottomSlot(Category category)
    {
        return Provider.HasSlot(category, Slot.Bottom);
    }

    public static bool CanFillShoesSlot(Category category)
    {
        return Provider.HasSlot(category, Slot.Shoes);
    }

    public static bool IsCorePair(Category a, Category b, bool isDressMode)
    {
        if (isDressMode)
        {
            return (a == Category.Dress && CanFillShoesSlot(b)) ||
                   (b == Category.Dress && CanFillShoesSlot(a));
        }

        return (CanFillTopSlot(a) && CanFillBottomSlot(b)) ||
               (CanFillTopSlot(b) && CanFillBottomSlot(a));
    }

    public static bool IsSupportPair(Category a, Category b, bool isDressMode)
    {
        if (isDressMode)
            return false;

        return (CanFillBottomSlot(a) && CanFillShoesSlot(b)) ||
               (CanFillBottomSlot(b) && CanFillShoesSlot(a));
    }

    public static bool IsOnePiece(Category category)
    {
        return Provider.HasTrait(category, "OnePiece");
    }

    public static bool IsOuterwear(Category category)
    {
        return Provider.HasSlot(category, Slot.Outerwear);
    }

    public static bool IsAnchorEligible(Category category)
    {
        return Provider.HasSlot(category, Slot.Anchor);
    }

    public static bool HasSlot(Category category, Slot slot)
    {
        return Map.TryGetValue(category, out var info)
            && info.Slots.Contains(slot);
    }

    

}