using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Engine;
using Kombinle.Core.Generation;
using Kombinle.Core.Scoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Tests
{
    public class DecisionSummary_SoftAnchorFeedbackTests
    {
        [Fact]
        public void When_AnchorIsSoftAndMissing_ShouldEmitWardrobeFeedback()
        {
            // Arrange
            var wardrobe = new List<Garment>
    {
        new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Formal },
        new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey, Formality = Formality.Formal },
        new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Formal }
    };

            var occasion = Occasion.BusinessMeeting_Formal();
            var gen = new CombinationGenerator();
            var scorer = new CombinationScorer(new ScoringConfig());
            var ranker = new CombinationRanker();

            var combos = gen.Generate(wardrobe, occasion);
            var scored = combos.Select(c => scorer.Score(c, occasion, occasion.DefaultContext, null)).ToList();
            var ranked = ranker.Rank(scored);

            // Act
            var summary = DecisionSummaryBuilder.Build(
                scenarioTitle: "Test",
                occasion: occasion,
                generated: combos,
                ranked: ranked,
                effectiveContext: occasion.DefaultContext
            );

            // Assert
            //Assert.Equal(MessageKind.ContextWarning, map["RAIN_SUEDE_SHOES"].Kind);

            Assert.Contains(
                summary.WardrobeFeedback,
                f => f.Type == WardrobeGapType.MissingSoftAnchor
            );
        }

    }
}
