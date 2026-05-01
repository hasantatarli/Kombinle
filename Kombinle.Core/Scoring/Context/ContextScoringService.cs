using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Domain.Traits;
using Kombinle.Core.Generation;
using Kombinle.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring.Context
{
    public class ContextScoringService
    {
        private const int PenaltyCapMin = -22;

        public ContextResult Apply(CombinationCandidate candidate, ContextInput context)
        {
            var res = new ContextResult();

            var hasShoes = candidate.SlotToItem.ContainsKey(Slot.Shoes);
            var hasOuterwear = candidate.SlotToItem.ContainsKey(Slot.Outerwear);

            var shoesItem = hasShoes ? candidate.SlotToItem[Slot.Shoes] : null;
            var owItem = hasOuterwear ? candidate.SlotToItem[Slot.Outerwear] : null;

            //Console.WriteLine($"[CTX] HasShoes={hasShoes} HasOuterwear={hasOuterwear}");
            //Console.WriteLine($"[CTX] ShoesTraits={(shoesItem?.Shoe == null ? "NULL" : "OK")}");
            //Console.WriteLine($"[CTX] OuterwearTraits={(owItem?.Outerwear == null ? "NULL" : "OK")}");

            ApplyRain(candidate, context, res);
            ApplyOutdoor(candidate, context, res);
            ApplyNight(candidate, context, res);

            if (res.DeltaScore < PenaltyCapMin)
                res.DeltaScore = PenaltyCapMin;

            return res;
        }

        private static void ApplyRain(CombinationCandidate candidate, ContextInput context, ContextResult res)
        {
            //Console.WriteLine("[CTX] Slot keys: " + string.Join(", ", candidate.SlotToItem.Keys));
            //Console.WriteLine("[CTX] Anchor category: " + candidate.Anchor.Category);

            if (context.Weather != Weather.Rain) return;

            var shoes = FindShoes(candidate);
            var shoesItem = candidate.SlotToItem.TryGetValue(Slot.Shoes, out var si) ? si : null;

            var hasOuterwear = HasOuterwear(candidate);
            var protection = FindOuterwearProtection(candidate);

            // Rain protection bonus
            if (protection != null && protection.Value == WeatherProtection.Rain && protection.Confidence >= 0.8)
            {
                res.DeltaScore += 3;
                res.Reasons.Add("Rain: Outerwear rain protection (+3)");
                res.UserNotes.Add(new ContextUserNote("OUTDOOR_NO_OUTERWEAR", "Dışarıda dış katman (ceket/coat) faydalı olabilir."));

            }

            // Suede risk
            if (shoes?.Material != null && shoes.Material.Value == ShoeMaterial.Suede)
            {
                var conf = shoes.Material.Confidence;
                var pen = conf >= 0.8 ? -10 : conf >= 0.5 ? -5 : -2;

                res.DeltaScore += pen;
                res.WarningCodes.Add("RAIN_SUEDE_SHOES");
                res.Reasons.Add(conf >= 0.8
                    ? "Rain: Suede shoes risk"
                    : "Rain: Shoes likely suede (uncertain)");
                res.UserNotes.Add(new ContextUserNote("RAIN_SUEDE_SHOES", "Yağmurda süet ayakkabı daha riskli olabilir."));

            }
            else if (shoes?.WaterResistance != null && shoes.WaterResistance.Value == WaterResistance.Low)
            {
                var conf = shoes.WaterResistance.Confidence;
                var pen = conf >= 0.8 ? -6 : conf >= 0.5 ? -3 : -1;

                res.DeltaScore += pen;
                res.WarningCodes.Add("RAIN_LOW_WATER_SHOES");
                res.Reasons.Add(conf >= 0.8
                    ? "Rain: Low water resistance shoes"
                    : "Rain: Shoes likely low water resistance (uncertain)");
            }

            // If rain and shoes exist but we have no traits at all => soft pressure
            if (shoesItem != null && shoes == null)
            {
                res.DeltaScore += -2;
                res.Reasons.Add("Rain: Shoes traits missing (soft pressure)");
                res.UserNotes.Add(new ContextUserNote(null, "Hava koşullarına uygunluk için ayakkabı bilgisi eksik."));


            }
            else if (shoes != null && shoes.Material == null && shoes.WaterResistance == null)
            {
                res.DeltaScore += -2;
                res.Reasons.Add("Rain: Shoes material/resistance unknown (soft pressure)");
            }
        }

        private static void ApplyOutdoor(CombinationCandidate candidate, ContextInput context, ContextResult res)
        {
            if (context.Setting != Setting.Outdoor) return;

            // Outdoor'da "no outerwear" / bazı riskler sadece gerçekten ihtiyaç varsa devreye girsin
            var needsOuterwear = (context.Weather == Weather.Rain) || (context.Time == TimeOfDay.Night);

            if (!needsOuterwear) return;

            // 1) Outerwear yoksa penalty + warning
            if (!HasOuterwear(candidate))
            {
                res.DeltaScore += -4;
                res.WarningCodes.Add("OUTDOOR_NO_OUTERWEAR");
                res.Reasons.Add("Outdoor: No outerwear");
                res.UserNotes.Add(new ContextUserNote("OUTDOOR_NO_OUTERWEAR", "Dışarıda dış katman (ceket/coat) faydalı olabilir."));

            }

            // 2) Outdoor + (Rain/Night) iken suede shoes ekstra soft risk
            var shoes = FindShoes(candidate);
            if (shoes?.Material != null && shoes.Material.Value == ShoeMaterial.Suede)
            {
                res.DeltaScore += -3;
                res.Reasons.Add("Outdoor: Suede shoes (soft risk)");
            }

        }

        private static void ApplyNight(CombinationCandidate candidate, ContextInput context, ContextResult res)
        {
            if (context.Time != TimeOfDay.Night) return;
            if (context.Setting != Setting.Outdoor) return;

            var items = candidate.SlotToItem.Values.ToList();
            if (candidate.Anchor != null)
                items.Add(candidate.Anchor);

            int brightCount = items.Count(i => ColorRules.IsBright(i.ColorFamily));

            if (brightCount == 0)
            {
                res.DeltaScore += -2;
                res.UserNotes.Add(new ContextUserNote(
                    "NIGHT_LOW_VISIBILITY",
                    "Gece dışarıda daha görünür renkler tercih edilebilir."
                ));
            }
        }


        private static ShoeTraits? FindShoes(CombinationCandidate candidate)
            => candidate.SlotToItem.TryGetValue(Slot.Shoes, out var s) ? s.Shoe : null;

        private static bool HasOuterwear(CombinationCandidate candidate)
            => candidate.SlotToItem.ContainsKey(Slot.Outerwear);

        private static TagValue<WeatherProtection>? FindOuterwearProtection(CombinationCandidate candidate)
           => candidate.SlotToItem.TryGetValue(Slot.Outerwear, out var o)
                ? o.Outerwear?.Protection
                : null;
    }
}
