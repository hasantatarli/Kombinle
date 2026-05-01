using Kombinle.Core.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Traits
{
    public enum WeatherProtection { None, Light, Rain }

    public class OuterwearTraits
    {
        public TagValue<WeatherProtection>? Protection { get; set; }
    }
}
