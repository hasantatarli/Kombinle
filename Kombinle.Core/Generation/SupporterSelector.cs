using Kombinle.Core.Domain;
using Kombinle.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Generation
{
    public class SupporterSelector
    {
        public Dictionary<Slot, List<Garment>> BuildPool(
            List<Garment> wardrobe,
            Occasion occasion,
            Garment? anchor)
        {
            var pool = new Dictionary<Slot, List<Garment>>();

            foreach (var req in occasion.SlotSet.Requirements)
            {
                if (req.Slot == Slot.Anchor) continue;

                var targetFormality = occasion.RequiredFormality;

                var list = wardrobe
                    .Where(g =>
                         SlotRequirementMatcher.Matches(g, req) &&
                            (anchor == null || !IsSameGarment(g, anchor)) &&
                            (anchor == null || !ColorRules.IsClashing(anchor.ColorFamily, g.ColorFamily))
                    )
                    .OrderBy(g => Math.Abs(GetFormalityRank(g.Formality) - GetFormalityRank(targetFormality)))
                    .ThenBy(g => GetFormalityRank(g.Formality))
                    .ToList();

                pool[req.Slot] = list;
            }

            return pool;
        }
        private static bool IsSameGarment(Garment a, Garment b)
        {
            return string.Equals(a.EffectiveCategoryId, b.EffectiveCategoryId, StringComparison.OrdinalIgnoreCase)
                   && a.ColorFamily == b.ColorFamily
                   && a.Formality == b.Formality;
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
    }

}
