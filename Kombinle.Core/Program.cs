// See https://aka.ms/new-console-template for more information
using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Domain.Traits;
using Kombinle.Core.Engine;
using Kombinle.Core.Generation;
using Kombinle.Core.Rules;
using Kombinle.Core.Scoring;
using Kombinle.Core.Scoring.Alternatives;
using Kombinle.Core.Scoring.Context;

class Program
{
    static void Main()
    {

        Console.WriteLine(new string('-', 60));
        Test_BusinessMeeting_Formal();

        //Console.WriteLine(new string('-', 60));
        //Test_CasualWeekend();

        //Console.WriteLine(new string('-', 60));
        //Test_Interview_Formal();
    }

    static void Test_WeddingFormal_JacketOrDress()
    {
        var wardrobe = new List<Garment>
        {
            new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Navy,  Formality = Formality.Formal },
            new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Black, Formality = Formality.Formal },

            new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Formal },
            new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.Blue,  Formality = Formality.Formal },

            new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Navy,  Formality = Formality.Formal },
            new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey,  Formality = Formality.Formal },
            new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Brown, Formality = Formality.Formal },

            new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Formal },
        };

        var occasion = Occasion.WeddingFormal_JacketOrDress();

        RunScenario(
            scenarioTitle: "TEST: WeddingFormal_JacketOrDress",
            wardrobe: wardrobe,
            occasion: occasion,
            maxResults: 5,
            user: BuildUserProfile(useFavoriteColor: false, favorite: ColorFamily.Navy),
            validateCandidate: null // bu senaryoda ekstra assert yok
        );
    }

    static void Test_WeddingFormal_DressOnly()
    {
        // Kadın akışı: Dress + Shoes zorunlu, Outerwear opsiyonel
        var wardrobe = new List<Garment>
        {
            new Garment { Category = Category.Dress, ColorFamily = ColorFamily.Navy, Formality = Formality.Formal },
            new Garment { Category = Category.Dress, ColorFamily = ColorFamily.Red,  Formality = Formality.Formal },   // varyasyon için
            new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Formal },

            // Outerwear opsiyonel (varsa primary'ye eklenecek)
            new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Black, Formality = Formality.Formal },
            new Garment { Category = Category.Coat,   ColorFamily = ColorFamily.Grey,  Formality = Formality.Formal },
        };

        var occasion = Occasion.WeddingFormal_DressOnly();

        RunScenario(
            scenarioTitle: "TEST: WeddingFormal_DressOnly",
            wardrobe: wardrobe,
            occasion: occasion,
            maxResults: 5,
            user: BuildUserProfile(useFavoriteColor: false, favorite: ColorFamily.Navy),
            validateCandidate: c =>
            {
                // DressOnly'de Top/Bottom beklemiyoruz
                if (c.SlotToItem.ContainsKey(Slot.Top) || c.SlotToItem.ContainsKey(Slot.Bottom))
                {
                    Console.WriteLine("❌ HATA: DressOnly occasion'da Top/Bottom slotu dolmamalı.");
                }
            }
        );
    }


    static void Test_BusinessMeeting_Formal()
    {
        var wardrobe = BuildMixedWardrobe();
        var occasion = Occasion.BusinessMeeting_Formal();

        RunScenario(
            scenarioTitle: "TEST: Business Meeting (Formal)",
            wardrobe: wardrobe,
            occasion: occasion,
            maxResults: 6,
            user: null,
            context: null
        );
    }

    static void Test_CasualWeekend()
    {
        var wardrobe = BuildMixedWardrobe();
        var occasion = Occasion.CasualWeekend();

        RunScenario(
            scenarioTitle: "TEST: Casual Weekend",
            wardrobe: wardrobe,
            occasion: occasion,
            maxResults: 6,
            user: null,
            context: null,
            mutateCombosForTest: combos =>
            {
                // Best'in suede olmasını zorla
                foreach (var c in combos)
                {
                    if (c.SlotToItem.TryGetValue(Slot.Shoes, out var s))
                    {
                        s.Shoe = new ShoeTraits
                        {
                            Material = new TagValue<ShoeMaterial>(ShoeMaterial.Suede, TagSource.User, 1.0)
                        };
                    }
                }
            }

        );
    }
    static void Test_Interview_Formal()
    {
        var wardrobe = BuildMixedWardrobe();
        var occasion = Occasion.Interview_Formal();

        RunScenario(
            scenarioTitle: "TEST: Interview (Formal)",
            wardrobe: wardrobe,
            occasion: occasion,
            maxResults: 6,
            user: null,
            context: null
        );
    }

    static List<Garment> BuildMixedWardrobe()
    {
        return new List<Garment>
        {
            // Jackets
            new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Navy,  Formality = Formality.Formal },
            new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Black, Formality = Formality.Formal },
            new Garment { Category = Category.Jacket, ColorFamily = ColorFamily.Brown, Formality = Formality.Formal }, // clash risk

            // Tops
            new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Formal },
            new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.Blue,  Formality = Formality.Formal },
            new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.Red,   Formality = Formality.Casual }, // low formality

            // Bottoms
            new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey,  Formality = Formality.Formal },
            new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Brown, Formality = Formality.Formal }, // clash risk

            // Shoes
            new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Formal },
            new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Brown, Formality = Formality.Casual }, // clash + casual

            // Outerwear
            //new Garment { Category = Category.Coat, ColorFamily = ColorFamily.Grey, Formality = Formality.Formal }
        };
    }

    static void PrintResults(string title, Occasion occasion, List<CombinationCandidate> combos)
    {
        Console.WriteLine(title);
        Console.WriteLine($"Occasion: {occasion.Name}");
        Console.WriteLine($"Üretilen kombin sayısı: {combos.Count}");
        Console.WriteLine();

        int idx = 1;
        foreach (var c in combos)
        {
            Console.WriteLine($"=== Kombin #{idx} ({c.Strategy}) ===");
            Console.WriteLine(c.Anchor == null ? "Anchor: (none)" : $"Anchor: {c.Anchor.Category} / {c.Anchor.ColorFamily}"
);

            foreach (var kv in c.SlotToItem.OrderBy(k => k.Key))
            {
                var g = kv.Value;
                Console.WriteLine($"- {kv.Key}: {g.Category} / {g.ColorFamily} / {g.Formality}");
            }

            Console.WriteLine($"Signature: {c.Signature}");
            Console.WriteLine("Reasons:");
            foreach (var r in c.Reasons) Console.WriteLine($"  * {r}");
            Console.WriteLine();

            idx++;
        }
    }

    // ----------------------------
    // Shared scenario runner
    // ----------------------------
    static void RunScenario(
        string scenarioTitle,
        List<Garment> wardrobe,
        Occasion occasion,
        int maxResults,
        UserProfile? user,
        ContextInput? context = null,
        Action<CombinationCandidate>? validateCandidate = null,
        Action<List<CombinationCandidate>>? mutateCombosForTest = null
    )
    {
        // 1) Generate
        var generator = new CombinationGenerator();
        var combos = generator.Generate(wardrobe, occasion, maxResults: maxResults);

        foreach (var c in combos)
        {
            if (c.SlotToItem.TryGetValue(Slot.Shoes, out var shoeGarment))
            {
                shoeGarment.Shoe ??= new ShoeTraits();
                shoeGarment.Shoe.Material = new TagValue<ShoeMaterial>(
                    ShoeMaterial.Suede,
                    TagSource.User,
                    1.0
                );
            }
        }


        // Optional validation
        if (validateCandidate != null)
            foreach (var c in combos) validateCandidate(c);

        // Optional test-only mutation (kapalı default)
        mutateCombosForTest?.Invoke(combos);

        // 2) Score
        var cfg = new ScoringConfig();
        var scorer = new CombinationScorer(cfg);

        var effectiveContext = context ?? occasion.DefaultContext;
        var scored = combos.Select(c => scorer.Score(c, occasion, context: effectiveContext, user: user)).ToList();

        // 3) Rank
        var ranker = new CombinationRanker();
        var ranked = ranker.Rank(scored);

        // 4) Summary
        var summary = DecisionSummaryBuilder.Build(
            scenarioTitle: scenarioTitle,
            occasion: occasion,
            generated: combos,
            ranked: ranked,
            effectiveContext: effectiveContext,
            alternativeCount: 2,
            alternativeMaxScoreGap: cfg.AlternativeMaxScoreGap,
            alternativeMaxScoreGap_DiverseAnchor: cfg.AlternativeMaxScoreGap_DiverseAnchor);

        // 5) Print (minimal + useful)
        PrintSummary(occasion, summary);
        PrintBestContextSummary(summary);
    }


    // ----------------------------
    // Printing PrintBestContextSummary
    // ----------------------------
    static void PrintBestContextSummary(DecisionSummary summary)
    {
        Console.WriteLine("=== CONTEXT SUMMARY (BEST) ===");
        if (summary.Best == null)
        {
            Console.WriteLine("No best combination.");
            return;
        }
        Console.WriteLine($"ContextDelta: {summary.Best.ContextDelta}");

        if (summary.Best.ContextReasons.Count == 0)
            Console.WriteLine("ContextReasons: (none)");
        else
            foreach (var r in summary.Best.ContextReasons)
                Console.WriteLine(" - " + r);

        Console.WriteLine("ContextWarnings:");
        if (summary.Best.ContextWarningCodes.Count == 0)
        {
            Console.WriteLine(" (none)");
        }
        else
        {
            foreach (var code in summary.Best.ContextWarningCodes)
            {
                if (ContextMessageCatalog.TryGet(code, out var msg))
                    Console.WriteLine($" - {msg.TitleTr} ({code})");
                else
                    Console.WriteLine($" - {code}");
            }
        }

        Console.WriteLine();
    }
    // ----------------------------
    // Printing helpers
    // ----------------------------
    //static void PrintCombos(List<CombinationCandidate> combos)
    //{
    //    int idx = 1;
    //    foreach (var c in combos)
    //    {
    //        Console.WriteLine($"=== Kombin #{idx} ({c.Strategy}) ===");
    //        Console.WriteLine($"Anchor: {c.Anchor.Category} / {c.Anchor.ColorFamily}");

    //        foreach (var kv in c.SlotToItem.OrderBy(k => k.Key))
    //        {
    //            var g = kv.Value;
    //            Console.WriteLine($"- {kv.Key}: {g.Category} / {g.ColorFamily} / {g.Formality}");
    //        }

    //        Console.WriteLine($"Signature: {c.Signature}");
    //        Console.WriteLine("Reasons:");
    //        foreach (var r in c.Reasons) Console.WriteLine($"  * {r}");
    //        Console.WriteLine();

    //        idx++;
    //    }
    //}
    static void PrintSummary(Occasion occasion, DecisionSummary summary)
    {
        Console.WriteLine("=== SUMMARY ===");
        Console.WriteLine($"Scenario: {summary.ScenarioTitle}");
        var cx = summary.EffectiveContext;
        Console.WriteLine($"Context: {(cx == null ? "(none)" : $"{cx.Weather}/{cx.Setting}/{cx.Time}")}");
        Console.WriteLine($"Occasion: {occasion.Name} | RequiredFormality: {occasion.RequiredFormality}");
        Console.WriteLine($"Generated: {summary.GeneratedCount} | Ranked: {summary.RankedCount}");
        Console.WriteLine($"HardFailed: {summary.HardFailedCount} | Warnings: {summary.WarningCount}");
        Console.WriteLine($"PoolHealth: {summary.PoolHealth} (HardFail {(summary.HardFailRate * 100):0}% | Warn {(summary.WarningRate * 100):0}%)");
        Console.WriteLine($"BestRisk: {summary.BestRisk}");
        Console.WriteLine($"Best: {summary.BestShort}");
        Console.WriteLine($"ContextHealth: {summary.ContextHealth} | AvgDelta {summary.ContextAvgDelta:0.0} | Penalty {(summary.ContextPenaltyRate * 100):0}% | Warn {(summary.ContextWarningRate * 100):0}%");
        var best = summary.Best;
        if (best == null)
        {
            Console.WriteLine($"BestContextHealth: {summary.BestContextHealth}");
            Console.WriteLine("BestContext: (none)");
        }
        else
        {
            Console.WriteLine($"BestContextHealth: {summary.BestContextHealth} (Delta {best.ContextDelta}, Warn {best.ContextWarningCodes.Count})");
            Console.WriteLine($"BestContext: Delta {best.ContextDelta} | WarnCodes {best.ContextWarningCodes.Count}");
        }
        if (summary.SuggestReviewAlternatives)
            Console.WriteLine($"Suggestion: Review alternatives — {summary.SuggestReviewAlternativesReason}");

        //if (summary.Alternatives != null && summary.Alternatives.Count > 0)
        //{
        //    Console.WriteLine("Alternative reasons:");
        //    foreach (var alt in summary.Alternatives)
        //    {
        //        if (alt.AlternativeReasons.Count == 0) continue;
        //        Console.WriteLine($"- {alt.Candidate.Signature}: {alt.AlternativeReasons[0]}");
        //    }
        //}

        if (summary.Alternatives != null && summary.Alternatives.Count > 0)
        {
            Console.WriteLine("Alternative reasons:");
            foreach (var alt in summary.Alternatives)
            {
                if (alt.AlternativeReasonCodes == null || alt.AlternativeReasonCodes.Count == 0)
                    continue;

                var firstCode = alt.AlternativeReasonCodes[0];

                if (AlternativeMessageCatalog.TryGet(firstCode, out var msg))
                    Console.WriteLine($"- {alt.Candidate.Signature}: {msg.TitleTr}");
                else
                    Console.WriteLine($"- {alt.Candidate.Signature}: {firstCode}");
            }
        }

        // W1 - Wardrobe Feedback
        var ux = Kombinle.Core.Scoring.Presenting.WardrobeFeedbackPresenter.BuildUx(summary.WardrobeFeedback);
        if (ux != null)
        {
            Console.WriteLine("Wardrobe feedback:");
            Console.WriteLine("- " + ux.LineTr);

            // İstersen detayı da göster (V1: gösterelim, sonra UI’da gizlenebilir)
            if (!string.IsNullOrWhiteSpace(ux.DetailTr))
                Console.WriteLine("  " + ux.DetailTr);
        }

        if (summary.AlternativeShort.Count > 0)
        {
            Console.WriteLine("Alternatives:");
            foreach (var a in summary.AlternativeShort)
                Console.WriteLine($"- {a}");
        }

        if (summary.BestWhy.Count > 0)
        {
            Console.WriteLine("Why:");
            foreach (var w in summary.BestWhy)
                Console.WriteLine($"- {w}");
        }

        if (summary.BestRiskNotes.Count > 0)
        {
            Console.WriteLine("Risk:");
            foreach (var n in summary.BestRiskNotes)
                Console.WriteLine($"- {n}");
        }
        if (summary.WardrobeFeedback.Count > 0)
        {
            Console.WriteLine("Wardrobe feedback:");
            foreach (var f in summary.WardrobeFeedback)
                Console.WriteLine($"- {f.Message}");
        }


        Console.WriteLine();
    }
    //static void PrintRanking(List<ScoredCombination> ranked)
    //{
    //    Console.WriteLine("=== RANKING ===");
    //    foreach (var s in ranked)
    //    {
    //        Console.WriteLine($"{s.Score} pts (TB:{s.TieBreakScore}) | {s.Candidate.Strategy} | Anchor: {s.Candidate.Anchor.ColorFamily} | Fail: {s.FailReasons.Count}");

    //        foreach (var tb in s.TieBreakdown)
    //            Console.WriteLine($"   TB {tb.Value:+#;-#;0}  {tb.Reason}");
    //    }
    //    Console.WriteLine();
    //}
    //static void PrintBest(ScoredCombination? best, List<ScoredCombination> alternatives)
    //{
    //    Console.WriteLine("=== BEST ===");
    //    if (best == null)
    //    {
    //        Console.WriteLine("(no results)");
    //        return;
    //    }

    //    Console.WriteLine($"Best Score: {best.Score}");
    //    foreach (var b in best.Breakdown)
    //        Console.WriteLine($"{b.Value:+#;-#;0}  ␦ {b.Reason}");

    //    Console.WriteLine();
    //    Console.WriteLine("=== TIEBREAK ===");
    //    foreach (var tb in best.TieBreakdown)
    //        Console.WriteLine($"{tb.Value:+#;-#;0}  ␦ {tb.Reason}");

    //    if (alternatives.Count > 0)
    //    {
    //        Console.WriteLine();
    //        Console.WriteLine("=== ALTERNATIVES ===");
    //        foreach (var a in alternatives)
    //        {
    //            Console.WriteLine($"{a.Score} pts (TB:{a.TieBreakScore}) | {a.Candidate.Strategy} | Anchor: {a.Candidate.Anchor.ColorFamily} | Fail: {a.FailReasons.Count}");
    //        }
    //    }
    //    Console.WriteLine();
    //}

    // ----------------------------
    // User helper
    // ----------------------------
    static UserProfile BuildUserProfile(bool useFavoriteColor, ColorFamily favorite)
    {
        var user = new UserProfile();

        if (useFavoriteColor)
        {
            user.FavoriteColors = new List<ColorFamily> { favorite };
        }

        return user;
    }
}
