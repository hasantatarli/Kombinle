using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Rules
{
    public static class WardrobeRules
    {
        public static bool IsCombinationPossible(
            Combination combination,
            List<Garment> wardrobe,
            out List<string> failReasons)
        {
            failReasons = new List<string>();

            foreach (var item in combination.Items)
            {
                bool exists = wardrobe.Any(w =>
                    string.Equals(w.EffectiveCategoryId, item.EffectiveCategoryId, StringComparison.OrdinalIgnoreCase) &&
                    w.ColorFamily == item.ColorFamily &&
                    w.Formality == item.Formality);

                if (!exists)
                {
                    failReasons.Add(
                        $"Dolapta olmayan parça: {item.EffectiveCategoryId} ({item.ColorFamily})");
                }
            }

            return failReasons.Count == 0;
        }
    }

}
