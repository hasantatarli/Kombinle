using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Engine;
using Kombinle.Core.Generation;
using Kombinle.Core.Scoring;

namespace Kombinle.Core.Tests
{
    internal static class TestHarness
    {
        public static ScoredCombination BestScored(
            List<Garment> wardrobe,
            Occasion occasion,
            ContextInput context,
            int maxResults = 5)
        {
            var generator = new CombinationGenerator();
            var combos = generator.Generate(wardrobe, occasion, maxResults: maxResults);

            var scorer = new CombinationScorer(new ScoringConfig());
            var scored = combos
                .Select(c => scorer.Score(c, occasion, context: context, user: null))
                .ToList();

            return scored.OrderByDescending(s => s.Score + s.TieBreakScore).First();
        }
    }
}
