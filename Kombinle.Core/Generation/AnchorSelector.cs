using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Generation
{
    public class AnchorSelector
    {
        public List<AnchorCandidate> SelectAnchors(List<Garment> wardrobe, Occasion occasion)
        {
            var anchorReq = occasion.SlotSet.Get(Slot.Anchor);
            if (anchorReq == null) return new List<AnchorCandidate>();

            var candidates = wardrobe
                .Where(g =>
                    anchorReq.AllowedCategories.Contains(g.Category) &&
                    g.Formality >= occasion.RequiredFormality)
                .Select(g => new AnchorCandidate
                {
                    Garment = g,
                    Priority = (int)g.Formality,
                    Reason = $"Anchor adayı: {g.Category} ({g.ColorFamily}, {g.Formality})"
                })
                .OrderByDescending(c => c.Priority)
                .ToList();

            return candidates;
        }
    }

}
