using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Domain.Traits;
using Kombinle.Core.Generation;
using Kombinle.Core.Rules;


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

            ApplyLayerSuitability(candidate, context, res);


            if (res.DeltaScore < PenaltyCapMin)
                res.DeltaScore = PenaltyCapMin;

            return res;
        }

        private static void ApplyLayerSuitability(CombinationCandidate candidate, ContextInput context, ContextResult res)
        {
            var intensity = GetTotalLayerIntensity(candidate);
            var hasProtectionLayer = HasLayerRole(candidate, LayerRole.Protection);

            //Console.WriteLine($"[LAYER] Season={context.Season} Setting={context.Setting} Intensity={intensity}");

            if (context.Season == Season.Summer && context.Setting == Setting.Indoor)
            {
                if (intensity >= 4)
                {
                    res.DeltaScore -= 12;
                    // Console.WriteLine("[LAYER] Summer indoor high intensity penalty applied");
                    res.UserNotes.Add(new ContextUserNote("UNNECESSARY_LAYER_INDOOR", "İç mekân ve yaz koşullarında daha hafif kombin daha rahat olur."));
                }
                else if (intensity >= 3)
                {
                    res.DeltaScore -= 6;
                    //Console.WriteLine("[LAYER] Summer indoor medium intensity penalty applied");
                    res.UserNotes.Add(new ContextUserNote("UNNECESSARY_LAYER_INDOOR", "İç mekân ve yaz koşullarında daha hafif kombin daha rahat olur."));
                }
                else if (intensity == 2)
                {
                    res.DeltaScore -= 3;
                    //Console.WriteLine("[LAYER] Summer indoor structured layer penalty applied");
                    res.UserNotes.Add(new ContextUserNote("UNNECESSARY_LAYER_INDOOR", "İç mekân ve yaz koşullarında daha hafif kombin daha rahat olur."));
                }
                else if (intensity == 1)
                {
                    res.DeltaScore -= 2;
                    //Console.WriteLine("[LAYER] Summer indoor light layer penalty applied");
                    res.UserNotes.Add(new ContextUserNote("UNNECESSARY_LAYER_INDOOR", "İç mekân ve yaz koşullarında daha hafif kombin daha rahat olur."));
                }
            }

            if (context.Season == Season.Winter && context.Setting == Setting.Outdoor)
            {
                if (intensity <= 1)
                {
                    res.DeltaScore -= 12;
                    //Console.WriteLine("[LAYER] Winter outdoor low intensity penalty applied");
                    res.UserNotes.Add(new ContextUserNote("OUTDOOR_NO_OUTERWEAR", "Dış ortamda daha koruyucu bir katman faydalı olabilir."));
                }
                else if (intensity == 2)
                {
                    res.DeltaScore -= 4;
                    //Console.WriteLine("[LAYER] Winter outdoor medium-low intensity penalty applied");
                    res.UserNotes.Add(new ContextUserNote("OUTDOOR_NO_OUTERWEAR", "Dış ortamda daha koruyucu bir katman faydalı olabilir."));
                }

                if (hasProtectionLayer)
                {
                    res.DeltaScore += 3;
                    //Console.WriteLine("[LAYER] Winter outdoor protection bonus applied");
                    res.UserNotes.Add(new ContextUserNote("WINTER_OUTDOOR_PROTECTIVE_LAYER", "Soğuk/dış ortam için dış katman kombini daha koruyucu hale getirir."));
                }
            }
        }

        private static void ApplyRain(CombinationCandidate candidate, ContextInput context, ContextResult res)
        {
            //Console.WriteLine("[CTX] Slot keys: " + string.Join(", ", candidate.SlotToItem.Keys));
            //Console.WriteLine("[CTX] Anchor category: " + candidate.Anchor.Category);

            if (context.Weather != Weather.Rain) return;

            var shoes = FindShoes(candidate);
            var shoesItem = candidate.SlotToItem.TryGetValue(Slot.Shoes, out var si) ? si : null;

            var hasOuterwear = HasOuterwear(candidate);
            var hasProtectionLayer = HasLayerRole(candidate, LayerRole.Protection);
            var hasComfortLayer = HasLayerRole(candidate, LayerRole.Comfort);
            var hasStructureLayer = HasLayerRole(candidate, LayerRole.Structure);
            var protection = FindOuterwearProtection(candidate);


            // Rain expects protective outerwear.
            // Comfort layers like hoodie/cardigan do not count as rain protection.
            if (!hasProtectionLayer)
            {
                res.DeltaScore -= 4;
                res.Reasons.Add("Rain: No protective outer layer");
            }

            if (hasComfortLayer && !hasProtectionLayer && !hasStructureLayer)
            {
                res.DeltaScore -= 2;
                res.Reasons.Add("Rain: Comfort layer only");
            }

            // Rain protection bonus
            if (protection != null && protection.Value == WeatherProtection.Rain && protection.Confidence >= 0.8)
            {
                res.DeltaScore += 3;
                res.Reasons.Add("Rain: Outerwear rain protection (+3)");
                // res.UserNotes.Add(new ContextUserNote("OUTDOOR_NO_OUTERWEAR", "Dışarıda dış katman (ceket/coat) faydalı olabilir."));

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
            var needsOuterwear =
                context.Weather == Weather.Rain ||
                context.Season == Season.Winter ||
                context.Weather == Weather.Cold ||
                (context.Time == TimeOfDay.Night && context.Season != Season.Summer);

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
        {
            var hasInSlots = candidate.SlotToItem.Values.Any(x =>
                CategorySemantics.IsLightLayer(x.Category) ||
                CategorySemantics.IsStructuredLayer(x.Category) ||
                CategorySemantics.IsHeavyLayer(x.Category));

            var hasAnchorLayer =
                candidate.Anchor != null &&
                (CategorySemantics.IsLightLayer(candidate.Anchor.Category) ||
                 CategorySemantics.IsStructuredLayer(candidate.Anchor.Category) ||
                 CategorySemantics.IsHeavyLayer(candidate.Anchor.Category));

            return hasInSlots || hasAnchorLayer;
        }

        private static TagValue<WeatherProtection>? FindOuterwearProtection(CombinationCandidate candidate)
           => candidate.SlotToItem.TryGetValue(Slot.Outerwear, out var o)
                ? o.Outerwear?.Protection
                : null;

        //private static bool IsLightLayer(Category category)
        //{
        //    return category == Category.Cardigan ||
        //           category == Category.Hoodie;
        //}

        //private static bool IsStructuredLayer(Category category)
        //{
        //    return category == Category.Jacket;
        //}

        //private static bool IsHeavyLayer(Category category)
        //{
        //    return category == Category.Coat;
        //}

        //private static LayerRole GetLayerRole(Category category)
        //{
        //    if (category == Category.Hoodie ||
        //        category == Category.Cardigan)
        //        return LayerRole.Comfort;

        //    if (category ==  Category.Jacket)
        //        return LayerRole.Structure;

        //    if (category == Category.LightOuterwear ||
        //        category == Category.Coat)
        //        return LayerRole.Protection;


        //    return LayerRole.None;
        //}

        private static int GetLayerIntensity(Category category)
        {
            return CategorySemantics.GetLayerRole(category) switch
            {
                LayerRole.Comfort => 1,
                LayerRole.Structure => 2,
                LayerRole.Protection => 3,
                _ => 0
            };
        }

        private static int GetTotalLayerIntensity(CombinationCandidate candidate)
        {
            var total = candidate.SlotToItem.Values
                .Sum(x => GetLayerIntensity(x.Category));

            if (candidate.Anchor != null)
                total += GetLayerIntensity(candidate.Anchor.Category);

            return total;
        }
        private static bool HasLayerRole(CombinationCandidate candidate, LayerRole role)
        {
            return candidate.SlotToItem.Values.Any(x =>
                CategorySemantics.GetLayerRole(x.Category) == role);
        }
    }
}
