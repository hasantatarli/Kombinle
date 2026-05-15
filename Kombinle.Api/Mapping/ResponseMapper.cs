using Kombinle.Api.Contracts;
using Kombinle.Core.Domain;
using Kombinle.Core.Generation;
using Kombinle.Core.Scoring;
using Kombinle.Core.Scoring.Alternatives;
using Kombinle.Core.Scoring.Context;
using Kombinle.Core.Scoring.Presenting;
using System;

namespace Kombinle.Api.Mapping;

public static class ResponseMapper
{
    public static DecisionResponse ToResponse(DecisionSummary summary)
    {
        if (summary.Best == null)
            return CreateNoBestResponse(summary);

        var decision = CreateDecisionCard(summary);
        var wardrobeFeedback = MapWardrobeFeedback(summary);

        var alternativeViews = CreateAlternativeViews(summary);
        var recommendedAlternative = alternativeViews.Recommended;
        var alternatives = alternativeViews.Others;

        return new DecisionResponse(
            ScenarioTitle: summary.ScenarioTitle,
            Decision: decision,
            RecommendedAlternative: recommendedAlternative,
            WardrobeFeedback: wardrobeFeedback,
            WardrobeGaps: MapWardrobeGaps(summary),
            Alternatives: alternatives,
            Debug: MapDebug(summary)

        );
    }

    private static List<OutfitItemDto> MapOutfitItems(ScoredCombination best)
    {
        var items = new List<OutfitItemDto>();
        var addedGarments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static string BuildGarmentKey(Garment garment)
        {
            return $"{garment.Category}:{garment.ColorFamily}:{garment.Formality}";
        }

        void AddItem(string slotName, Garment garment)
        {
            var garmentKey = BuildGarmentKey(garment);

            if (!addedGarments.Add(garmentKey))
                return;

            items.Add(new OutfitItemDto(
                Slot: slotName,
                Category: garment.Category.ToString(),
                ColorFamily: garment.ColorFamily.ToString()
            ));
        }

        if (best.Candidate.Anchor != null)
        {
            AddItem("Anchor", best.Candidate.Anchor);
        }

        foreach (var kv in best.Candidate.SlotToItem.OrderBy(x => GetSlotOrder(x.Key)))
        {
            AddItem(kv.Key.ToString(), kv.Value);
        }

        return items;
    }

    private static bool IsLayerItem(string slotName, Garment garment)
    {
        return slotName.Equals("Outerwear", StringComparison.OrdinalIgnoreCase)
               || (slotName.Equals("Anchor", StringComparison.OrdinalIgnoreCase)
                   && garment.Category == Category.Jacket);
    }

    private static (List<OutfitItemDto> CoreItems, List<OutfitItemDto> Layers) MapOutfitSections(ScoredCombination best)
    {
        var coreItems = new List<OutfitItemDto>();
        var layers = new List<OutfitItemDto>();
        var addedGarments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static string BuildGarmentKey(Garment garment)
        {
            return $"{garment.Category}:{garment.ColorFamily}:{garment.Formality}";
        }

        void AddItem(string slotName, Garment garment)
        {
            var garmentKey = BuildGarmentKey(garment);

            if (!addedGarments.Add(garmentKey))
                return;

            var dto = new OutfitItemDto(
                Slot: slotName,
                Category: garment.Category.ToString(),
                ColorFamily: garment.ColorFamily.ToString()
            );

            if (IsLayerItem(slotName, garment))
                layers.Add(dto);
            else
                coreItems.Add(dto);
        }

        if (best.Candidate.Anchor != null)
        {
            AddItem("Anchor", best.Candidate.Anchor);
        }

        foreach (var kv in best.Candidate.SlotToItem.OrderBy(x => GetSlotOrder(x.Key)))
        {
            AddItem(kv.Key.ToString(), kv.Value);
        }

        return (coreItems, layers);
    }

    private static int GetSlotOrder(Slot slot)
    {
        return slot switch
        {
            Slot.Top => 1,
            Slot.Bottom => 2,
            Slot.Shoes => 3,
            Slot.Outerwear => 4,
            _ => 100
        };
    }

    private static WardrobeFeedbackCardDto? MapWardrobeFeedback(DecisionSummary summary)
    {
        if (summary.WardrobeFeedback == null || summary.WardrobeFeedback.Count == 0)
            return null;

        var ux = WardrobeFeedbackPresenter.BuildUx(summary.WardrobeFeedback);
        if (ux == null)
            return null;

        var code = ResolveWardrobeFeedbackCode(summary.WardrobeFeedback);

        return new WardrobeFeedbackCardDto(
            LineTr: ux.LineTr,
            DetailTr: ux.DetailTr,
            Code: code
        );
    }

    private static string ResolveWardrobeFeedbackCode(IReadOnlyList<WardrobeFeedback> feedback)
    {
        var codes = feedback
            .Where(x => !string.IsNullOrWhiteSpace(x.ContextWarningCode))
            .Select(x => x.ContextWarningCode)
            .ToList();

        var rainCode = codes.FirstOrDefault(x =>
            x.StartsWith("RAIN_", StringComparison.OrdinalIgnoreCase));

        if (rainCode != null)
            return rainCode;

        var softCode = codes.FirstOrDefault(x =>
            x.StartsWith("SOFT_", StringComparison.OrdinalIgnoreCase));

        if (softCode != null)
            return softCode;

        var outdoorCode = codes.FirstOrDefault(x =>
            x.StartsWith("OUTDOOR_", StringComparison.OrdinalIgnoreCase));

        if (outdoorCode != null)
            return outdoorCode;

        return codes.FirstOrDefault() ?? feedback[0].ContextWarningCode;
    }


    //private static List<AlternativeCardDto> MapAlternatives(DecisionSummary summary)
    //{
    //    var result = new List<AlternativeCardDto>();
    //    var alternatives = summary.Alternatives ?? new List<ScoredCombination>();

    //    var startIndex = ShouldRecommendAlternative(summary) ? 1 : 0;

    //    for (var i = startIndex; i < alternatives.Count; i++)
    //    {
    //        var alt = alternatives[i];

    //        var shortText =
    //            summary.AlternativeShort != null && i < summary.AlternativeShort.Count
    //                ? summary.AlternativeShort[i]
    //                : alt.Candidate.Signature;

    //        result.Add(new AlternativeCardDto(
    //            ShortTr: shortText,
    //            Reasons: MapAlternativeReasons(alt)
    //        ));
    //    }

    //    return result;
    //}

    private static List<AlternativeReasonDto> MapAlternativeReasons(ScoredCombination alt)
    {
        var result = new List<AlternativeReasonDto>();

        if (alt.AlternativeReasonCodes == null || alt.AlternativeReasonCodes.Count == 0)
            return result;

        var orderedCodes = alt.AlternativeReasonCodes
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code =>
            {
                if (AlternativeMessageCatalog.TryGet(code, out var msg))
                    return msg.Priority;

                return 1000;
            })
            .ToList();

        var selectedCodes = new List<string>();
        var seenGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var code in orderedCodes)
        {
            if (AlternativeMessageCatalog.TryGet(code, out var msg))
            {
                var groupKey = string.IsNullOrWhiteSpace(msg.Group)
                    ? code
                    : msg.Group!;

                if (!seenGroups.Add(groupKey))
                    continue;
            }

            selectedCodes.Add(code);

            if (selectedCodes.Count >= 2)
                break;
        }

        foreach (var code in selectedCodes)
        {
            if (AlternativeMessageCatalog.TryGet(code, out var msg))
            {
                result.Add(new AlternativeReasonDto(
                    Code: code,
                    TitleTr: msg.TitleTr,
                    DetailTr: string.IsNullOrWhiteSpace(msg.DetailTr) ? null : msg.DetailTr
                ));
            }
            else
            {
                result.Add(new AlternativeReasonDto(
                    Code: code,
                    TitleTr: code,
                    DetailTr: null
                ));
            }
        }

        return result;
    }

    private static string ResolveDecisionMessageCode(DecisionSummary summary)
    {
        if (summary.Best == null)
            return "DECISION_NO_BEST";

        var hasAlternatives = summary.Alternatives != null && summary.Alternatives.Count > 0;
        var isWarning = summary.BestContextHealth == ContextHealthLevel.Poor;

        if (!isWarning)
        {
            return summary.OccasionRequiredFormality switch
            {
                Formality.Casual => "DECISION_SAFE_CASUAL",
                Formality.Smart => "DECISION_SAFE_SMART",
                Formality.Formal => "DECISION_SAFE_FORMAL",
                _ => "DECISION_SAFE"
            };
        }

        var topWarningCode =
            summary.WardrobeFeedback?.FirstOrDefault()?.ContextWarningCode
            ?? summary.Best?.ContextWarningCodes?.FirstOrDefault();

        var isSoftWarning =
            !string.IsNullOrWhiteSpace(topWarningCode) &&
            topWarningCode.StartsWith("SOFT_", StringComparison.OrdinalIgnoreCase);

        if (isSoftWarning)
        {

            return hasAlternatives
                ? "DECISION_SOFT_WARNING_WITH_ALTERNATIVES"
                : "DECISION_SOFT_WARNING_NO_ALTERNATIVES";
        }

        return hasAlternatives
            ? "DECISION_WARNING_WITH_ALTERNATIVES"
            : "DECISION_WARNING_NO_ALTERNATIVES";
    }

    private static string? ResolveDecisionSubtextTr(DecisionSummary summary, string decisionMessageCode)
    {
        if (DecisionMessageCatalog.TryGet(decisionMessageCode, out var baseMsg) == false)
            return null;

        if (summary.Best != null && summary.BestContextHealth != ContextHealthLevel.Poor)
            return baseMsg.SubtextTr;

        var hasAlternatives = summary.Alternatives != null && summary.Alternatives.Count > 0;

        var topWarningCode =
            summary.WardrobeFeedback?.FirstOrDefault()?.ContextWarningCode
            ?? summary.Best?.ContextWarningCodes?.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(topWarningCode) &&
            ContextMessageCatalog.TryGet(topWarningCode, out var ctxMsg))
        {
            if (hasAlternatives)
                return $"{ctxMsg.TitleTr}; bu nedenle daha güvenli alternatifler önerdim.";

            return $"{ctxMsg.TitleTr}; eklersen kombin daha derli toplu görünebilir.";
        }

        return baseMsg.SubtextTr;
    }

    private static bool ShouldRecommendAlternative(DecisionSummary summary)
    {
        if (summary.Best == null)
            return false;

        if (summary.Alternatives == null || summary.Alternatives.Count == 0)
            return false;

        return summary.BestContextHealth == ContextHealthLevel.Poor;
    }

    //private static AlternativeCardDto? MapRecommendedAlternative(DecisionSummary summary)
    //{
    //    if (!ShouldRecommendAlternative(summary))
    //        return null;

    //    var first = summary.Alternatives!.FirstOrDefault();
    //    if (first == null)
    //        return null;

    //    var shortText =
    //        summary.AlternativeShort != null && summary.AlternativeShort.Count > 0
    //            ? summary.AlternativeShort[0]
    //            : first.Candidate.Signature;

    //    return new AlternativeCardDto(
    //        ShortTr: shortText,
    //        Reasons: MapAlternativeReasons(first)
    //    );
    //}
    private static DebugDto MapDebug(DecisionSummary summary)
    {
        return new DebugDto(
            GeneratedCount: summary.GeneratedCount,
            RankedCount: summary.RankedCount,
            ContextAvgDelta: summary.ContextAvgDelta,
            ContextPenaltyRate: summary.ContextPenaltyRate,
            ContextWarningRate: summary.ContextWarningRate
        );
    }

    private static DecisionResponse CreateNoBestResponse(DecisionSummary summary)
    {
        var headlineTr = "Şu an öneri üretemedim.";
        string? subtextTr = "Renk ve stil uyumu açısından güvenle önerebileceğim bir kombin çıkmadı.";
        var outfitShortTr = "Bu senaryo için uygun bir kombin çıkaramadım.";

        if (DecisionMessageCatalog.TryGet("DECISION_NO_BEST", out var noDecisionMessage))
        {
            headlineTr = noDecisionMessage.HeadlineTr;
            subtextTr = noDecisionMessage.SubtextTr;
        }

        return new DecisionResponse(
            ScenarioTitle: summary.ScenarioTitle,
            Decision: new DecisionCardDto(
                HeadlineTr: headlineTr,
                SubtextTr: subtextTr,
               Outfit: new OutfitDto(
                    Items: new List<OutfitItemDto>(),
                    CoreItems: new List<OutfitItemDto>(),
                    Layers: new List<OutfitItemDto>(),
                    ShortTr: outfitShortTr
                ),
                BestContextHealth: "Unknown",
                WhyThisWorksTr: new List<string>(),
                ContextNotes: new List<ContextNoteDto>()
            ),
            RecommendedAlternative: null,
            WardrobeFeedback: null,
            WardrobeGaps: MapWardrobeGaps(summary),
            Alternatives: new List<AlternativeCardDto>(),
            Debug: MapDebug(summary)
        );
    }

    private static List<WardrobeGapDto> MapWardrobeGaps(DecisionSummary summary)
    {
        if (summary.WardrobeGaps == null || summary.WardrobeGaps.Count == 0)
            return new List<WardrobeGapDto>();

        return summary.WardrobeGaps
            .OrderBy(x => x.Priority)
            .Select(x => new WardrobeGapDto(
                Code: x.Code,
                Category: x.Category.ToString(),
                SuggestionType: x.SuggestionType.ToString(),
                Priority: x.Priority
            ))
            .ToList();
    }

    private static DecisionCardDto CreateDecisionCard(DecisionSummary summary)
    {
        var best = summary.Best!;
        var decisionMessageCode = ResolveDecisionMessageCode(summary);

        var headlineTr = "Karar üretildi.";
        if (DecisionMessageCatalog.TryGet(decisionMessageCode, out var decisionMessage))
        {
            headlineTr = decisionMessage.HeadlineTr;
        }

        var subtextTr = ResolveDecisionSubtextTr(summary, decisionMessageCode);
        var outfitSections = MapOutfitSections(best);

        return new DecisionCardDto(
            HeadlineTr: headlineTr,
            SubtextTr: subtextTr,
            Outfit: new OutfitDto(
                Items: MapOutfitItems(best),
                CoreItems: outfitSections.CoreItems,
                Layers: outfitSections.Layers,
                ShortTr: BuildShortFromCandidate(best.Candidate)
            ),
            BestContextHealth: summary.BestContextHealth.ToString(),
            WhyThisWorksTr: BuildWhyThisWorks(best),
            ContextNotes: MapContextNotes(best)
        );
    }

    private static (AlternativeCardDto? Recommended, List<AlternativeCardDto> Others) CreateAlternativeViews(DecisionSummary summary)
    {
        var result = new List<AlternativeCardDto>();
        var alternatives = summary.Alternatives ?? new List<ScoredCombination>();

        if (alternatives.Count == 0)
            return (null, result);

        AlternativeCardDto? recommended = null;
        var startIndex = 0;

        if (ShouldRecommendAlternative(summary))
        {
            recommended = MapAlternativeCard(summary, alternatives[0], 0);
            startIndex = 1;
        }

        for (var i = startIndex; i < alternatives.Count; i++)
        {
            result.Add(MapAlternativeCard(summary, alternatives[i], i));
        }

        return (recommended, result);
    }

    private static AlternativeCardDto MapAlternativeCard(DecisionSummary summary, ScoredCombination alt, int index)
    {
        return new AlternativeCardDto(
            ShortTr: ResolveAlternativeShortText(summary, alt, index),
            Reasons: MapAlternativeReasons(alt)
        );
    }

    private static string ResolveAlternativeShortText(DecisionSummary summary, ScoredCombination alt, int index)
    {
        var generated = BuildShortFromCandidate(alt.Candidate);

        if (!string.IsNullOrWhiteSpace(generated))
            return generated;

        if (summary.AlternativeShort != null && index < summary.AlternativeShort.Count)
            return summary.AlternativeShort[index];

        return alt.Candidate.Signature;
    }

    private static string BuildShortFromCandidate(CombinationCandidate candidate)
    {
        var parts = new List<string>();

        if (candidate.Anchor != null)
        {
            parts.Add($"{candidate.Anchor.ColorFamily} {candidate.Anchor.Category}");
        }

        foreach (var kv in candidate.SlotToItem.OrderBy(x => GetSlotOrder(x.Key)))
        {
            if (kv.Key == Slot.Anchor)
                continue;

            var g = kv.Value;
            if (g == null) continue;

            parts.Add($"{g.ColorFamily} {g.Category}");
        }

        return string.Join(" + ", parts);
    }

    private static List<string> BuildWhyThisWorks(ScoredCombination best)
    {
        var result = new List<string>();

        // Önce context (en kritik)
        if (best.ContextWarningCodes == null || best.ContextWarningCodes.Count == 0)
        {
            result.Add("Koşullar açısından güvenli görünüyor.");
        }

        // sonra formality
        if (best.Breakdown.Any(x =>
                x.Reason.StartsWith("Formality:", StringComparison.OrdinalIgnoreCase) &&
                x.Value > 0))
        {
            result.Add("Formallik seviyesi ortama uygun.");
        }

        // sonra color
        if (best.Breakdown.Any(x =>
                x.Reason.StartsWith("Renk uyumu:", StringComparison.OrdinalIgnoreCase) &&
                x.Value > 0))
        {
            result.Add("Renkler birbiriyle uyumlu.");
        }

        return result
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();
    }

    private static List<ContextNoteDto> MapContextNotes(ScoredCombination? best)
    {
        if (best?.ContextUserNotes == null || best.ContextUserNotes.Count == 0)
            return new List<ContextNoteDto>();

        return best.ContextUserNotes
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Text) && !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => x.Code!.Trim())
            .Select(g => g.First())
            .Select(x => new ContextNoteDto(x.Code!.Trim(), x.Text.Trim()))
            .ToList();
    }
}