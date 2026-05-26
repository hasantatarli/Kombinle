using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Occasions;

namespace Kombinle.Api.Services;

public sealed class ConfigValidationService
{
    public void ValidateCategoryTraits(
        CategoryCatalogService categoryCatalogService)
    {
        var categories = categoryCatalogService.GetAll();

        var knownTraits = categories
            .SelectMany(x => x.Traits ?? [])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var occasion in OccasionCatalog.All())
        {
            foreach (var req in occasion.Value.SlotSet.Requirements)
            {
                foreach (var trait in req.AllowedTraits)
                {
                    if (!knownTraits.Contains(trait))
                    {
                        throw new InvalidOperationException(
                            $"Occasion '{occasion.Key}' slot '{req.Slot}' uses unknown allowedTrait: {trait}");
                    }
                }
            }
        }
    }
}