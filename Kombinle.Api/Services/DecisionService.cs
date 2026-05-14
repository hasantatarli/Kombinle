using Kombinle.Api.Contracts;
using Kombinle.Api.Mapping;
using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Engine;
using Kombinle.Core.Generation;
using Kombinle.Core.Scoring;
using Kombinle.Core.Infrastructure;

namespace Kombinle.Api.Services;

public sealed class DecisionService : IDecisionService
{
    public DecisionResponse Decide(DecisionRequest req)
    {
        // 1) Occasion resolve
        var occasion = OccasionResolver.Resolve(req.OccasionId);

        // 2) Context resolve (request override yoksa default)
        var effectiveContext = req.Context != null
            ? MappingHelpers.ToContextInput(req.Context)
            : occasion.DefaultContext;

        // 3) User prefs (opsiyonel)
        UserProfile? user = null;
        if (req.User?.FavoriteColors is { Count: > 0 })
        {
            user = new UserProfile
            {
                FavoriteColors = req.User.FavoriteColors
                    .Select(MappingHelpers.ParseColorFamily)
                    .ToList()
            };
        }

        // 4) Items -> wardrobe (şimdilik: sadece gelenleri wardrobe kabul ediyoruz)
        // Not: Bu P0 doğrulama için yeterli. Sonra “seed + expand wardrobe” yaparız.
        List<Garment> wardrobe;

        if (req.Items != null && req.Items.Count > 0)
        {
            wardrobe = req.Items.Select(MappingHelpers.ToGarment).ToList();
        }
        else if (!string.IsNullOrWhiteSpace(req.WardrobeProfileId))
        {
            wardrobe = TestWardrobeLoader.Load(req.WardrobeProfileId);
        }
        else
        {
            throw new Exception("No wardrobe provided");
        }

        // 5) Generate
        var generator = new CombinationGenerator();
        var combos = generator.Generate(
                            wardrobe,
                            occasion,
                            effectiveContext,
                            maxResults: 10);

        //Console.WriteLine("=== GENERATED ===");

        //foreach (var c in combos)
        //{
        //    Console.WriteLine(c.Signature);
        //}

        // 6) Score & Rank
        //var cfg = new ScoringConfig();
        var scoringConfigPath = Path.Combine(AppContext.BaseDirectory, "Resources", "scoring_config.json");
        var cfg = ScoringConfigLoader.LoadFromJsonFile(scoringConfigPath);

        var scorer = new CombinationScorer(cfg);
        var scored = combos.Select(c => scorer.Score(c, occasion, context: effectiveContext, user: user)).ToList();

        var ranker = new CombinationRanker();
        var ranked = ranker.Rank(scored);

        //Console.WriteLine("=== RANKED CANDIDATES ===");

        //foreach (var x in ranked)
        //{
        //    Console.WriteLine($"SIGNATURE: {x.Candidate.Signature}");
        //    Console.WriteLine($"Score={x.Score} TieBreak={x.TieBreakScore} ContextDelta={x.ContextDelta}");

        //    foreach (var b in x.Breakdown)
        //        Console.WriteLine($"{b.Value} | {b.Reason}");

        //    Console.WriteLine();
        //}

        //Console.WriteLine("=== RAIN SUEDE RANKED ===");

        //foreach (var x in ranked)
        //{
        //    Console.WriteLine($"SIGNATURE: {x.Candidate.Signature}");
        //    Console.WriteLine($"Score={x.Score} TieBreak={x.TieBreakScore} ContextDelta={x.ContextDelta}");
        //    Console.WriteLine($"Warnings={string.Join(",", x.ContextWarningCodes)}");
        //    foreach (var b in x.Breakdown)
        //        Console.WriteLine($"{b.Value} | {b.Reason}");
        //    Console.WriteLine();
        //}

        // 7) Summary
        var summary = DecisionSummaryBuilder.Build(
            scenarioTitle: occasion.Name,
            occasion: occasion,
            generated: combos,
            ranked: ranked,
            effectiveContext: effectiveContext,
            req.RotationAttempt,
            alternativeCount: 2,    
            alternativeMaxScoreGap: cfg.AlternativeMaxScoreGap,
            alternativeMaxScoreGap_DiverseAnchor: cfg.AlternativeMaxScoreGap_DiverseAnchor
        );

        summary.WardrobeGaps = WardrobeGapEngine.Analyze(
            occasion,
            wardrobe
        );

        // 8) API response mapping
        return ResponseMapper.ToResponse(summary);
    }
}
