using System;
using System.Collections.Generic;
using System.Linq;
using Kombinle.Core.Domain;

namespace Kombinle.Core.Scoring.WardrobeFeedbackRules
{
    public static class WardrobeFeedbackEngine
    {
        private const double MissingItemWarnRateThreshold = 0.80; // >= 80%

        public static IReadOnlyList<Domain.WardrobeFeedback> Evaluate(IReadOnlyList<ScoredCombination> ranked)
        {
            if (ranked == null) throw new ArgumentNullException(nameof(ranked));
            if (ranked.Count == 0) return Array.Empty<Domain.WardrobeFeedback>();

            // Presence count per combination (not frequency)
            var presenceCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var sc in ranked)
            {
                if (sc?.ContextWarningCodes == null || sc.ContextWarningCodes.Count == 0)
                    continue;

                foreach (var code in sc.ContextWarningCodes
                             .Where(c => !string.IsNullOrWhiteSpace(c))
                             .Select(c => c.Trim())
                             .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    presenceCounts.TryGetValue(code, out var count);
                    presenceCounts[code] = count + 1;
                }
            }

            if (presenceCounts.Count == 0)
                return Array.Empty<Domain.WardrobeFeedback>();

            var poolSize = ranked.Count;

            //Console.WriteLine("=== WARDROBE FEEDBACK COUNTS ===");

            //foreach (var kvp in presenceCounts)
            //{
            //    var rate = (double)kvp.Value / poolSize;
            //    Console.WriteLine($"{kvp.Key} Count={kvp.Value} Pool={poolSize} Rate={rate:0.00} Priority={GetWarningPriority(kvp.Key)}");
            //}

            var top = presenceCounts
                        .Select(kvp => new
                        {
                            Code = kvp.Key,
                            Rate = (double)kvp.Value / poolSize,
                            Priority = GetWarningPriority(kvp.Key)
                        })
                        .Where(x => x.Rate >= MissingItemWarnRateThreshold)
                        .OrderBy(x => x.Priority)
                        .ThenByDescending(x => x.Rate)
                        .FirstOrDefault();

            if (top == null)
                return Array.Empty<Domain.WardrobeFeedback>();

            // Engine-neutral message (UI tonu sonra)
            var msg = $"Bu koşulda dolap kapsaması zayıf: {top.Code}";

            return new[]
            {
                new Domain.WardrobeFeedback(WardrobeGapType.MissingItemForContext, top.Code, msg)
            };
        }
        private static int GetWarningPriority(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return 999;

            if (code.StartsWith("RAIN_", StringComparison.OrdinalIgnoreCase))
                return 10;

            if (code.StartsWith("OUTDOOR_", StringComparison.OrdinalIgnoreCase))
                return 20;

            if (code.StartsWith("NIGHT_", StringComparison.OrdinalIgnoreCase))
                return 30;

            if (code.StartsWith("SOFT_", StringComparison.OrdinalIgnoreCase))
                return 90;

            return 50;
        }
    }
}
