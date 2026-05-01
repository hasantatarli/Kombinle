using Kombinle.Core.Domain;
using Kombinle.Core.Generation;
using Kombinle.Core.Scoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Tests
{
    public class DecisionSummary_FallbackOutfitTests
    {
        [Fact]
        public void When_NoCombinationGenerated_ShouldReturnFallbackOutfit_WithItems()
        {
            var generated = new List<CombinationCandidate>
    {
        new CombinationCandidate
        {
            Anchor = new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Navy },
            SlotToItem = new Dictionary<Slot, Garment>
            {
                [Slot.Anchor] = new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Navy, Formality = Formality.Formal },
                [Slot.Shoes]  = new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Formal }
            }
        }
    };

            var summary = DecisionSummaryBuilder.Build(
                scenarioTitle: "Test",
                occasion: Occasion.BusinessMeeting_Formal(),
                generated: generated,
                ranked: new List<ScoredCombination>(),
                effectiveContext: null
            );

            Assert.NotNull(summary.FallbackOutfit);
            Assert.True(summary.FallbackOutfit.Items.Count >= 1);

        }

    }
}
