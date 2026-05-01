using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring
{
    public class ScoredCombination
    {
        public CombinationCandidate Candidate { get; set; } = null!;

        // Ana skor
        public int Score { get; internal set; }
        public List<ScoreItem> Breakdown { get; } = new();

        // Tie-break skor
        public int TieBreakScore { get; internal set; }
        public List<ScoreItem> TieBreakdown { get; } = new();

        // Risk semantiği: artık net ayrım var
        public List<string> HardFailReasons { get; } = new();
        public List<string> WarningReasons { get; } = new();

        // Backward-compatible: eski kodlarda FailReasons.Count çalışmaya devam etsin diye
        // Bu listeye prefix ile ekleme yapıyoruz (HARD:/WARN:)
        public List<string> FailReasons { get; } = new();

        public int HardFailCount => HardFailReasons.Count;
        public int WarningCount => WarningReasons.Count;

        public int ContextDelta { get; internal set; }
        public List<string> ContextReasons { get; } = new();
        public List<string> ContextUserReasons { get; set; } = new();

        public List<string> ContextWarningCodes { get; } = new();
        public List<ContextUserNote> ContextUserNotes { get; set; } = new();

        public List<string> AlternativeReasons { get; } = new();
        public List<string> AlternativeReasonCodes { get; set; } = new();


        public void Add(int value, string reason)
        {
            Score += value;
            Breakdown.Add(new ScoreItem { Value = value, Reason = reason });
        }

        public void AddTieBreak(int value, string reason)
        {
            TieBreakScore += value;
            TieBreakdown.Add(new ScoreItem { Value = value, Reason = reason });
        }

        public void AddHardFail(string reason)
        {
            HardFailReasons.Add(reason);
            FailReasons.Add($"HARD: {reason}");
        }

        public void AddWarning(string reason)
        {
            WarningReasons.Add(reason);
            FailReasons.Add($"WARN: {reason}");
        }
    }

}
