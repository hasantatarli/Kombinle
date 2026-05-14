using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring.BestPool
{
    internal static class BestPoolBuilder
    {
        //private const int MaxScoreGapFromBest = 10;
        private const int MaxPoolSize = 5;
        private const double BestPoolThresholdRatio = 0.75;

        internal static List<ScoredCombination> Build(
            IReadOnlyList<ScoredCombination> ranked,
            ScoredCombination best,
            Func<ScoredCombination, bool> hasMeaningfulDifference)
        {
            var result = new List<ScoredCombination>();

            foreach (var candidate in ranked)
            {
                if (result.Count >= MaxPoolSize)
                    break;

                if (!IsEligible(best, candidate))
                    continue;

                if (!ReferenceEquals(candidate, best) && !hasMeaningfulDifference(candidate))
                    continue;

                result.Add(candidate);
            }

            return result;
        }

        private static bool IsEligible(
            ScoredCombination best,
            ScoredCombination candidate)
        {
            if (candidate.HardFailCount > 0)
                return false;

            var minScore = best.Score * BestPoolThresholdRatio;

            if (candidate.Score < minScore)
                return false;

            if (candidate.ContextWarningCodes.Count > 0)
                return false;

            if (candidate.ContextDelta < best.ContextDelta)
                return false;

            return true;
        }
    }
}
