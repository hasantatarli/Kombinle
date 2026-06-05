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
            return GetConflictSeverity(roleA, roleB, season) != SemanticConflictSeverity.Hard;
        }

        public static SemanticConflictSeverity GetConflictSeverity(
            string? roleA,
            string? roleB,
            Season? season)
        {
            if (roleA == null || roleB == null)
                return SemanticConflictSeverity.None;

            var isStructureProtection =
                (roleA == SemanticLayerRoles.Structure &&
                 roleB == SemanticLayerRoles.Protection)
                ||
                (roleA == SemanticLayerRoles.Protection &&
                 roleB == SemanticLayerRoles.Structure);

            var isComfortProtection =
                (roleA == SemanticLayerRoles.Comfort &&
                 roleB == SemanticLayerRoles.Protection)
                ||
                (roleA == SemanticLayerRoles.Protection &&
                 roleB == SemanticLayerRoles.Comfort);

            if (season == Season.Summer && isStructureProtection)
                return SemanticConflictSeverity.Hard;

            return SemanticConflictSeverity.None;
        }
    }
}
