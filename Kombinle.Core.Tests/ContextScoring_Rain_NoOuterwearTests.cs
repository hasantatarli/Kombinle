using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Engine;
using Kombinle.Core.Generation;
using Kombinle.Core.Scoring;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kombinle.Core.Tests
{
    public class ContextScoring_Rain_NoOuterwearTests
    {
        [Fact]
        public void Rain_WithNoOuterwear_ShouldApplyPenaltyAndWarning()
        {
            // Arrange: wardrobe'da Outerwear yok
            var wardrobe = new List<Garment>
            {
                new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Casual },
                new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey,  Formality = Formality.Casual },
                new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Casual }
            };

            var occasion = Occasion.CasualWeekend();

            var context = new ContextInput(
                Weather: Weather.Rain,
                Setting: Setting.Outdoor,
                Time: TimeOfDay.Day
            );

            var generator = new CombinationGenerator();
            var combos = generator.Generate(wardrobe, occasion, maxResults: 3);

            var scorer = new CombinationScorer(new ScoringConfig());

            // Act
            var scored = combos
                .Select(c => scorer.Score(c, occasion, context: context, user: null))
                .ToList();

            var best = scored.OrderByDescending(s => s.Score + s.TieBreakScore).First();

            // Assert
            Assert.Contains(best.ContextWarningCodes, w => w.EndsWith("_NO_OUTERWEAR"));
            Assert.True(best.ContextDelta <= -5);
            Assert.Contains(best.ContextReasons, r => r.Contains("No outerwear"));
        }
    }
}
