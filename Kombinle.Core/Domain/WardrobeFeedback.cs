using System;

namespace Kombinle.Core.Domain
{
    public enum WardrobeGapType
    {
        MissingItemForContext,
        LowVarietyForContext,
        FragileBestChoice,
        MissingSoftAnchor,
        IncompleteOutfit,
        IncompatibleOutfit
    }

    public sealed class WardrobeFeedback
    {
        public WardrobeGapType Type { get; }
        public string ContextWarningCode { get; }
        public string Message { get; } // short, neutral

        public WardrobeFeedback(WardrobeGapType type, string contextWarningCode, string message)
        {
            if (string.IsNullOrWhiteSpace(contextWarningCode))
                throw new ArgumentException("Context warning code cannot be null or empty.", nameof(contextWarningCode));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            Type = type;
            ContextWarningCode = contextWarningCode.Trim();
            Message = message.Trim();
        }
    }
}
