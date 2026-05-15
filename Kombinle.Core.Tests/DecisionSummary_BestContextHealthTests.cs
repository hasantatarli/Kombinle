using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Domain.Traits;
using Kombinle.Core.Engine;
using Kombinle.Core.Generation;
using Kombinle.Core.Scoring;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kombinle.Core.Tests
{
    //public class DecisionSummary_BestContextHealthTests
    //{
    //    [Fact]
    //    public void BestContextHealth_Poor_ShouldSuggestReviewAlternatives()
    //    {
    //        // -------- Arrange --------
    //        var wardrobe = new List<Garment>
    //        {
    //            new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Black, Formality = Formality.Formal },
    //            new Garment { Category = Category.Shirt,  ColorFamily = ColorFamily.Navy,  Formality = Formality.Formal },
    //            new Garment { Category = Category.Pants,  ColorFamily = ColorFamily.Grey,  Formality = Formality.Formal },
    //            new Garment { Category = Category.Shoes,  ColorFamily = ColorFamily.Black, Formality = Formality.Formal },
    //        };

    //        var occasion = Occasion.BusinessMeeting_Formal();

    //        var context = new ContextInput(
    //            Weather: Weather.Rain,
    //            Setting: Setting.Outdoor,
    //            Time: TimeOfDay.Day
    //        );

    //        // Generator
    //        var generator = new CombinationGenerator();
    //        var combos = generator.Generate(wardrobe, occasion, maxResults: 5);

    //        // Testi deterministik yapmak için: ilk kombine suede shoe ver
    //        var first = combos.First();
    //        first.SlotToItem[Slot.Shoes].Shoe = new ShoeTraits
    //        {
    //            Material = new TagValue<ShoeMaterial>(ShoeMaterial.Suede, TagSource.User, 1.0)
    //        };

    //        // -------- Act --------
    //        var scorer = new CombinationScorer(new ScoringConfig());
    //        var scored = combos
    //            .Select(c => scorer.Score(c, occasion, context: context, user: null))
    //            .ToList();

    //        var ranked = new CombinationRanker().Rank(scored);

    //        var summary = DecisionSummaryBuilder.Build(
    //            scenarioTitle: "TEST",
    //            occasion: occasion,
    //            generated: combos,
    //            ranked: ranked,
    //            effectiveContext: null,
    //            alternativeCount: 2,
    //            alternativeMaxScoreGap: 20,
    //            alternativeMaxScoreGap_DiverseAnchor: 35
    //        );

    //        // -------- Assert --------
    //        Assert.Equal(ContextHealthLevel.Poor, summary.BestContextHealth);
    //        Assert.True(summary.SuggestReviewAlternatives);
    //        Assert.NotNull(summary.SuggestReviewAlternativesReason);
    //    }
    //}
}
