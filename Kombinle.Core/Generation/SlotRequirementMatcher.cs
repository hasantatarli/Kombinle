using Kombinle.Core.Domain;

namespace Kombinle.Core.Generation
{
    public static class SlotRequirementMatcher
    {
        // Matching dimensions:
        //
        // allowedCategories:
        //   Explicit whitelist for exact garment categories.
        //
        // allowedTraits:
        //   Semantic behavior matching
        //   (e.g. Casual, Structure, Top-like behavior).
        //
        // allowedSlots:
        //   Outfit composition eligibility
        //   (e.g. can fill Anchor / Outerwear / Shoes slot).
        //
        // A garment matching any configured dimension is considered eligible.
        public static bool Matches(
            Garment garment,
            SlotRequirement requirement)
        {
            var categoryMatch =
                requirement.AllowedCategories.Count > 0 &&
                requirement.AllowedCategories.Contains(garment.Category);

            var traitMatch =
                requirement.AllowedTraits.Count > 0 &&
                requirement.AllowedTraits.Any(trait =>
                    CategorySemantics.Provider.HasTrait(
                        garment.Category,
                        trait));

            var slotMatch = requirement.AllowedSlots.Count > 0 && requirement.AllowedSlots.Any(slot => CategorySemantics.Provider.HasSlot(garment.Category, slot));

            return categoryMatch || traitMatch || slotMatch;
        }
    }
}
