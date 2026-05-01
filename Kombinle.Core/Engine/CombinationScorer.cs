using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Generation;
using Kombinle.Core.Rules;
using Kombinle.Core.Scoring;
using Kombinle.Core.Scoring.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Engine
{
    public class CombinationScorer
    {
        private readonly ScoringConfig _cfg;
        private readonly ContextScoringService _contextScoring = new();


        public CombinationScorer(ScoringConfig cfg)
        {
            _cfg = cfg;
        }

        public ScoredCombination Score(CombinationCandidate candidate, Occasion occasion, ContextInput? context = null, UserProfile? user = null)
        {
            var result = new ScoredCombination { Candidate = candidate };

            var items = candidate.Combination.Items;
            var anchor = candidate.Anchor;

            bool isDressMode = candidate.Anchor?.Category == Category.Dress;
            bool isTopBottomMode = !isDressMode;

            // 1) Formality sinyali (Generator zaten filtreliyor ama skor sinyali olarak tutuyoruz)
            //if (items.All(i => i.Formality >= occasion.RequiredFormality))
            //    result.Add(_cfg.FormalityMatch, "Formality: Occasion gereksinimi karşılandı");
            //else
            //    result.Add(_cfg.FormalityMismatch, "Formality: Occasion gereksinimi karşılanmadı");

            var targetFormality = occasion.RequiredFormality;

            var formalityDistance = items
                .Select(i => Math.Abs((int)i.Formality - (int)targetFormality))
                .Sum();

            var exactMatchCount = items.Count(i => i.Formality == targetFormality);

            if (formalityDistance == 0)
            {
                int bonus = _cfg.FormalityMatch;

                if (isDressMode)
                    bonus += 1;

                result.Add(bonus, "Formality: Occasion hedef formality ile tam uyumlu");
            }
            else
            {
                var penalty = -2 * formalityDistance;

                result.Add(penalty,
                    $"Formality: Occasion hedefinden sapma var (distance={formalityDistance})");

                if (exactMatchCount > 0)
                {
                    result.Add(exactMatchCount,
                        $"Formality: Hedef formality ile eşleşen parça sayısı {exactMatchCount}");
                }
            }

            // Casual occasion'da formal anchor fazla baskın görünür.
            // Casual jacket/blazer kabul edilebilir; Formal anchor ise hafif değil, belirgin ceza almalı.
            if (occasion.RequiredFormality == Formality.Casual
                && candidate.Anchor != null
                && candidate.Anchor.Formality == Formality.Formal)
            {
                result.Add(-6, "Formality: Casual occasion için formal anchor fazla güçlü");
            }

            // 2) Renk çifti skorları (anchor-aware)

            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    var a = items[i];
                    var b = items[j];

                    bool involvesAnchor = IsSameGarment(a, anchor) || IsSameGarment(b, anchor);

                    // Color compatibility ile pair relation weight'i ayırıyoruz.
                    // Pair önemli olsa bile renk zayıfsa tam bonus verilmez;
                    // clash varsa ise ceza ve HardFail uygulanır.
                    var compatibility = ColorRules.GetCompatibility(a.ColorFamily, b.ColorFamily);

                    if (compatibility == ColorCompatibility.Clash)
                    {
                        int pen = involvesAnchor ? _cfg.ColorClash_AnchorPair : _cfg.ColorClash_OtherPair;
                        result.Add(pen, $"Renk çakışması: {a.ColorFamily} – {b.ColorFamily}");
                        result.AddHardFail($"Renk çakışması: {a.ColorFamily} ile {b.ColorFamily}");
                        continue;
                    }

                    int relationWeight;

                    if (IsCorePair(a, b, isDressMode))
                    {
                        relationWeight = involvesAnchor ? _cfg.ColorMatch_AnchorPair : _cfg.ColorMatch_OtherPair;
                    }
                    else if (IsSupportPair(a, b, isDressMode))
                    {
                        relationWeight = 2;
                    }
                    else
                    {
                        relationWeight = 1;
                    }

                    int scoreDelta = compatibility switch
                    {
                        ColorCompatibility.StrongMatch => relationWeight,
                        ColorCompatibility.Acceptable => Math.Min(1, relationWeight),
                        ColorCompatibility.WeakMatch => -1,
                        _ => 0
                    };

                    if (scoreDelta > 0)
                        result.Add(scoreDelta, $"Renk uyumu: {a.Category} - {b.Category}");
                    else if (scoreDelta < 0)
                        result.Add(scoreDelta, $"Zayıf renk uyumu: {a.Category} - {b.Category}");
                }
            }

            // Dress core structure bonus (Top+Bottom yerine geçer)
            if (isDressMode)
            {
                result.Add(2, "Dress: Tek parça uyumlu yapı bonusu");
            }

            // 3) Kullanıcı favori renk bonusu (opsiyonel)
            if (user?.FavoriteColors != null && user.FavoriteColors.Count > 0)
            {
                bool usedFav = items.Any(i => user.FavoriteColors.Contains(i.ColorFamily));
                if (usedFav)
                    result.Add(_cfg.FavoriteColorBonus, "Kullanıcı favori rengi kullanıldı");
            }

            if (context != null)
            {
                //Console.WriteLine($"[CTX] Weather={context.Weather} Setting={context.Setting} Time={context.Time}");

                var cx = _contextScoring.Apply(candidate, context);


                //Console.WriteLine($"[SCORE-CX] delta={cx.DeltaScore} reasons={cx.Reasons.Count} warnings={cx.WarningCodes.Count}");
                result.ContextUserNotes = cx.UserNotes
                        .Where(n => n != null && !string.IsNullOrWhiteSpace(n.Text))
                        .Select(n => new ContextUserNote(n.Code, n.Text.Trim()))
                        .Distinct()
                        .ToList();


                result.ContextDelta += cx.DeltaScore;
                result.Add(cx.DeltaScore, $"Context delta {cx.DeltaScore}");

                result.ContextReasons.AddRange(cx.Reasons);
                result.ContextWarningCodes.AddRange(cx.WarningCodes);

                foreach (var w in cx.WarningCodes)
                    result.AddWarning(w); // RiskOf bunu görebilsin diye
            }
            else
            {
                Console.WriteLine("[CTX] context is NULL");
            }

            // 🔥 MODE-AWARE tweak: TopBottom ama anchor yoksa küçük penalty
            if (isTopBottomMode && candidate.Anchor == null)
            {
                result.Add(-1, "Anchor eksik (TopBottom mode)");
            }

            // 4) Düşük skor = Warning (HardFail değil)
            if (result.Score < _cfg.WarningThreshold)
            {
                result.AddWarning($"Düşük skor uyarısı: {result.Score} (<{_cfg.WarningThreshold})");
            }

            // -----------------------
            // 5) TIE-BREAK (ana skoru bozmaz)
            // -----------------------

            // 5a) Neutral bonus (cap’li)
            var neutralCount = items.Count(i => ColorRules.IsNeutral(i.ColorFamily));
            if (neutralCount > 0)
            {
                var neutralPts = Math.Min(_cfg.NeutralBonusCap, neutralCount * _cfg.NeutralBonusPerItem);
                if (neutralPts > 0)
                    result.AddTieBreak(neutralPts, $"TieBreak: Neutral renkler ({neutralCount} parça) +{neutralPts}");
            }

            // 5b) Occasion bazlı güvenli anchor rengi bonusu
            if (candidate.Anchor != null &&
                occasion.PreferredAnchorColors != null &&
                occasion.PreferredAnchorColors.Count > 0 &&
                occasion.PreferredAnchorColors.Contains(candidate.Anchor.ColorFamily))
            {
                int pts = occasion.PreferredAnchorColorTieBreakBonus;
                if (pts != 0)
                    result.AddTieBreak(pts, $"TieBreak: Occasion için güvenli anchor rengi ({candidate.Anchor.ColorFamily}) +{pts}");
            }

            // 5c) Optional outerwear doluysa bonus
            bool outerwearOptional = occasion.SlotSet.OptionalSlots.Any(s => s.Slot == Slot.Outerwear);
            bool hasOuterwear = candidate.SlotToItem.ContainsKey(Slot.Outerwear);

            if (outerwearOptional && hasOuterwear)
            {
                result.AddTieBreak(_cfg.OptionalOuterwearBonus,
                    $"TieBreak: Optional outerwear tamamlandı +{_cfg.OptionalOuterwearBonus}");
            }

            // Casual preference (soft bias for Smart/Casual occasions)
            if (occasion.RequiredFormality == Formality.Casual || occasion.RequiredFormality == Formality.Smart)
            {
                bool hasCasualSignal =
                    candidate.SlotToItem.Values.Any(x =>
                        x.Category == Category.Sneakers
                        || x.Category == Category.Jeans
                        || x.Category == Category.Tshirt
                        || (x.Category == Category.Shoes && x.Formality == Formality.Casual)
                        || (x.Category == Category.Pants && x.Formality == Formality.Casual)
                        || (x.Category == Category.Shirt && x.Formality == Formality.Casual)
                    );

                if (hasCasualSignal)
                {
                    result.AddTieBreak(1, "TieBreak: Casual occasion için rahat parça sinyali +1");
                }
            }

            return result;
        }

        private static bool IsCorePair(Garment a, Garment b, bool isDressMode)
        {
            if (isDressMode)
            {
                return (a.Category == Category.Dress && IsFootwearCategory(b.Category)) ||
                       (b.Category == Category.Dress && IsFootwearCategory(a.Category));
            }

            return (IsTopCategory(a.Category) && IsBottomCategory(b.Category)) ||
                   (IsTopCategory(b.Category) && IsBottomCategory(a.Category));
        }

        private static bool IsSupportPair(Garment a, Garment b, bool isDressMode)
        {
            if (isDressMode)
            {
                return false;
            }

            return (IsBottomCategory(a.Category) && IsFootwearCategory(b.Category)) ||
                   (IsBottomCategory(b.Category) && IsFootwearCategory(a.Category));
        }

        private static bool IsTopCategory(Category category) =>
            category == Category.Shirt ||
            category == Category.Blouse ||
            category == Category.Tshirt ||
            category == Category.Sweater ||
            category == Category.Hoodie ||
            category == Category.Cardigan ||
            category == Category.Jacket ||
            category == Category.Blazer ||
            category == Category.Coat;

        private static bool IsBottomCategory(Category category) =>
            category == Category.Pants ||
            category == Category.Skirt ||
            category == Category.Jeans;

        private static bool IsFootwearCategory(Category category) =>
            category == Category.Shoes ||
            category == Category.Sneakers;

        // Aynı özelliklerde farklı obje olma ihtimaline karşı
        private static bool IsSameGarment(Garment? a, Garment? b)
        {
            if (ReferenceEquals(a, b)) return true;

            if (a == null || b == null) return false;

            return a.Category == b.Category &&
                   a.ColorFamily == b.ColorFamily &&
                   a.Formality == b.Formality;
        }

    }
}
