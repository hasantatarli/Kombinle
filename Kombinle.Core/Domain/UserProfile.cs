using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
    public class UserProfile
    {
        public List<ColorFamily> FavoriteColors { get; set; } = new();
    }

}
