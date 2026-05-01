using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Config
{
    public class ScoringConfig
    {
        //public int ColorMatch_AnchorPair = 10;   // Anchor ile uyum
        //public int ColorMatch_OtherPair = 5;     // Supporter-supporter uyum
        //public int ColorClash_AnchorPair = -30;  // Anchor ile çakışma daha ağır
        //public int ColorClash_OtherPair = -15;

        //public int FormalityMatch = 10;
        //public int FormalityMismatch = -10;

        //public int FavoriteColorBonus = 5;

        //// Eğer skor çok düşerse “uyarı” sayabiliriz (hard fail değil)
        //public int WarningThreshold = 50;

        //// Tie-break (eşitlik bozucu) - küçük olmalı
        //public int NeutralBonusPerItem = 1;
        //public int NeutralBonusCap = 2;          // max +2
        //public int OptionalOuterwearBonus = 1;


        //// H2.3b: Alternatif kalitesi guardrail'i
        //public int AlternativeMaxScoreGap = 20;

        //// H2.3c: Eğer alternatif dolmazsa, farklı anchor için biraz daha toleranslı ol
        //public int AlternativeMaxScoreGap_DiverseAnchor = 35;

        public int ColorMatch_AnchorPair { get; set; } = 10;
        public int ColorMatch_OtherPair { get; set; } = 5;
        public int ColorClash_AnchorPair { get; set; } = -30;
        public int ColorClash_OtherPair { get; set; } = -15;

        public int FormalityMatch { get; set; } = 10;
        public int FormalityMismatch { get; set; } = -10;

        public int FavoriteColorBonus { get; set; } = 5;

        public int WarningThreshold { get; set; } = 50;

        public int NeutralBonusPerItem { get; set; } = 1;
        public int NeutralBonusCap { get; set; } = 2;
        public int OptionalOuterwearBonus { get; set; } = 1;

        public int AlternativeMaxScoreGap { get; set; } = 20;
        public int AlternativeMaxScoreGap_DiverseAnchor { get; set; } = 35;

    }

}
