using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Semantics
{
    public static class SemanticRoleResolver
    {
        public static string? GetRole(Category category)
        {
            return category switch
            {
                Category.Hoodie => SemanticLayerRoles.Comfort,
                Category.Cardigan => SemanticLayerRoles.Comfort,

                Category.Jacket => SemanticLayerRoles.Structure,

                Category.LightOuterwear => SemanticLayerRoles.Protection,
                Category.Coat => SemanticLayerRoles.Protection,

                _ => null
            };
        }
    }
}
