using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Domain.Semantics;
using Kombinle.Core.Engine;
using Kombinle.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Generation
{
    public class CombinationGenerator
    {
        private readonly AnchorSelector _anchorSelector = new();
        private readonly SupporterSelector _supporterSelector = new();

        // H2.3b Guardrails (MVP):
        private const int MaxVariantsPerAnchor = 2; // 1 anchor -> max 2 variant
        private const int MaxAltsPerSlot = 1;       // slot bazında max 1 alternatif dene

        public List<CombinationCandidate> Generate(
            List<Garment> wardrobe,
            Occasion occasion,
             ContextInput? context = null,
            int maxResults = 5)
        {

            var modes = occasion.CombinationModes;

            if (modes != null && modes.Count > 0)
            {
                var modeResults = new List<CombinationCandidate>();

                if (modes.Contains("Dress", StringComparer.OrdinalIgnoreCase))
                {
                    //modeResults.AddRange(GenerateDressMode(wardrobe, occasion, maxResults));
                    modeResults.AddRange(GenerateDressMode(wardrobe, occasion, context, maxResults * 3));
                }

                if (modes.Contains("TopBottom", StringComparer.OrdinalIgnoreCase))
                {
                    //modeResults.AddRange(GenerateTopBottomMode(wardrobe, occasion, maxResults));
                    modeResults.AddRange(GenerateTopBottomMode(wardrobe, occasion, context, maxResults * 3));
                }

                //return modeResults.Take(maxResults).ToList();
                return modeResults
                    .Where(x => !HasMultipleProtectionLayers(x))
                    .GroupBy(x => x.Signature)
                    .Select(g => g.First())
                    .ToList();
            }

            if (wardrobe == null || wardrobe.Count == 0) return new();
            if (occasion?.SlotSet?.Requirements == null || occasion.SlotSet.Requirements.Count == 0) return new();

            var results = new List<CombinationCandidate>();
            var seen = new HashSet<string>();

            var anchorReq = occasion.SlotSet.Get(Slot.Anchor);
            var anchorLevel = anchorReq?.Level ?? RequirementLevel.Hard;
            var effectiveContext = context ?? occasion.DefaultContext ?? new ContextInput();

            // 1) Candidate anchors
            var anchorCandidates = _anchorSelector.SelectAnchors(wardrobe, occasion, effectiveContext)
                .Select(a => (Garment: (Garment?)a.Garment, Reason: a.Reason))
                .ToList();

            // 2) If anchor is Soft, add a "no anchor" option
            if (anchorLevel == RequirementLevel.Soft)
            {
                anchorCandidates.Insert(0, (Garment: (Garment?)null, Reason: "Anchor soft: no anchor fallback."));
            }

            // 3) If anchor is Hard, and there are no anchors, return empty
            if (anchorLevel == RequirementLevel.Hard && anchorCandidates.Count == 0)
                return results;


            foreach (var anchorCandidate in anchorCandidates)
            {
                var anchor = anchorCandidate.Garment; // nullable
                var pool = _supporterSelector.BuildPool(wardrobe, occasion, anchor);

                if (pool.TryGetValue(Slot.Top, out var tops))
                {
                    Console.Error.WriteLine(
                        "TOP POOL: " + string.Join(",", tops.Select(x => x.CategoryId)));
                }

                // 1) Primary
                var primary = BuildPrimary(anchor, occasion, pool, anchorLevel, context);
                if (primary == null) continue;

                primary.Strategy = anchor == null ? "Primary:NoAnchor" : "Primary";
                primary.Reasons.Add(anchorCandidate.Reason);
                primary.Signature = BuildSignature(primary);

                if (seen.Add(primary.Signature))
                {
                    results.Add(primary);
                }

                //if (results.Count >= maxResults) return results;

                // 2) Controlled variants (guardrails)
                int variantsAddedForThisAnchor = 0;

                foreach (var slot in GetVariantSlots(occasion))
                {
                    if (variantsAddedForThisAnchor >= MaxVariantsPerAnchor) break;

                    if (!primary.SlotToItem.ContainsKey(slot)) continue;
                    if (!pool.ContainsKey(slot)) continue;

                    var used = primary.SlotToItem[slot];

                    int triedForSlot = 0;

                    foreach (var alt in pool[slot])
                    {
                        if (triedForSlot >= MaxAltsPerSlot) break;

                        if (ReferenceEquals(alt, used)) continue;
                        if (SameItem(alt, used)) continue;

                        var variant = CloneCandidate(primary);
                        variant.SlotToItem[slot] = alt;

                        variant.Combination = new Combination
                        {
                            Items = variant.SlotToItem.Values.Distinct().ToList()
                        };

                        variant.Strategy = $"Variant:{slot}";
                        variant.Reasons = new List<string>(primary.Reasons)
                        {
                            $"{slot} slotu değişti: {used.EffectiveCategoryId}/{used.ColorFamily} -> {alt.EffectiveCategoryId}/{alt.ColorFamily}"
                        };

                        variant.Signature = BuildSignature(variant);

                        if (seen.Add(variant.Signature))
                        {
                            results.Add(variant);
                            variantsAddedForThisAnchor++;
                        }

                        triedForSlot++;

                        //if (results.Count >= maxResults) return results;
                        if (variantsAddedForThisAnchor >= MaxVariantsPerAnchor) break;
                    }
                }

                // if (results.Count >= maxResults) return results;
            }
            //return results; // ✅ kritik: her durumda return
            return results
                    .Where(x => !HasMultipleProtectionLayers(x))
                    .Take(maxResults)
                    .ToList();
        }


        private List<CombinationCandidate> GenerateTopBottomMode(
            List<Garment> wardrobe,
            Occasion occasion,
            ContextInput? context,
            int maxResults)
        {
            if (wardrobe == null || wardrobe.Count == 0) return new();
            if (occasion?.SlotSet?.Requirements == null || occasion.SlotSet.Requirements.Count == 0) return new();

            var results = new List<CombinationCandidate>();
            var seen = new HashSet<string>();

            var anchorReq = occasion.SlotSet.Get(Slot.Anchor);
            var anchorLevel = anchorReq?.Level ?? RequirementLevel.Hard;
            var effectiveContext = context ?? occasion.DefaultContext ?? new ContextInput();

            var anchorCandidates = new List<(Garment? Garment, string Reason)>
            {
                (null, "TopBottom mode: no anchor baseline.")
            };

            var optionalAnchors = _anchorSelector.SelectAnchors(wardrobe, occasion, effectiveContext)
                .Select(a => (Garment: (Garment?)a.Garment, Reason: a.Reason))
                .Where(a => a.Garment != null && !CategorySemantics.IsOnePiece(a.Garment.EffectiveCategoryId))
                .ToList();

            anchorCandidates.AddRange(optionalAnchors);

            // 3) If anchor is Hard, and there are no anchors, return empty
            if (anchorLevel == RequirementLevel.Hard && anchorCandidates.Count == 0)
                return results;

            foreach (var anchorCandidate in anchorCandidates)
            {
                var anchor = anchorCandidate.Garment; // nullable
                var pool = _supporterSelector.BuildPool(wardrobe, occasion, anchor);

                // 🔥 Dress'i tamamen çıkar (TopBottom mode)
                foreach (var slot in pool.Keys.ToList())
                {
                    pool[slot] = pool[slot]
                                        .Where(g => !CategorySemantics.IsOnePiece(g.EffectiveCategoryId))
                                        .ToList();
                }

                // 1) Primary
                var primary = BuildPrimary(anchor, occasion, pool, anchorLevel, context);
                if (primary == null) continue;

                primary.Strategy = anchor == null ? "Primary:NoAnchor" : "Primary";
                primary.Reasons.Add(anchorCandidate.Reason);
                primary.Signature = BuildSignature(primary);

                if (seen.Add(primary.Signature))
                {
                    results.Add(primary);
                }

                if (results.Count >= maxResults) return results;

                // 2) Controlled variants (guardrails)
                int variantsAddedForThisAnchor = 0;

                foreach (var slot in GetVariantSlots(occasion))
                {
                    if (variantsAddedForThisAnchor >= MaxVariantsPerAnchor) break;

                    if (!primary.SlotToItem.ContainsKey(slot)) continue;
                    if (!pool.ContainsKey(slot)) continue;

                    var used = primary.SlotToItem[slot];

                    int triedForSlot = 0;

                    foreach (var alt in pool[slot])
                    {
                        if (triedForSlot >= MaxAltsPerSlot) break;

                        if (ReferenceEquals(alt, used)) continue;
                        if (SameItem(alt, used)) continue;

                        var variant = CloneCandidate(primary);
                        variant.SlotToItem[slot] = alt;

                        variant.Combination = new Combination
                        {
                            Items = variant.SlotToItem.Values.Distinct().ToList()
                        };

                        variant.Strategy = $"Variant:{slot}";
                        variant.Reasons = new List<string>(primary.Reasons)
                        {
                            $"{slot} slotu değişti: {used.EffectiveCategoryId}/{used.ColorFamily} -> {alt.EffectiveCategoryId}/{alt.ColorFamily}"
                        };

                        variant.Signature = BuildSignature(variant);

                        if (seen.Add(variant.Signature))
                        {
                            results.Add(variant);
                            variantsAddedForThisAnchor++;
                        }

                        triedForSlot++;

                        if (results.Count >= maxResults) return results;
                        if (variantsAddedForThisAnchor >= MaxVariantsPerAnchor) break;
                    }
                }

                if (results.Count >= maxResults) return results;
            }

            return results;
        }

        private List<CombinationCandidate> GenerateDressMode(
            List<Garment> wardrobe,
            Occasion occasion,
            ContextInput? context,
            int maxResults)
        {
            var results = new List<CombinationCandidate>();
            var seen = new HashSet<string>();

            var dresses = wardrobe
                             .Where(g => CategorySemantics.IsOnePiece(g.EffectiveCategoryId))
                             .ToList();

            var shoes = wardrobe
                .Where(g => CategorySemantics.CanFillShoesSlot(g.EffectiveCategoryId))
                .ToList();

            foreach (var dress in dresses)
            {
                foreach (var shoe in shoes)
                {
                    var slotMap = new Dictionary<Slot, Garment>
                    {
                        [Slot.Anchor] = dress,
                        [Slot.Shoes] = shoe
                    };

                    var candidate = new CombinationCandidate
                    {
                        Anchor = dress,
                        SlotToItem = slotMap,
                        Combination = new Combination
                        {
                            Items = slotMap.Values.ToList()
                        },
                        Strategy = "DressMode",
                        Reasons = new List<string> { "Dress kombin üretildi." }
                    };

                    candidate.Signature = BuildSignature(candidate);

                    if (seen.Add(candidate.Signature))
                    {
                        results.Add(candidate);
                    }

                    if (results.Count >= maxResults)
                        return results;
                }
            }

            return results;
        }
        private CombinationCandidate? BuildPrimary(
            Garment? anchor,
            Occasion occasion,
            Dictionary<Slot, List<Garment>> pool,
            RequirementLevel anchorLevel,
            ContextInput? context
        )
        {
            var slotMap = new Dictionary<Slot, Garment>();

            //if (anchor != null)
            //    slotMap[Slot.Anchor] = anchor;

            if (context?.Season == Season.Summer && context.Weather != Weather.Cold && pool.ContainsKey(Slot.Top))
            {
                pool[Slot.Top] = pool[Slot.Top]
                    .Where(g => !CategorySemantics.HasTrait(g.EffectiveCategoryId, SemanticTraits.Warm))
                    .ToList();
            }

            // Hard slots must be filled (including Anchor if Hard)
            foreach (var req in occasion.SlotSet.HardSlots)
            {
                if (req.Slot == Slot.Anchor)
                {
                    if (anchorLevel == RequirementLevel.Hard && anchor == null)
                        return null;

                    continue;
                }

                if (!pool.ContainsKey(req.Slot) || pool[req.Slot].Count == 0)
                    return null;

                slotMap[req.Slot] = pool[req.Slot][0];
            }



            // Optional slots
            foreach (var opt in occasion.SlotSet.OptionalSlots)
            {
                if (opt.Slot == Slot.Anchor) continue;

                if (opt.Slot == Slot.Outerwear && !ShouldIncludeOuterwear(context))
                    continue;

                if (!pool.ContainsKey(opt.Slot) || pool[opt.Slot].Count == 0) continue;

                var selected = pool[opt.Slot][0];

                if (opt.Slot == Slot.Outerwear && context != null)
                {
                    var candidates = pool[opt.Slot];

                    if (context.Season == Season.Summer)
                    {
                        candidates = candidates
                            .Where(g => !CategorySemantics.HasTrait(g.EffectiveCategoryId, SemanticTraits.Heavy))
                            .ToList();
                    }

                    var selectedOuterwear = SelectOuterwearForContext(candidates, context);

                    if (selectedOuterwear == null)
                        continue;

                    Console.Error.WriteLine(
    $"OUTERWEAR SELECTED: season={context.Season}, weather={context.Weather}, selected={selectedOuterwear.EffectiveCategoryId}, candidates={string.Join(",", pool[opt.Slot].Select(x => x.EffectiveCategoryId))}");

                    if (selectedOuterwear == null)
                        continue;

                    selected = selectedOuterwear;
                }

                //if (opt.Slot == Slot.Outerwear &&
                //         context?.Season == Season.Summer &&
                //         anchor != null &&
                //         CategorySemantics.IsStructuredLayer(anchor.EffectiveCategoryId) &&
                //         CategorySemantics.IsProtectionLayer(selected.EffectiveCategoryId))
                //{
                //    continue;
                //}

                if (opt.Slot == Slot.Outerwear &&
                                    anchor != null &&
                                    context != null)
                {
                    var anchorRole = SemanticRoleResolver.GetRole(anchor.EffectiveCategoryId);
                    var outerwearRole = SemanticRoleResolver.GetRole(selected.EffectiveCategoryId);

                    if (!SemanticCompatibilityMatrix.IsCompatible(
                            anchorRole,
                            outerwearRole,
                            context.Season))
                    {
                        continue;
                    }
                }

                slotMap[opt.Slot] = selected;
            }

            var comb = new Combination { Items = slotMap.Values.Distinct().ToList() };

            return new CombinationCandidate
            {
                Anchor = anchor, // nullable
                Combination = comb,
                SlotToItem = slotMap,
                Reasons = new List<string> { "Primary kombin üretildi (deterministik seçim)." }
            };
        }

        private static bool ShouldIncludeOuterwear(ContextInput? context)
        {
            if (context == null)
                return false;

            // Outwear is not suggested in Indoor occasions. 
            if (context.Setting == Setting.Indoor)
                return false;

            return context.Weather == Weather.Rain
                || context.Weather == Weather.Snow
                || context.Weather == Weather.Cold
                || (context.Season == Season.Winter && context.Setting == Setting.Outdoor)
                || (context.Season == Season.Autumn && context.Setting == Setting.Outdoor && context.Time == TimeOfDay.Night);
        }
        private static List<Slot> GetVariantSlots(Occasion occasion)
        {
            // MVP: Required slots only (excluding Anchor)
            return occasion.SlotSet.HardSlots
                .Where(r => r.Slot != Slot.Anchor)
                .Select(r => r.Slot)
                .ToList();
        }

        private static CombinationCandidate CloneCandidate(CombinationCandidate src)
        {
            return new CombinationCandidate
            {
                Anchor = src.Anchor,
                Combination = src.Combination,
                SlotToItem = src.SlotToItem.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value),
                Strategy = src.Strategy,
                Reasons = new List<string>(src.Reasons),
                Signature = src.Signature
            };
        }

        private static string BuildSignature(CombinationCandidate c)
        {

            var parts = new List<string>();

            if (c.Anchor != null)
            {
                parts.Add($"Anchor:{c.Anchor.EffectiveCategoryId}-{c.Anchor.ColorFamily}-{c.Anchor.Formality}");
            }

            parts.AddRange(
                c.SlotToItem
                    .Where(kv => kv.Value != null)
                    .OrderBy(kv => kv.Key)
                    .Select(kv => $"{kv.Key}:{kv.Value!.EffectiveCategoryId}-{kv.Value.ColorFamily}-{kv.Value.Formality}")
            );

            return string.Join("|", parts);
        }

        private static bool SameItem(Garment a, Garment b)
        {
            return string.Equals(a.EffectiveCategoryId, b.EffectiveCategoryId, StringComparison.OrdinalIgnoreCase) &&
                   a.ColorFamily == b.ColorFamily &&
                   a.Formality == b.Formality;
        }


        private static bool HasMultipleProtectionLayers(CombinationCandidate candidate)
        {
            var items = candidate.SlotToItem.Values.ToList();

            if (candidate.Anchor != null)
                items.Add(candidate.Anchor);

            var protectionCount = items
                .Distinct()
                .Count(x => CategorySemantics.IsProtectionLayer(x.EffectiveCategoryId));

            return protectionCount > 1;
        }

        private static Garment? SelectOuterwearForContext(
                IEnumerable<Garment> candidates,
                ContextInput context)
        {
            var list = candidates.ToList();

            if (list.Count == 0)
                return null;

            if (context.Season == Season.Summer)
            {
                return candidates.FirstOrDefault(g =>
                    !CategorySemantics.HasTrait(g.EffectiveCategoryId, SemanticTraits.Heavy));
            }

            if (context.Setting == Setting.Outdoor &&
                (context.Season == Season.Winter ||
                 context.Weather == Weather.Cold ||
                 context.Weather == Weather.Snow))
            {
                return list.FirstOrDefault(g =>
                      CategorySemantics.IsProtectionLayer(g.EffectiveCategoryId) &&
                      CategorySemantics.HasTrait(g.EffectiveCategoryId, SemanticTraits.Heavy))
                  ?? list[0];
            }

            //foreach (var g in candidates)
            //{
            //    Console.WriteLine(
            //        $"OUTERWEAR CANDIDATE: {g.EffectiveCategoryId}, Heavy={CategorySemantics.HasTrait(g.EffectiveCategoryId, SemanticTraits.Heavy)}, Light={CategorySemantics.HasTrait(g.EffectiveCategoryId, SemanticTraits.Light)}");
            //}

            return list[0];
        }

        private static bool IsOuterwearAllowedForContext(
            Garment garment,
            ContextInput context)
        {
            if (!CategorySemantics.IsProtectionLayer(garment.CategoryId))
                return true;

            if (context.Season == Season.Summer)
            {
                return !CategorySemantics.HasTrait(
                    garment.CategoryId,
                    SemanticTraits.Heavy);
            }

            return true;
        }
    }

}
