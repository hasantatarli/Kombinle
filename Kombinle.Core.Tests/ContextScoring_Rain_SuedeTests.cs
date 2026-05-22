using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Domain.Traits;
using Kombinle.Core.Engine;
using Kombinle.Core.Generation;

namespace Kombinle.Core.Tests
{
    public class ContextScoring_Rain_SuedeTests
    {
        //[Fact]
        //public void Rain_WithSuedeShoes_ShouldApplyStrongPenaltyAndWarning()
        //{
        //    // Arrange
        //    var wardrobe = new List<Garment>
        //    {
        //        new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Casual },
        //        new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey,  Formality = Formality.Casual },
        //        new Garment
        //        {
        //            Category = Category.Shoes,
        //            ColorFamily = ColorFamily.Black,
        //            Formality = Formality.Casual,
        //            Shoe = new ShoeTraits
        //            {
        //                Material = new TagValue<ShoeMaterial>(ShoeMaterial.Suede, TagSource.User, 1.0)
        //            }
        //        }
        //    };

        //    var occasion = Occasion.CasualWeekend();

        //    var context = new ContextInput(
        //        Weather: Weather.Rain,
        //        Setting: Setting.Outdoor,
        //        Time: TimeOfDay.Day
        //    );

        //    var generator = new CombinationGenerator();
        //    var combos = generator.Generate(wardrobe, occasion, maxResults: 3);

        //    var scorer = new CombinationScorer(new ScoringConfig());

        //    // Act
        //    var scored = combos
        //        .Select(c => scorer.Score(c, occasion, context: context, user: null))
        //        .ToList();

        //    var best = scored.OrderByDescending(s => s.Score + s.TieBreakScore).First();

        //    // Assert
        //    Assert.True(best.ContextDelta <= -10);
        //    Assert.Contains("RAIN_SUEDE_SHOES", best.ContextWarningCodes);
        //    Assert.Contains(best.ContextReasons, r => r.Contains("Suede"));
        //}
    }
}
