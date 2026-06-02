using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring
{
    public class CombinationRanker
    {
        public List<ScoredCombination> Rank(List<ScoredCombination> scored)
        {
            // MVP ranking kuralı:
            // 1) HardFail az olan (0 olanlar üstte)
            // 2) Score yüksek olan
            // 3) ContextDelta Yüksel olan
            // 4) TieBreak yüksek olan
            // 5) Warning az olan (ikincil kalite sinyali)
            // 6) Signature stabil determinism
            return scored
             .OrderBy(s => s.HardFailCount)
             .ThenByDescending(s => s.Score)
             .ThenByDescending(s => s.ContextDelta)
             .ThenByDescending(s => s.TieBreakScore)
             .ThenBy(s => s.WarningCount)
             .ThenBy(s => s.Candidate.Signature)
             .ToList();
        }

        public (ScoredCombination? Best, List<ScoredCombination> Alternatives) PickTop(
            List<ScoredCombination> ranked, int alternativeCount = 2)
        {
            if (ranked.Count == 0) return (null, new List<ScoredCombination>());

            var best = ranked[0];
            var alternatives = ranked.Skip(1).Take(alternativeCount).ToList();
            return (best, alternatives);
        }
    }
}
