using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
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
