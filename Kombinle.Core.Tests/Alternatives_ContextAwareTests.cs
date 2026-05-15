using Kombinle.Core.Domain;
using Kombinle.Core.Generation;
using Kombinle.Core.Scoring;
using Kombinle.Core.Scoring.Alternatives;
using System.Collections.Generic;
using Xunit;

namespace Kombinle.Core.Tests
{
    public class Alternatives_ContextAwareTests
    {
        //[Fact]
        //public void When_BestContextIsPoor_ShouldPreferSaferAlternative()
        //{
        //    var occasion = Occasion.BusinessMeeting_Formal();

        //    var topBlue = new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.Blue, Formality = Formality.Formal };
        //    var topWhite = new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Formal };

        //    var bottomGrey = new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey, Formality = Formality.Formal };
        //    var bottomBrown = new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Brown, Formality = Formality.Formal };

        //    // Best
        //    var bestAnchor = new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Black, Formality = Formality.Formal };
        //    var bestShoes = new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Formal };

        //    var bestCandidate = new CombinationCandidate
        //    {
        //        Signature = "BEST",
        //        Anchor = bestAnchor,
        //        SlotToItem = BuildSlots(bestAnchor, topBlue, bottomGrey, bestShoes)
        //    };

        //    var best = new ScoredCombination
        //    {
        //        Candidate = bestCandidate,
        //        Score = 67,
        //        TieBreakScore = 0,
        //        ContextDelta = -13
        //    };
        //    best.ContextWarningCodes.Add("RAIN_SUEDE_SHOES");

        //    // Unsafe alt: skor yüksek ama context warning var
        //    var unsafeAnchor = new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Navy, Formality = Formality.Formal };
        //    var unsafeShoes = new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Formal };

        //    var unsafeCandidate = new CombinationCandidate
        //    {
        //        Signature = "UNSAFE",
        //        Anchor = unsafeAnchor,
        //        SlotToItem = BuildSlots(unsafeAnchor, topBlue, bottomGrey, unsafeShoes) // best ile benzer olabilir
        //    };

        //    var unsafeAlt = new ScoredCombination
        //    {
        //        Candidate = unsafeCandidate,
        //        Score = 66,
        //        TieBreakScore = 0,
        //        ContextDelta = -10
        //    };
        //    unsafeAlt.ContextWarningCodes.Add("RAIN_SUEDE_SHOES");

        //    // Safe alt: skor daha düşük ama context daha iyi + anlamlı fark (core slot farkı)
        //    // Core slot farkını garanti etmek için Top'u değiştiriyoruz (Blue -> White) veya Bottom'u değiştiriyoruz.
        //    var safeAnchor = new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Brown, Formality = Formality.Formal };
        //    var safeShoes = new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Formal };

        //    var safeCandidate = new CombinationCandidate
        //    {
        //        Signature = "SAFE",
        //        Anchor = safeAnchor,
        //        SlotToItem = BuildSlots(safeAnchor, topWhite, bottomGrey, safeShoes) // Top farklı => meaningful difference
        //    };

        //    var safeAlt = new ScoredCombination
        //    {
        //        Candidate = safeCandidate,
        //        Score = 60,
        //        TieBreakScore = 0,
        //        ContextDelta = 0
        //    };
        //    // warning yok

        //    var ranked = new List<ScoredCombination> { best, unsafeAlt, safeAlt };

        //    var picked = AlternativePicker.Pick_ProductQuality(
        //        occasion,
        //        ranked,
        //        best,
        //        alternativeCount: 1,
        //        alternativeMaxScoreGap: 20,
        //        alternativeMaxScoreGap_DiverseAnchor: 35
        //    );

        //    Assert.Single(picked);
        //    Assert.Equal("SAFE", picked[0].Candidate.Signature);
        //    Assert.Contains("ALT_CONTEXT_SAFER",picked[0].AlternativeReasonCodes);
        //    //Assert.NotEmpty(picked[0].AlternativeReasonCodes);


        //}

        //private static Dictionary<Slot, Garment> BuildSlots(
        //    Garment anchor, Garment top, Garment bottom, Garment shoes)
        //{
        //    return new Dictionary<Slot, Garment>
        //    {
        //        [Slot.Anchor] = anchor,
        //        [Slot.Top] = top,
        //        [Slot.Bottom] = bottom,
        //        [Slot.Shoes] = shoes
        //    };
        //}

        //}
    }
}
