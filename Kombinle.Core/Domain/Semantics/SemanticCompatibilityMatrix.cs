using Kombinle.Core.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Semantics
{
    public static class SemanticCompatibilityMatrix
    {
        public static bool IsCompatible(
            string? roleA,
            string? roleB,
            Season? season)
        {
            if (roleA == null || roleB == null)
                return true;

            if (season == Season.Summer)
            {
                var isStructureProtection =
                    (roleA == SemanticLayerRoles.Structure &&
                     roleB == SemanticLayerRoles.Protection)
                    ||
                    (roleA == SemanticLayerRoles.Protection &&
                     roleB == SemanticLayerRoles.Structure);

                if (isStructureProtection)
                    return false;
            }

            return true;
        }
    }
}
