using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
    public enum WardrobeGapTypeV2
    {
        MissingCasualTop,
        MissingCasualBottom,
        MissingCasualShoes
    }

    public enum WardrobeSuggestionType
    {
        CasualUpgrade,
        FormalUpgrade,
        WeatherProtection,
        Versatility
    }

    public sealed class WardrobeGap
    {
        public string Code { get; }
        public WardrobeGapTypeV2 Type { get; }
        public string CategoryId { get; }
        public WardrobeSuggestionType SuggestionType { get; }
        public int Priority { get; }

        public WardrobeGap(
            string code,
            WardrobeGapTypeV2 type,
            string categoryId,
            WardrobeSuggestionType suggestionType,
            int priority)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Wardrobe gap code cannot be null or empty.", nameof(code));

            Code = code.Trim();
            Type = type;
            CategoryId = categoryId;
            SuggestionType = suggestionType;
            Priority = priority;
        }
    }
}
