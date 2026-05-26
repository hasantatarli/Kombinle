using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
    // Default fallback provider.
    // Used only when the API does not initialize JsonCategorySemanticsProvider.
    // Active API flow should use category_catalog.json as the source of truth.
    public sealed class DefaultCategorySemanticsProvider : ICategorySemanticsProvider
    {
        public bool HasTrait(Category category, string trait)
        {
            return CategorySemantics.HasTrait(category, trait);
        }

        public string? GetGroup(Category category)
        {
            return CategorySemantics.GetGroup(category);
        }

        public bool HasSlot(Category category, Slot slot)
        {
            return CategorySemantics.HasSlot(category, slot);
        }
    }
}
