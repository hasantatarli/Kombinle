using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Semantics
{
    public static class SemanticRoleResolver
    {
        public static string? GetRole(string categoryId)
        {
            if (CategorySemantics.IsComfortLayer(categoryId))
                return SemanticLayerRoles.Comfort;

            if (CategorySemantics.IsStructuredLayer(categoryId))
                return SemanticLayerRoles.Structure;

            if (CategorySemantics.IsProtectionLayer(categoryId))
                return SemanticLayerRoles.Protection;

            return null;
        }
    }
}
