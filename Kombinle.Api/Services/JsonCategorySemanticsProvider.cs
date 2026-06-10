using Kombinle.Core.Domain;

namespace Kombinle.Api.Services
{
    public sealed class JsonCategorySemanticsProvider : ICategorySemanticsProvider
    {
        private readonly Dictionary<string, CategorySemanticEntry> _map;

        public JsonCategorySemanticsProvider(CategoryCatalogService catalogService)
        {
            var items = catalogService.GetAll();

            _map = new Dictionary<string, CategorySemanticEntry>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Id))
                    throw new InvalidOperationException("Category catalog item has empty id.");

                if (string.IsNullOrWhiteSpace(item.Group))
                    throw new InvalidOperationException($"Category '{item.Id}' has empty group.");

                if (item.Traits == null)
                    throw new InvalidOperationException($"Category '{item.Id}' has null traits.");

                if (item.Slots == null)
                    throw new InvalidOperationException($"Category '{item.Id}' has null slots.");

                foreach (var slot in item.Slots)
                {
                    if (!Enum.TryParse<Slot>(slot, ignoreCase: true, out _))
                        throw new InvalidOperationException(
                            $"Category '{item.Id}' has invalid slot: {slot}");
                }

                _map[item.Id] = new CategorySemanticEntry(
                    item.Group,
                    item.Traits,
                    item.Slots);
            }
        }

        public bool HasTrait(string categoryId, string trait)
        {
            return _map.TryGetValue(categoryId, out var info)
                   && info.Traits.Contains(
                       trait,
                       StringComparer.OrdinalIgnoreCase);
        }

        public string? GetGroup(string categoryId)
        {
            return _map.TryGetValue(categoryId, out var info)
                ? info.Group
                : null;
        }

        public bool HasSlot(string categoryId, Slot slot)
        {
            return _map.TryGetValue(categoryId, out var info)
                   && info.Slots.Any(x =>
                       string.Equals(x, slot.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        private sealed record CategorySemanticEntry(
            string Group,
            List<string> Traits,
            List<string> Slots);
    }

}
