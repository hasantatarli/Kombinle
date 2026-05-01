using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Engine;
using Kombinle.Core.Generation;

namespace Kombinle.Core.Tests
{
    public class ContextScoring_RainTests
    {
        [Fact]
        public void Rain_WithShoesTraitsMissing_AppliesSoftPenalty_NoWarning()
        {
            // Arrange
            var wardrobe = new List<Garment>
            {
                new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Casual },
                new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey,  Formality = Formality.Casual },
                new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Brown, Formality = Formality.Casual },
                new Garment { Category = Category.Coat,  ColorFamily = ColorFamily.Grey,  Formality = Formality.Casual }
            };

            var occasion = Occasion.CasualWeekend();

            var context = new ContextInput(
                Weather: Weather.Rain,
                Setting: Setting.Outdoor,
                Time: TimeOfDay.Day
            );

            var generator = new CombinationGenerator();
            var combos = generator.Generate(wardrobe, occasion, maxResults: 5);

            var scorer = new CombinationScorer(new ScoringConfig());

            // Act
            var scored = combos
                .Select(c => scorer.Score(c, occasion, context: context, user: null))
                .ToList();

            var best = scored.OrderByDescending(s => s.Score + s.TieBreakScore).First();

            // Assert
            Assert.Equal(-2, best.ContextDelta);
            Assert.Single(best.ContextReasons);
            Assert.Contains("Shoes traits missing", best.ContextReasons[0]);

            Assert.Empty(best.ContextWarningCodes);
        }
    }
}
