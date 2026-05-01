using System.Collections.Generic;
using System.Linq;
using Kombinle.Core.Domain;
using Kombinle.Core.Scoring;
using Kombinle.Core.Scoring.WardrobeFeedbackRules;
using Xunit;

namespace Kombinle.Core.Tests.WardrobeFeedbackRules
{
    public class WardrobeFeedbackEngine_MissingItemTests
    {
        [Fact]
        public void When_PoolHasSameContextWarningInMostCombinations_ShouldReturn_MissingItemForContext()
        {
            // Arrange
            var ranked = new List<ScoredCombination>();

            // Pool size = 10, 9 of them have "Rain"
            for (int i = 0; i < 9; i++)
            {
                var sc = CreateScored();
                sc.ContextWarningCodes.Add("Rain");
                ranked.Add(sc);
            }

            ranked.Add(CreateScored()); // 10th has no warnings

            // Act
            var feedback = WardrobeFeedbackEngine.Evaluate(ranked);

            // Assert
            Assert.NotNull(feedback);
            Assert.Single(feedback);

            var f = feedback.Single();
            Assert.Equal(WardrobeGapType.MissingItemForContext, f.Type);
            Assert.Equal("Rain", f.ContextWarningCode);
            Assert.False(string.IsNullOrWhiteSpace(f.Message));
        }

        private static ScoredCombination CreateScored()
        {
            // OPTION A: If ScoredCombination has a parameterless constructor:
            // return new ScoredCombination();

            // OPTION B: If it requires Candidate or other args, adapt this line:
            // return new ScoredCombination(candidate: SomeFactory.MinCandidate());

            // ---- Minimal best-effort fallback ----
            // Try parameterless first (compile-time). If it doesn't compile, use OPTION B.
            return new ScoredCombination();
        }
    }
}
