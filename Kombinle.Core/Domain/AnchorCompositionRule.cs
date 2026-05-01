using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
    public class AnchorCompositionRule
    {
        public Category Anchor { get; set; }
        public List<Category> RequiredCategories { get; set; } = new();
    }

}
