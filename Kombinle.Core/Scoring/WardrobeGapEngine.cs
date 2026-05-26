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
                CategorySemantics.CanFillTopSlot(x.Category) &&
                (
                    CategorySemantics.Provider.HasTrait(x.Category, SemanticTraits.Casual) ||
                    x.Formality == Formality.Casual
                ));

            bool hasCasualBottom = items.Any(x =>
                CategorySemantics.CanFillBottomSlot(x.Category) &&
                (
                    CategorySemantics.Provider.HasTrait(x.Category, SemanticTraits.Casual) ||
                    x.Formality == Formality.Casual
                ));

            bool hasCasualShoes = items.Any(x =>
                CategorySemantics.CanFillShoesSlot(x.Category) &&
                (
                    CategorySemantics.Provider.HasTrait(x.Category, SemanticTraits.Casual) ||
                    x.Formality == Formality.Casual
                ));

            if (!hasCasualTop)
            {
                result.Add(new WardrobeGap(
                    code: "MISSING_CASUAL_TOP",
                    type: WardrobeGapTypeV2.MissingCasualTop,
                    category: Category.Shirt,
                    suggestionType: WardrobeSuggestionType.CasualUpgrade,
                    priority: 10));
            }

            if (!hasCasualBottom)
            {
                result.Add(new WardrobeGap(
                    code: "MISSING_CASUAL_BOTTOM",
                    type: WardrobeGapTypeV2.MissingCasualBottom,
                    category: Category.Pants,
                    suggestionType: WardrobeSuggestionType.CasualUpgrade,
                    priority: 10));
            }

            if (!hasCasualShoes)
            {
                result.Add(new WardrobeGap(
                    code: "MISSING_CASUAL_SHOES",
                    type: WardrobeGapTypeV2.MissingCasualShoes,
                    category: Category.Shoes,
                    suggestionType: WardrobeSuggestionType.CasualUpgrade,
                    priority: 10));
            }

            return result;
        }
    }
}
