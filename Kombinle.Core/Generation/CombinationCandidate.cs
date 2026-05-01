using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Generation
{
    public class CombinationCandidate
    {
        public Combination Combination { get; set; } = new();
        public Garment? Anchor { get; set; } = null!;

        public Dictionary<Slot, Garment> SlotToItem { get; set; } = new();

        public string Strategy { get; set; } = string.Empty; // Primary / Variant:Top vs
        public List<string> Reasons { get; set; } = new();

        public string Signature { get; set; } = string.Empty;

    }
}
