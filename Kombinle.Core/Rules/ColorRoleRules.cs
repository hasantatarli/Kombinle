using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Rules
{
    public static class ColorRoleRules
    {
        public static bool IsNeutral(ColorFamily color)
        {
            return color == ColorFamily.White
                   || color == ColorFamily.Black
                   || color == ColorFamily.Grey
                   || color == ColorFamily.Navy;
        }
    }
}
