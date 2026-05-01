using Kombinle.Core.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Traits
{
    public enum ShoeMaterial { Leather, Suede, Canvas, Synthetic }
    public enum WaterResistance { Low, Medium, High }

    public class ShoeTraits
    {
        public TagValue<ShoeMaterial>? Material { get; set; }
        public TagValue<WaterResistance>? WaterResistance { get; set; }
    }
}
