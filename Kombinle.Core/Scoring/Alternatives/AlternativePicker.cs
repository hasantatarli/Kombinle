using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Kombinle.Core.Rules;

namespace Kombinle.Core.Scoring.Alternatives
{
    internal static class AlternativePicker
    {
        internal static List<ScoredCombination> Pick_ProductQuality(
            Occasion occasion,
            List<ScoredCombination> ranked,
            ScoredCombination best,
            int alternativeCount,
            int alternativeMaxScoreGap,
            int alternativeMaxScoreGap_DiverseAnchor)
        {
            var result = new List<ScoredCombination>();
            var seenSignatures = new HashSet<string>(StringComparer.Ordinal)
            {
                best.Candidate.Signature
            };

            var coreSlots = DecisionSummaryBuilder.GetCoreSlots(occasion);

            bool AllowedByScoreGap(ScoredCombination s)
                => (best.Score - s.Score) <= alternativeMaxScoreGap;

            bool AllowedByScoreGap_Diverse(ScoredCombination s)
                => (best.Score - s.Score) <= alternativeMaxScoreGap_DiverseAnchor;

            bool IsDifferentAnchor(ScoredCombination s)
            {
                // Best has no anchor -> don't force diverse anchor
                if (best.Candidate.Anchor == null) return true;

                // Alternative has no anchor -> it's definitely different (and likely a meaningful alt)
                if (s.Candidate.Anchor == null) return true;

                return !DecisionSummaryBuilder.IsSameGarment(best.Candidate.Anchor, s.Candidate.Anchor);
            }

            bool IsFormalNoAnchorFallback(ScoredCombination s)
            {
                return occasion.RequiredFormality >= Formality.Formal
                       && best.Candidate.Anchor != null
                       && s.Candidate.Anchor == null;
            }

            bool IsOnlyLayerRemovedAlternative(ScoredCombination s)
            {
                if (best.Candidate.Anchor == null || s.Candidate.Anchor != null)
                    return false;

                foreach (var slot in best.Candidate.SlotToItem.Keys.Union(s.Candidate.SlotToItem.Keys))
                {
                    best.Candidate.SlotToItem.TryGetValue(slot, out var bestItem);
                    s.Candidate.SlotToItem.TryGetValue(slot, out var altItem);

                    if (!DecisionSummaryBuilder.IsSameGarment(bestItem, altItem))
                        return false;
                }

                return true;
            }

            bool Accept(ScoredCombination s, bool relaxedGap = false)
            {
                if (s.HardFailCount > 0) return false;

                if (!(relaxedGap ? AllowedByScoreGap_Diverse(s) : AllowedByScoreGap(s)))
                    return false;

                var sig = s.Candidate.Signature ?? string.Empty;
                if (sig.Length == 0) return false;
                if (!seenSignatures.Add(sig)) return false;

                if (IsOnlyLayerRemovedAlternative(s))
                    return false;

                if (!DecisionSummaryBuilder.HasMeaningfulDifference(best.Candidate, s.Candidate, coreSlots))
                    return false;

                return true;
            }

            void StampAlternativeReason(ScoredCombination s)
            {
                void AddReasonCodeOnce(string code)
                {
                    if (!s.AlternativeReasonCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
                        s.AlternativeReasonCodes.Add(code);
                }

                bool IsSameGarment(Garment? a, Garment? b)
                {
                    if (a is null && b is null) return true;
                    if (a is null || b is null) return false;

                    return string.Equals(a.EffectiveCategoryId, b.EffectiveCategoryId, StringComparison.OrdinalIgnoreCase)
                           && a.ColorFamily == b.ColorFamily
                           && a.Formality == b.Formality;
                }

                int ColorHarmonyScore(ScoredCombination x)
                {
                    return x.Breakdown
                        .Where(b =>
                            b.Reason.StartsWith("Renk uyumu:", StringComparison.OrdinalIgnoreCase) ||
                            b.Reason.StartsWith("Zayıf renk uyumu:", StringComparison.OrdinalIgnoreCase))
                        .Sum(b => b.Value);
                }

                bool HasTopBottomStructure(ScoredCombination x)
                {
                    return x.Candidate.SlotToItem.ContainsKey(Slot.Top)
                           && x.Candidate.SlotToItem.ContainsKey(Slot.Bottom);
                }

                bool HasAnyItemSwap(ScoredCombination x, ScoredCombination y)
                {
                    foreach (var slot in x.Candidate.SlotToItem.Keys.Union(y.Candidate.SlotToItem.Keys))
                    {
                        x.Candidate.SlotToItem.TryGetValue(slot, out var xItem);
                        y.Candidate.SlotToItem.TryGetValue(slot, out var yItem);

                        if (!IsSameGarment(xItem, yItem))
                            return true;
                    }

                    return !IsSameGarment(x.Candidate.Anchor, y.Candidate.Anchor);
                }

              
                bool HasNeutralColorShift(ScoredCombination alt, ScoredCombination baseCombination)
                {
                    foreach (var slot in alt.Candidate.SlotToItem.Keys.Union(baseCombination.Candidate.SlotToItem.Keys))
                    {
                        alt.Candidate.SlotToItem.TryGetValue(slot, out var altItem);
                        baseCombination.Candidate.SlotToItem.TryGetValue(slot, out var baseItem);

                        if (altItem is null || baseItem is null)
                            continue;

                        if (IsSameGarment(altItem, baseItem))
                            continue;

                        if (!ColorRoleRules.IsNeutral(baseItem.ColorFamily) && ColorRoleRules.IsNeutral(altItem.ColorFamily))
                            return true;
                    }

                    return false;
                }

                if (ColorHarmonyScore(s) > ColorHarmonyScore(best))
                {
                    AddReasonCodeOnce("ALT_COLOR_ADVANTAGE");
                }

                if (HasNeutralColorShift(s, best))
                {
                    AddReasonCodeOnce("ALT_COLOR_STYLE_SHIFT_NEUTRAL");
                }

                if (occasion.RequiredFormality >= Formality.Formal &&
                        best.Candidate.Anchor != null && CategorySemantics.IsOnePiece(best.Candidate.Anchor.EffectiveCategoryId)
                        && HasTopBottomStructure(s))
                {
                    AddReasonCodeOnce("ALT_STRUCTURE_TOP_BOTTOM");
                }
                else if (occasion.RequiredFormality >= Formality.Formal
                    && best.Candidate.Anchor != null
                    && s.Candidate.Anchor == null)
                {
                    AddReasonCodeOnce("ALT_NO_ANCHOR_FORMAL");
                }

                if (occasion.RequiredFormality < Formality.Formal
                    && best.Candidate.Anchor != null
                    && s.Candidate.Anchor == null
                    && !HasCoreItemSwap(best, s))
                {
                    AddReasonCodeOnce("ALT_LAYER_REMOVED");
                }

                if (best.Candidate.SlotToItem.TryGetValue(Slot.Top, out var bestTop)
                    && s.Candidate.SlotToItem.TryGetValue(Slot.Top, out var altTop)
                    && !IsSameGarment(bestTop, altTop))
                {
                    if (string.Equals(altTop.EffectiveCategoryId, "Shirt", StringComparison.OrdinalIgnoreCase)
                             && altTop.ColorFamily == ColorFamily.White)
                    {
                        AddReasonCodeOnce("ALT_SHIRT_SWAP_WHITE");
                    }
                    else if (string.Equals(altTop.EffectiveCategoryId, "Shirt", StringComparison.OrdinalIgnoreCase)
                             && altTop.ColorFamily == ColorFamily.Blue)
                    {
                        AddReasonCodeOnce("ALT_SHIRT_SWAP_BLUE");
                    }
                }

                if (best.Candidate.SlotToItem.TryGetValue(Slot.Bottom, out var bestBottom)
                    && s.Candidate.SlotToItem.TryGetValue(Slot.Bottom, out var altBottom)
                    && !IsSameGarment(bestBottom, altBottom))
                {
                    if (!string.Equals(bestBottom.EffectiveCategoryId, altBottom.EffectiveCategoryId, StringComparison.OrdinalIgnoreCase))
                    {
                        AddReasonCodeOnce("ALT_BOTTOM_STRUCTURE_SHIFT");
                    }
                }

                if (best.Candidate.SlotToItem.TryGetValue(Slot.Shoes, out var bestShoes)
                    && s.Candidate.SlotToItem.TryGetValue(Slot.Shoes, out var altShoes)
                    && !IsSameGarment(bestShoes, altShoes))
                {
                    if (altShoes.ColorFamily == ColorFamily.Black)
                        AddReasonCodeOnce("ALT_SHOES_SWAP_BLACK");
                    else if (altShoes.ColorFamily == ColorFamily.Brown)
                        AddReasonCodeOnce("ALT_SHOES_SWAP_BROWN");
                }

                if (HasOuterwearRelevantContext(s)
                    && s.Candidate.Anchor == null
                    && s.Candidate.SlotToItem.ContainsKey(Slot.Outerwear))
                {
                    AddReasonCodeOnce("ALT_SOFT_OUTERWEAR_OPTIONAL");
                }

                if (s.AlternativeReasonCodes.Count == 0)
                {
                    if (HasAnyItemSwap(best, s))
                        AddReasonCodeOnce("ALT_ITEM_SWAP_VARIATION");
                    else
                        AddReasonCodeOnce("ALT_GENERAL_VARIATION");
                }
            }

            bool SameGarment(Garment? a, Garment? b)
            {
                if (a is null && b is null) return true;
                if (a is null || b is null) return false;

                return string.Equals(a.EffectiveCategoryId, b.EffectiveCategoryId, StringComparison.OrdinalIgnoreCase)
                       && a.ColorFamily == b.ColorFamily
                       && a.Formality == b.Formality;
            }

            bool HasCoreItemSwap(ScoredCombination x, ScoredCombination y)
            {
                foreach (var slot in x.Candidate.SlotToItem.Keys.Union(y.Candidate.SlotToItem.Keys))
                {
                    if (slot == Slot.Outerwear)
                        continue;

                    x.Candidate.SlotToItem.TryGetValue(slot, out var xItem);
                    y.Candidate.SlotToItem.TryGetValue(slot, out var yItem);

                    if (!SameGarment(xItem, yItem))
                        return true;
                }

                return false;
            }

            bool HasOuterwearRelevantContext(ScoredCombination x)
            {
                return x.ContextWarningCodes.Any(code =>
                    code.Contains("OUTDOOR", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("RAIN", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("COLD", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("NIGHT", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("OUTERWEAR", StringComparison.OrdinalIgnoreCase));
            }


            // ---------- C2: context-aware ordering ----------
            bool IsBestContextPoor()
            {
                if (best.ContextWarningCodes != null && best.ContextWarningCodes.Count > 0) return true;
                if (best.ContextDelta <= -6) return true;
                return false;
            }

            int ContextSafetyScore(ScoredCombination s)
            {
                int score = 0;

                int warnCount = s.ContextWarningCodes?.Count ?? 0;
                score += (warnCount == 0) ? 50 : (-20 * warnCount);

                score += (s.ContextDelta - best.ContextDelta);

                if (score > 80) score = 80;
                if (score < -80) score = -80;

                return score;
            }

            var candidates =
                IsBestContextPoor()
                    ? ranked.Skip(1)
                        .OrderByDescending(ContextSafetyScore)
                        .ThenByDescending(x => x.Score)
                        .ThenByDescending(x => x.TieBreakScore)
                        .ToList()
                    : ranked.Skip(1).ToList();

            // Pass 1: Prefer different anchor/path alternatives first.
            // Different anchor alternatives are allowed to use the wider diverse-anchor score gap.
            // This keeps meaningful Dress vs TopBottom alternatives visible without letting HardFail items pass.
            foreach (var s in candidates)
            {
                if (result.Count >= alternativeCount) break;
                if (!IsDifferentAnchor(s)) continue;
                if (IsFormalNoAnchorFallback(s)) continue;
                if (!Accept(s, relaxedGap: true)) continue;

                StampAlternativeReason(s);
                result.Add(s);
            }

            // Pass 2: Fill remaining with any meaningful alternatives
            foreach (var s in candidates)
            {
                if (result.Count >= alternativeCount) break;
                if (IsFormalNoAnchorFallback(s)) continue;
                if (!Accept(s)) continue;

                StampAlternativeReason(s);
                result.Add(s);
            }

            // Pass 3: If still not enough, allow a more distant but different-anchor fallback
            if (result.Count < alternativeCount)
            {
                foreach (var s in candidates)
                {
                    if (result.Count >= alternativeCount) break;

                    if (!IsDifferentAnchor(s)) continue;
                    if (!Accept(s, relaxedGap: true)) continue;

                    StampAlternativeReason(s);
                    result.Add(s);
                }
            }

            return result
                .OrderBy(x => x.WarningCount > 0 ? 1 : 0)
                .ThenByDescending(x => x.Score)
                .ThenByDescending(x => x.TieBreakScore)
                .ToList();
        }
    }
}
