using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Config
{
    public static class AnchorCompositionConfig
    {
        public static List<AnchorCompositionRule> Rules = new()
    {
        new AnchorCompositionRule
        {
            Anchor = Category.Jacket,
            RequiredCategories = new List<Category>
            {
                Category.Pants,
                Category.Shoes
            }
        },
        new AnchorCompositionRule
        {
            Anchor = Category.Jacket,
            RequiredCategories = new List<Category>
            {
                Category.Pants
            }
        },
        new AnchorCompositionRule
        {
            Anchor = Category.Dress,
            RequiredCategories = new List<Category>()
        }
    };
    }

}
