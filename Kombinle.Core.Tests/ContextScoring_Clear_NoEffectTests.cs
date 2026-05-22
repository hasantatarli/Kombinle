using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Engine;
using Kombinle.Core.Generation;

namespace Kombinle.Core.Tests
{
    public class ContextScoring_Clear_NoEffectTests
    {
        //[Fact]
        //public void ClearWeather_ShouldNotChangeScoreOrProduceContextSignals()
        //{
        //    // Arrange
        //    var wardrobe = new List<Garment>
        //    {
        //        new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Casual },
        //        new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey,  Formality = Formality.Casual },
        //        new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Casual },
        //        new Garment { Category = Category.Coat,  ColorFamily = ColorFamily.Grey,  Formality = Formality.Casual }
        //    };

        //    var occasion = Occasion.CasualWeekend();

        //    var context = new ContextInput(
        //        Weather: Weather.Clear,
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

        //    var best = TestHarness.BestScored(wardrobe, occasion, context);


        //    // Assert
        //    Assert.Equal(0, best.ContextDelta);
        //    Assert.Empty(best.ContextReasons);
        //    Assert.Empty(best.ContextWarningCodes);
        //}
    }
}
