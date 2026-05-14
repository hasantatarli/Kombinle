using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Generation;
using Kombinle.Core.Scoring.Context;
using Kombinle.Core.Scoring.DTO;
using Kombinle.Core.Scoring.WardrobeFeedbackRules;
using Kombinle.Core.Scoring.BestPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring
{
    public static class DecisionSummaryBuilder
    {


        public static DecisionSummary Build(
            string scenarioTitle,
            Occasion occasion,
            List<CombinationCandidate> generated,
            List<ScoredCombination> ranked,
            ContextInput? effectiveContext,
            int rotationAttempt = 0,
            int alternativeCount = 2,
            int alternativeMaxScoreGap = 20, // H2.3b guardrail
            int alternativeMaxScoreGap_DiverseAnchor = 35
        )
        {


            var summary = new DecisionSummary
            {
                ScenarioTitle = scenarioTitle,
                GeneratedCount = generated?.Count ?? 0,
                RankedCount = ranked?.Count ?? 0,
                EffectiveContext = effectiveContext
            };

            //if (ranked == null || ranked.Count == 0)
            //{
            //    summary.BestRisk = DecisionRiskLevel.HardFail;
            //    summary.BestShort = "No viable combinations.";
            //    return summary;
            //}

            // B-PLAN: Hiç kombin üretilemediyse fallback üret
            //if (ranked == null || ranked.Count == 0)
            //{
            //    summary.BestRisk = DecisionRiskLevel.HardFail;

            //    summary.FallbackOutfit = BuildFallbackOutfit(generated, occasion);

            //    summary.WardrobeFeedback.Add(
            //        new WardrobeFeedback(
            //            WardrobeGapType.IncompleteOutfit,
            //            "INCOMPLETE_OUTFIT",
            //            "Tam kombin için dolabına 1-2 parça daha ekleyebilirsin."
            //        )
            //    );

            //    summary.BestShort = "No viable combinations.";
            //    return summary;
            //}

            ranked ??= new List<ScoredCombination>();
            generated ??= new List<CombinationCandidate>();

            var best = GetBestViable(ranked);

            if (best == null)
            {
                return BuildNoViableSummary(summary, generated, occasion);
            }

            var coreSlots = GetCoreSlots(occasion);

            summary.BestPool = BestPoolBuilder.Build(
                ranked,
                best,
                candidate => HasMeaningfulDifference(
                    best.Candidate,
                    candidate.Candidate,
                    coreSlots));

            if (summary.BestPool.Count > 0)
            {
                var selectedIndex = rotationAttempt % summary.BestPool.Count;
                best = summary.BestPool[selectedIndex];
            }

            summary.Best = best;
            summary.BestRisk = RiskOf(best);

            


            //Console.WriteLine("=== BEST POOL ===");

            //foreach (var item in summary.BestPool)
            //{
            //    Console.WriteLine(
            //        $"Score={item.Score} | ContextDelta={item.ContextDelta} | Signature={item.Candidate.Signature}");
            //}

            // --- Wardrobe Feedback: Soft Anchor (Jacket) ---
            var anchorReq = occasion.SlotSet.Get(Slot.Anchor);
            var bestHasLayerOrOuterwear =
                best.Candidate.SlotToItem.Values.Any(i =>
                    i.Category == Category.Coat ||
                    i.Category == Category.Jacket ||
                    i.Category == Category.Blazer ||
                    i.Category == Category.Cardigan ||
                    i.Category == Category.Hoodie)
                ||
                best.Candidate.Anchor is not null;

            var suppressSoftAnchorFeedback =
                (effectiveContext?.Season == Season.Summer && effectiveContext?.Setting == Setting.Indoor)
                || bestHasLayerOrOuterwear;



            if (anchorReq?.Level == RequirementLevel.Soft && best.Candidate.Anchor == null && !suppressSoftAnchorFeedback)
            {
                summary.WardrobeFeedback.Add(new WardrobeFeedback(
                        type: WardrobeGapType.MissingSoftAnchor,
                        contextWarningCode: "SOFT_ANCHOR_MISSING",
                        message: "Bu plan için bir ceket kombini güçlendirir."
                ));
            }


            //summary.BestContextHealth = ComputeContextHealth(
            //    avgDelta: best.ContextDelta,
            //    penaltyRate: best.ContextDelta < 0 ? 1.0 : 0.0,
            //    warningRate: best.ContextWarningCodes.Count > 0 ? 1.0 : 0.0
            //);

            // Context health metrics
            summary.ContextAvgDelta = ranked.Average(s => (double)s.ContextDelta);

            summary.ContextPenaltyRate =
                ranked.Count == 0 ? 0 : (double)ranked.Count(s => s.ContextDelta < 0) / ranked.Count;

            summary.ContextWarningRate =
                ranked.Count == 0 ? 0 : (double)ranked.Count(s => s.ContextWarningCodes.Count > 0) / ranked.Count;


            summary.HardFailedCount = ranked.Count(s => s.HardFailCount > 0);
            summary.WarningCount = ranked.Count(s => s.WarningCount > 0);

            // PoolHealth metrics
            summary.HardFailRate = summary.RankedCount == 0 ? 0 : (double)summary.HardFailedCount / summary.RankedCount;
            summary.WarningRate = summary.RankedCount == 0 ? 0 : (double)summary.WarningCount / summary.RankedCount;
            summary.PoolHealth = ComputePoolHealth(summary.HardFailRate, summary.WarningRate);


            summary.BestShort = BuildUserShort(best.Candidate);
            summary.BestRiskNotes = BuildRiskNotes(best);
            summary.BestWhy = BuildWhySelected(best);
            summary.BestContextWhy = best.ContextReasons.Take(3).ToList();

            summary.ContextHealth = ComputeContextHealth(summary.ContextAvgDelta, summary.ContextPenaltyRate, summary.ContextWarningRate);
            summary.BestContextHealth = ComputeBestContextHealth(best);

            summary.SuggestReviewAlternatives = summary.BestContextHealth == ContextHealthLevel.Poor;
            summary.SuggestReviewAlternativesReason =
                summary.SuggestReviewAlternatives
                    ? "Koşullar nedeniyle bu kombin riskli olabilir; alternatiflere göz at."
                    : null;
            summary.OccasionRequiredFormality = occasion.RequiredFormality;


            //summary.AlternativeContextWhy.Add(alt.ContextReasons.Take(2).ToList());
            //summary.EffectiveContext = effectiveContext;

            //Console.WriteLine("=== TOP 10 SCORED RESULTS ===");

            //foreach (var sc in ranked.Take(10))
            //{
            //    Console.WriteLine($"SIGNATURE: {sc.Candidate.Signature}");
            //    Console.WriteLine($"Score={sc.Score} TieBreak={sc.TieBreakScore} ContextDelta={sc.ContextDelta}");
            //    Console.WriteLine($"Warnings={string.Join(",", sc.ContextWarningCodes)}");

            //    foreach (var b in sc.Breakdown)
            //        Console.WriteLine($"  {b.Value} | {b.Reason}");

            //    foreach (var tb in sc.TieBreakdown)
            //        Console.WriteLine($"  TB {tb.Value} | {tb.Reason}");

            //    Console.WriteLine();
            //}

            summary.Alternatives = Kombinle.Core.Scoring.Alternatives.AlternativePicker.Pick_ProductQuality(
                    occasion,
                    ranked,
                    best,
                    alternativeCount,
                    alternativeMaxScoreGap,
                    alternativeMaxScoreGap_DiverseAnchor
                );


            foreach (var alt in summary.Alternatives)
                summary.AlternativeShort.Add(BuildUserShort(alt.Candidate));


            //Console.WriteLine("=== SCORED RESULTS ===");

            //foreach (var sc in ranked)
            //{
            //    Console.WriteLine($"SIGNATURE: {sc.Candidate.Signature}");
            //    Console.WriteLine($"Score={sc.Score} TieBreak={sc.TieBreakScore} ContextDelta={sc.ContextDelta}");

            //    foreach (var b in sc.Breakdown)
            //        Console.WriteLine($"  {b.Value} | {b.Reason}");

            //    foreach (var tb in sc.TieBreakdown)
            //        Console.WriteLine($"  TB {tb.Value} | {tb.Reason}");

            //    Console.WriteLine();
            //}

            //Console.WriteLine("=== COAT CANDIDATES ===");

            //foreach (var sc in ranked.Where(x =>
            //    x.Candidate.SlotToItem.Values.Any(i => i.Category == Category.Coat) ||
            //    x.Candidate.Anchor?.Category == Category.Coat))
            //{
            //    Console.WriteLine($"SIGNATURE: {sc.Candidate.Signature}");
            //    Console.WriteLine($"Score={sc.Score} TieBreak={sc.TieBreakScore} ContextDelta={sc.ContextDelta}");
            //    Console.WriteLine($"Warnings={string.Join(",", sc.ContextWarningCodes)}");

            //    foreach (var b in sc.Breakdown)
            //        Console.WriteLine($"  {b.Value} | {b.Reason}");

            //    Console.WriteLine();
            //}

            // W1 - Wardrobe Feedback (read-only, no mutation)
            var wardrobeFeedback = WardrobeFeedbackEngine.Evaluate(ranked);
            summary.WardrobeFeedback.AddRange(wardrobeFeedback);

            return summary;
        }


        /// <summary>
        /// Core slots: Required slots excluding Shoes/Outerwear/Accessory (MVP).
        /// Always includes Anchor.
        /// </summary>
        internal static List<Slot> GetCoreSlots(Occasion occasion)
        {
            var required = occasion.SlotSet.HardSlots.Select(r => r.Slot).ToList();

            required.Remove(Slot.Shoes);
            required.Remove(Slot.Accessory);
            required.Remove(Slot.Outerwear);

            if (!required.Contains(Slot.Anchor))
                required.Insert(0, Slot.Anchor);

            return required;
        }

        //internal static bool HasMeaningfulDifference(
        //    CombinationCandidate best,
        //    CombinationCandidate other,
        //    List<Slot> coreSlots)
        //{
        //    if (coreSlots == null || coreSlots.Count == 0)
        //        return !string.Equals(best.Signature, other.Signature, StringComparison.Ordinal);

        //    foreach (var slot in coreSlots)
        //    {
        //        var aHas = best.SlotToItem.TryGetValue(slot, out var a);
        //        var bHas = other.SlotToItem.TryGetValue(slot, out var b);

        //        if (aHas != bHas) return true;
        //        if (!aHas || !bHas) continue;

        //        if (!IsSameGarment(a!, b!)) return true;
        //    }

        //    return false;
        //}

        internal static bool HasMeaningfulDifference(
         CombinationCandidate best,
         CombinationCandidate other,
         List<Slot> coreSlots)
        {
            if (coreSlots == null || coreSlots.Count == 0)
                return !string.Equals(best.Signature, other.Signature, StringComparison.Ordinal);

            var differenceCount = 0;

            foreach (var slot in coreSlots)
            {
                if (slot == Slot.Anchor)
                {
                    if (!IsSameGarment(best.Anchor, other.Anchor))
                        differenceCount++;

                    continue;
                }

                var aHas = best.SlotToItem.TryGetValue(slot, out var a);
                var bHas = other.SlotToItem.TryGetValue(slot, out var b);

                if (aHas != bHas)
                {
                    differenceCount++;
                    continue;
                }

                if (!aHas || !bHas)
                    continue;

                if (!IsSameGarment(a!, b!))
                    differenceCount++;
            }

            return differenceCount >= 1;
        }

        private static DecisionRiskLevel RiskOf(ScoredCombination s)
        {
            if (s.HardFailCount > 0) return DecisionRiskLevel.HardFail;
            if (s.WarningCount > 0) return DecisionRiskLevel.Warning;
            return DecisionRiskLevel.Safe;
        }

        private static string BuildDebugShort(CombinationCandidate c, ScoredCombination s)
        {
            var parts = new List<string>
    {
        c.Anchor == null ? "NoAnchor" : $"{c.Anchor.ColorFamily} {c.Anchor.Category}"
    };

            if (c.SlotToItem.TryGetValue(Slot.Top, out var top))
                parts.Add($"{top.ColorFamily} {top.Category}");

            if (c.SlotToItem.TryGetValue(Slot.Bottom, out var bottom))
                parts.Add($"{bottom.ColorFamily} {bottom.Category}");

            if (c.SlotToItem.TryGetValue(Slot.Shoes, out var shoes))
                parts.Add($"{shoes.ColorFamily} {shoes.Category}");
            else
                parts.Add("Shoes:(missing)");

            var outerTag = c.SlotToItem.ContainsKey(Slot.Outerwear) ? " +Outerwear" : "";

            var risk = RiskOf(s);
            var riskTag = risk switch
            {
                DecisionRiskLevel.Safe => "SAFE",
                DecisionRiskLevel.Warning => "WARN",
                _ => "HARDFAIL"
            };

            return $"{string.Join(" + ", parts)}{outerTag} | Score:{s.Score} TB:{s.TieBreakScore} | {riskTag}";
        }

        private static string BuildUserShort(CombinationCandidate c)
        {
            var parts = new List<string>();

            void AddPart(Garment? garment)
            {
                if (garment == null) return;

                var text = $"{garment.ColorFamily} {garment.Category}";
                if (!parts.Contains(text, StringComparer.OrdinalIgnoreCase))
                    parts.Add(text);
            }

            AddPart(c.Anchor);

            if (c.SlotToItem.TryGetValue(Slot.Top, out var top))
                AddPart(top);

            if (c.SlotToItem.TryGetValue(Slot.Bottom, out var bottom))
                AddPart(bottom);

            if (c.SlotToItem.TryGetValue(Slot.Shoes, out var shoes))
                AddPart(shoes);

            var hasSoftOuterwearOnly =
                c.Anchor == null &&
                c.SlotToItem.TryGetValue(Slot.Outerwear, out var outerwear);

            var shortText = parts.Count == 0
                ? "Kombin özeti yok"
                : string.Join(" + ", parts);


            return shortText;
        }

        private static List<string> BuildRiskNotes(ScoredCombination best)
        {
            var notes = new List<string>();
            var warningTitles = new HashSet<string>();
            var warningCodes = new HashSet<string>(best.WarningReasons);

            if (best.HardFailCount > 0)
                notes.AddRange(best.HardFailReasons.Take(2).Select(r => $"HardFail: {r}"));

            if (best.WarningCount > 0)
            {
                foreach (var code in best.WarningReasons.Take(2))
                {
                    if (ContextMessageCatalog.TryGet(code, out var msg))
                    {
                        notes.Add($"Warning: {msg.TitleTr}");
                        warningTitles.Add(msg.TitleTr);
                    }
                    else
                    {
                        notes.Add($"Warning: {code}");
                    }
                }
            }

            // Context user-facing notes only if it actually hurts score
            if (best.ContextDelta < 0 && best.ContextUserNotes != null && best.ContextUserNotes.Count > 0)
            {
                foreach (var n in best.ContextUserNotes)
                {
                    if (n.Code != null && warningCodes.Contains(n.Code)) continue;
                    notes.Add($"Context: {n.Text}");
                    if (notes.Count >= 4) break;
                }
            }

            return notes;
        }

        private static List<string> BuildWhySelected(ScoredCombination best)
        {
            var why = new List<string>();

            foreach (var tb in best.TieBreakdown.Take(2))
                why.Add(tb.Reason);

            foreach (var b in best.Breakdown.Take(2))
            {
                if (why.Count >= 3) break;
                why.Add(b.Reason);
            }

            return why.Take(3).ToList();
        }

        internal static bool IsSameGarment(Garment? a, Garment? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;

            return a.Category == b.Category &&
                   a.ColorFamily == b.ColorFamily &&
                   a.Formality == b.Formality;
        }

        private static string ComputePoolHealth(double hardFailRate, double warningRate)
        {
            // MVP thresholds (tune later)
            if (hardFailRate <= 0.10 && warningRate <= 0.30) return "Good";
            if (hardFailRate <= 0.30 && warningRate <= 0.50) return "Okay";
            return "Poor";
        }

        private static ContextHealthLevel ComputeContextHealth(double avgDelta, double penaltyRate, double warningRate)
        {
            // Heuristics (Phase 2.5)
            // - warnings yüksekse: Poor
            // - ortalama delta çok negatifse: Poor
            // - biraz negatif ama makulse: Okay
            if (warningRate >= 0.30) return ContextHealthLevel.Poor;
            if (avgDelta <= -8) return ContextHealthLevel.Poor;

            if (warningRate >= 0.10) return ContextHealthLevel.Okay;
            if (avgDelta <= -2) return ContextHealthLevel.Okay;

            return ContextHealthLevel.Good;
        }


        private static ContextHealthLevel ComputeBestContextHealth(ScoredCombination best)
        {
            if (best.ContextWarningCodes.Count > 0) return ContextHealthLevel.Poor;
            if (best.ContextDelta <= -6) return ContextHealthLevel.Poor;

            if (best.ContextDelta < 0) return ContextHealthLevel.Okay;

            return ContextHealthLevel.Good;
        }

        private static FallbackOutfit BuildFallbackOutfit(List<CombinationCandidate>? generated, Occasion occasion)
        {
            var fallback = new FallbackOutfit
            {
                HeadlineTr = "Bugün için geçici öneri",
                SubtextTr = "Eksik parçalar var ama elindekilerle bunu giyebilirsin."
            };

            if (generated == null || generated.Count == 0)
                return fallback;

            // Öncelik: Anchor > Shoes > Top
            CombinationCandidate? candidate =
                generated.FirstOrDefault(c => c.Anchor != null)
                ?? generated.FirstOrDefault(c => c.SlotToItem.ContainsKey(Slot.Shoes))
                ?? generated.FirstOrDefault(c => c.SlotToItem.ContainsKey(Slot.Top))
                ?? generated.First();

            foreach (var kv in candidate.SlotToItem)
            {
                fallback.Items.Add(new OutfitItemDto(
                    slot: kv.Key.ToString(),
                    category: kv.Value.Category.ToString(),
                    colorFamily: kv.Value.ColorFamily.ToString()
                ));
            }

            return fallback;
        }
        private static ScoredCombination? GetBestViable(List<ScoredCombination>? ranked)
        {
            if (ranked == null || ranked.Count == 0)
                return null;

            return ranked.FirstOrDefault(x => x.HardFailCount == 0);
        }

        private static DecisionSummary BuildNoViableSummary(
            DecisionSummary summary,
            List<CombinationCandidate> generated,
            Occasion occasion)
        {
            summary.BestRisk = DecisionRiskLevel.HardFail;
            summary.FallbackOutfit = BuildFallbackOutfit(generated, occasion);

            var feedbackCode = generated.Count == 0
                ? "INCOMPLETE_OUTFIT"
                : "NO_VIABLE_COMBINATION";

            var gapType = generated.Count == 0
                ? WardrobeGapType.IncompleteOutfit
                : WardrobeGapType.IncompatibleOutfit;

            summary.WardrobeFeedback.Add(
                new WardrobeFeedback(
                    gapType,
                    feedbackCode,
                    feedbackCode
                )
            );

            summary.BestShort = "No viable combinations.";
            return summary;
        }


    }
}
