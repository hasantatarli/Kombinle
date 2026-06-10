using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
    public interface ICategorySemanticsProvider
    {
        bool HasTrait(string category, string trait);

        string? GetGroup(string category);

        bool HasSlot(string category, Slot slot);
    }
}
