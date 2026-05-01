using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Generation
{
    public class AnchorCandidate
    {
        public Garment Garment { get; set; } = null!;
        public int Priority { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
