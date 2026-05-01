using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring.DTO
{
    public sealed class OutfitItemDto
    {
        public string Slot { get; }
        public string Category { get; }
        public string ColorFamily { get; }

        public OutfitItemDto(string slot, string category, string colorFamily)
        {
            Slot = slot;
            Category = category;
            ColorFamily = colorFamily;
        }
    }
}
