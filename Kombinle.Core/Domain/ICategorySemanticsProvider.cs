using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
    public interface ICategorySemanticsProvider
    {
        bool HasTrait(Category category, string trait);

        string? GetGroup(Category category);

        bool HasSlot(Category category, Slot slot);
    }
}
