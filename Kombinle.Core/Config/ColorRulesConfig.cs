using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Config
{
    public sealed class ColorRulesConfig
    {
        public List<ColorPairRule> StrongPairs { get; set; } = new();
        public List<ColorPairRule> WeakPairs { get; set; } = new();
        public List<ColorPairRule> ClashPairs { get; set; } = new();

        public HashSet<ColorFamily> NeutralColors { get; set; } = new();
        public HashSet<ColorFamily> BrightColors { get; set; } = new();
    }

    public sealed class ColorPairRule
    {
        public ColorFamily A { get; set; }
        public ColorFamily B { get; set; }
    }
}
