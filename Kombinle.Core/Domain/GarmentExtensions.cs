using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
    public static class GarmentExtensions
    {
        public static List<Garment> GetAlternatives(this Garment g, List<Garment> wardrobe)
        {
            // aynı kategori, farklı renk
            return wardrobe
                .Where(x => x.Category == g.Category && x.ColorFamily != g.ColorFamily)
                .ToList();
        }
    }

}
