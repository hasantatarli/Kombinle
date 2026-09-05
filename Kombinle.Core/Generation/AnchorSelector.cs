using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Generation
{
    public class AnchorSelector
    {
        public List<AnchorCandidate> SelectAnchors(List<Garment> wardrobe, Occasion occasion, ContextInput context)
        {
            var anchorReq = occasion.SlotSet.Get(Slot.Anchor);
            if (anchorReq == null) return new List<AnchorCandidate>();

            //Console.WriteLine($"ANCHOR REQ: {string.Join(",", anchorReq.AllowedCategories)}");
            //Console.WriteLine($"ANCHOR TRAITS: {string.Join(",", anchorReq.AllowedTraits)}");

            //foreach (var g in wardrobe)
            //{
            //    Console.WriteLine(
            //        $"ANCHOR CHECK {g.Category} {g.Formality} | " +
            //        $"match={SlotRequirementMatcher.Matches(g, anchorReq)} | " +
            //        $"formality={g.Formality >= occasion.RequiredFormality} | " +
            //        $"protection={CategorySemantics.IsProtectionLayer(g.Category)} | " +
            //        $"structure={CategorySemantics.IsStructuredLayer(g.Category)}");
            //}

            var candidates = wardrobe
                .Where(g =>
                {
                    if (context.Setting == Setting.Indoor &&
                        context.Weather == Weather.Clear &&
                        CategorySemantics.IsProtectionLayer(g.EffectiveCategoryId) &&
                        !CategorySemantics.IsStructuredLayer(g.EffectiveCategoryId))
                    {
                        return false;
                    }

                    if (context.Season == Season.Summer &&
                        context.Weather != Weather.Cold &&
                        string.Equals(g.EffectiveCategoryId, "Hoodie", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    if (occasion.RequiredFormality == Formality.Casual &&
                        CategorySemantics.IsStructuredLayer(g.EffectiveCategoryId))
                    {
                        return false;
                    }

                    return
                         SlotRequirementMatcher.Matches(g, anchorReq);
                })
                .Select(g => new AnchorCandidate
                {
                    Garment = g,
                    Priority = -Math.Abs((int)g.Formality - (int)occasion.RequiredFormality),
                    Reason = $"Anchor adayı: {g.EffectiveCategoryId} ({g.ColorFamily}, {g.Formality})"
                })
                .OrderByDescending(c => c.Priority)
                .ToList();

            return candidates;
        }
    }

}
