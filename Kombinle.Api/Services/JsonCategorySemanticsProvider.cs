using Kombinle.Core.Domain;

namespace Kombinle.Api.Services
{
    public sealed class JsonCategorySemanticsProvider : ICategorySemanticsProvider
    {
        private readonly Dictionary<Category, CategorySemanticEntry> _map;

        public JsonCategorySemanticsProvider(CategoryCatalogService catalogService)
        {
            _map = catalogService
                .GetAll()
                .Select(x => new
                {
                    Parsed = Enum.TryParse<Category>(x.Id, ignoreCase: true, out var category),
                    Category = category,
                    Item = x
                })
                .Where(x => x.Parsed)
                .ToDictionary(
                    x => x.Category,
                    x => new CategorySemanticEntry(
                        x.Item.Group,
                        x.Item.Traits ?? [],
                        x.Item.Slots ?? []));
        }

        public bool HasTrait(Category category, string trait)
        {
            return _map.TryGetValue(category, out var info)
                   && info.Traits.Contains(
                       trait,
                       StringComparer.OrdinalIgnoreCase);
        }

        public string? GetGroup(Category category)
        {
            return _map.TryGetValue(category, out var info)
                ? info.Group
                : null;
        }

        private sealed record CategorySemanticEntry(
        string Group,
        List<string> Traits,
        List<string> Slots);

        public bool HasSlot(Category category, Slot slot)
        {
            return _map.TryGetValue(category, out var info)
                && info.Slots.Any(x =>
                    string.Equals(x, slot.ToString(), StringComparison.OrdinalIgnoreCase));
        }



    }

}
