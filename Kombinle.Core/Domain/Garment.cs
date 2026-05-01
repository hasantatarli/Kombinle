using Kombinle.Core.Domain.Traits;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
    public class Garment
    {
        public Category Category { get; set; }
        public ColorFamily ColorFamily { get; set; }
        public Formality Formality { get; set; }

        public ShoeTraits? Shoe { get; set; }               // sadece Shoes kategorisinde dolu olur
        public OuterwearTraits? Outerwear { get; set; }     // sadece Outerwear kategorisinde dolu olur
    }

}
