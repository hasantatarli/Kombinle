using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring
{
    public static class WardrobeGapEngine
    {
        public static List<WardrobeGap> Analyze(
            Occasion occasion,
            IReadOnlyList<Garment> items)
        {
            var result = new List<WardrobeGap>();

            bool isCasualLike =
                    occasion.RequiredFormality == Formality.Casual ||
                    occasion.RequiredFormality == Formality.Smart;

            if (!isCasualLike)
                return result;

            bool hasCasualTop = items.Any(x =>
                CategorySemantics.CanFillTopSlot(x.EffectiveCategoryId) &&
                (
                    CategorySemantics.Provider.HasTrait(x.EffectiveCategoryId, SemanticTraits.Casual) ||
                    x.Formality == Formality.Casual
                ));

            bool hasCasualBottom = items.Any(x =>
                CategorySemantics.CanFillBottomSlot(x.EffectiveCategoryId) &&
                (
                    CategorySemantics.Provider.HasTrait(x.EffectiveCategoryId, SemanticTraits.Casual) ||
                    x.Formality == Formality.Casual
                ));

            bool hasCasualShoes = items.Any(x =>
                CategorySemantics.CanFillShoesSlot(x.EffectiveCategoryId) &&
                (
                    CategorySemantics.Provider.HasTrait(x.EffectiveCategoryId, SemanticTraits.Casual) ||
                    x.Formality == Formality.Casual
                ));

            if (!hasCasualTop)
            {
                result.Add(new WardrobeGap(
                    code: "MISSING_CASUAL_TOP",
                    type: WardrobeGapTypeV2.MissingCasualTop,
                    categoryId: "Shirt",
                    suggestionType: WardrobeSuggestionType.CasualUpgrade,
                    priority: 10));
            }

            if (!hasCasualBottom)
            {
                result.Add(new WardrobeGap(
                    code: "MISSING_CASUAL_BOTTOM",
                    type: WardrobeGapTypeV2.MissingCasualBottom,
                    categoryId: "Pants",
                    suggestionType: WardrobeSuggestionType.CasualUpgrade,
                    priority: 10));
            }

            if (!hasCasualShoes)
            {
                result.Add(new WardrobeGap(
                    code: "MISSING_CASUAL_SHOES",
                    type: WardrobeGapTypeV2.MissingCasualShoes,
                    categoryId: "Shoes",
                    suggestionType: WardrobeSuggestionType.CasualUpgrade,
                    priority: 10));
            }

            return result;
        }
    }
}
