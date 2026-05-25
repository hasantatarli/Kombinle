using Kombinle.Core.Domain;

namespace Kombinle.Core.Generation
{
    internal static class SlotRequirementMatcher
    {
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

            return categoryMatch || traitMatch;
        }
    }
}
