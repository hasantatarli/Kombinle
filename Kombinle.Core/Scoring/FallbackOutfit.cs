using Kombinle.Core.Scoring.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring
{
    public sealed class FallbackOutfit
    {
        public List<OutfitItemDto> Items { get; } = new();
        public string HeadlineTr { get; set; } = "";
        public string SubtextTr { get; set; } = "";
    }

}
