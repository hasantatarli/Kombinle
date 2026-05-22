using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;

namespace Kombinle.Core.Scoring
{
    public enum DecisionRiskLevel
    {
        Safe = 0,
        Warning = 1,
        HardFail = 2
    }

    public class DecisionSummary
    {
        public string ScenarioTitle { get; set; } = string.Empty;

        public int GeneratedCount { get; set; }
        public int RankedCount { get; set; }

        public int HardFailedCount { get; set; }
        public int WarningCount { get; set; }

        // Best'e göre risk (önceden OverallRisk dediğimiz şey)
        public DecisionRiskLevel BestRisk { get; set; } = DecisionRiskLevel.Safe;

        // Best + alternatives (MVP contract)
        public ScoredCombination? Best { get; set; }
        public List<ScoredCombination> Alternatives { get; set; } = new();

        // “tek bakış” için kısa açıklamalar
        public string BestShort { get; set; } = string.Empty;
        public List<string> AlternativeShort { get; set; } = new();

        // Seçim nedeni (rank/tiebreak sinyalleri)
        public List<string> BestWhy { get; set; } = new();

        // Risk notları (warning/hardfail)
        public List<string> BestRiskNotes { get; set; } = new();
        // Pool health (run-level)
        public double HardFailRate { get; set; }      // 0..1
        public double WarningRate { get; set; }       // 0..1
        public string PoolHealth { get; set; } = "Unknown"; // Good / Okay / Poor

        public List<string> BestContextWhy { get; set; } = new();
        public ContextInput? EffectiveContext { get; set; }

        // Context health (Phase 2.5)
        public double ContextAvgDelta { get; set; }          // ortalama ContextDelta
        public double ContextPenaltyRate { get; set; }       // ContextDelta < 0 olanların oranı
        public double ContextWarningRate { get; set; }       // ContextWarningCodes != empty olanların oranı

        public ContextHealthLevel BestContextHealth { get; set; }


        public ContextHealthLevel ContextHealth { get; set; }

        public bool SuggestReviewAlternatives { get; set; }
        public string? SuggestReviewAlternativesReason { get; set; }

        public List<string> AlternativeWhy { get; set; } = new();

        public Dictionary<string, List<string>> AlternativeWhyBySignature { get; set; } = new();

        public List<WardrobeFeedback> WardrobeFeedback { get; } = new();

        // NEW (Phase B)
        public FallbackOutfit? FallbackOutfit { get; set; }

        public List<WardrobeGap> WardrobeGaps { get; set; } = new();

        public Formality OccasionRequiredFormality { get; set; }

        public List<ScoredCombination> BestPool { get; set; } = new();

        public List<ContextUserNote> BestContextNotes { get; set; } = new();

        public ScoredCombination? RawBest { get; set; }
        public int RotationAttempt { get; set; }

    }
}
