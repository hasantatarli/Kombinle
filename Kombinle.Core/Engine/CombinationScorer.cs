using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Domain.Semantics;
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

            bool isDressMode = candidate.Anchor != null && CategorySemantics.IsOnePiece(candidate.Anchor.Category);
            bool isTopBottomMode = !isDressMode;

            var targetFormality = occasion.RequiredFormality;
            var targetRank = GetFormalityRank(targetFormality);

            var formalityDistance = items
                .Select(i => Math.Abs(GetFormalityRank(i.Formality) - targetRank))
                .Sum();

            var exactMatchCount = items.Count(i => i.Formality == targetFormality);

            //Console.WriteLine($"TargetFormality={targetFormality}");

            //foreach (var item in items)
            //{
            //    Console.WriteLine(
            //        $"Item={item.Category}, Formality={item.Formality}, RankDistance={Math.Abs(GetFormalityRank(item.Formality) - targetRank)}");
            //}

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

            var preferredStyleTrait = OccasionStylePreferences.Get(occasion.Id);

            if (!string.IsNullOrWhiteSpace(preferredStyleTrait))
            {
                foreach (var item in candidate.Combination.Items)
                {
                    if (CategorySemantics.HasStyleTrait(item.Category, preferredStyleTrait))
                    {
                        result.Add(
                            1,
                            $"Style suitability: {item.Category} matches {preferredStyleTrait}");
                    }
                }

                if (candidate.Anchor != null &&
                    CategorySemantics.HasStyleTrait(candidate.Anchor.Category, preferredStyleTrait))
                {
                    result.Add(
                        1,
                        $"Style suitability: {candidate.Anchor.Category} matches {preferredStyleTrait}");
                }
            }

            // Casual occasion'da formal anchor fazla baskın görünür.
            // Casual jacket/Jacket kabul edilebilir; Formal anchor ise hafif değil, belirgin ceza almalı.
            if (occasion.RequiredFormality == Formality.Casual
                && candidate.Anchor != null
                && candidate.Anchor.Formality == Formality.Formal)
            {
                result.Add(-6, "Formality: Casual occasion için formal anchor fazla güçlü");
            }

            if (candidate.Anchor != null)
            {
                var role = CategorySemantics.GetLayerRole(candidate.Anchor.Category);

                if (occasion.RequiredFormality >= Formality.Smart &&
                    role == LayerRole.Structure)
                {
                    var bonus =
                            occasion.RequiredFormality == Formality.Formal
                                ? 6
                                : 2;

                    result.Add(bonus, "Anchor semantic: Smart/Formal occasion için structured layer uygun");
                }

                if (occasion.RequiredFormality == Formality.Casual &&
                    role == LayerRole.Comfort)
                {
                    result.Add(2, "Anchor semantic: Casual occasion için comfort layer uygun");
                }

                if (context?.Setting == Setting.Outdoor &&
                    (context.Weather == Weather.Rain ||
                     context.Weather == Weather.Cold ||
                     context.Season == Season.Winter) &&
                    role == LayerRole.Protection)
                {
                    result.Add(2, "Anchor semantic: Outdoor/soğuk/yağış için protective layer uygun");
                }
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

            var anchorReq = occasion.SlotSet.Get(Slot.Anchor);

            if (anchorReq?.Level == RequirementLevel.Soft &&
                candidate.Anchor != null)
            {
                result.Add(4, "Soft anchor kullanıldı");
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
                            CategorySemantics.Provider.HasTrait(x.Category, SemanticTraits.Casual) ||
                            x.Formality == Formality.Casual
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
            return CategorySemantics.IsCorePair(a.Category, b.Category, isDressMode);
        }

        private static bool IsSupportPair(Garment a, Garment b, bool isDressMode)
        {
            return CategorySemantics.IsSupportPair(a.Category, b.Category, isDressMode);
        }



        // Aynı özelliklerde farklı obje olma ihtimaline karşı
        private static bool IsSameGarment(Garment? a, Garment? b)
        {
            if (ReferenceEquals(a, b)) return true;

            if (a == null || b == null) return false;

            return a.Category == b.Category &&
                   a.ColorFamily == b.ColorFamily &&
                   a.Formality == b.Formality;
        }


        private static int GetFormalityRank(Formality formality)
        {
            return formality switch
            {
                Formality.Casual => 0,
                Formality.Smart => 1,
                Formality.Formal => 2,
                _ => 0
            };
        }

        private static int ScoreSoftAnchorPresence(CombinationCandidate candidate, Occasion occasion)
        {
            var anchorReq = occasion.SlotSet.Get(Slot.Anchor);

            if (anchorReq == null || anchorReq.Level != RequirementLevel.Soft)
                return 0;

            if (candidate.Anchor == null)
                return 0;

            return 4;
        }

    }
}
