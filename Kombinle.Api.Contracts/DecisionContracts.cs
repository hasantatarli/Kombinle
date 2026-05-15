using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Api.Contracts
{
    public record DecisionRequest(
        string OccasionId,
        ContextDto? Context,
        List<GarmentInputDto>? Items,
        UserPrefsDto? User,
        string? WardrobeProfileId,
        int RotationAttempt = 0
    );
    public record ContextDto(
        string Weather,
        string Setting,
        string TimeOfDay,
        string? Season
    );

    public record GarmentInputDto(
        string TempId,
        string Category,
        string ColorFamily,
        string Formality,
        ShoeInputDto? Shoe
    );

    public record ShoeInputDto(string? Material);

    public record UserPrefsDto(List<string>? FavoriteColors);

    //public List<string>? WhyThisWorksTr { get; set; }

    public record DecisionResponse(
        string ScenarioTitle,
        DecisionCardDto Decision,
        AlternativeCardDto? RecommendedAlternative,
        WardrobeFeedbackCardDto? WardrobeFeedback,
        List<WardrobeGapDto> WardrobeGaps,
        List<AlternativeCardDto> Alternatives,
        DebugDto? Debug
    );



    public record WardrobeGapDto(
        string Code,
        string Category,
        string SuggestionType,
        int Priority
    );

    public record DecisionCardDto(
        string HeadlineTr,
        string? SubtextTr,
        OutfitDto Outfit,
        string BestContextHealth,
        List<string> WhyThisWorksTr,
        List<ContextNoteDto> ContextNotes
    );

    public record OutfitDto(
        List<OutfitItemDto> Items,
        List<OutfitItemDto> CoreItems,
        List<OutfitItemDto> Layers,
        string ShortTr
    );

    public record OutfitItemDto(
        string Slot,
        string Category,
        string ColorFamily
    );

    public record WardrobeFeedbackCardDto(
        string LineTr,
        string? DetailTr,
        string Code
    );

    public record AlternativeCardDto(
        string ShortTr,
        List<AlternativeReasonDto> Reasons
    );

    public sealed record AlternativeReasonDto(
        string Code,
        string TitleTr,
        string? DetailTr
    );

    public record DebugDto(
        int GeneratedCount,
        int RankedCount,
        double ContextAvgDelta,
        double ContextPenaltyRate,
        double ContextWarningRate
    );

    public record ContextNoteDto(
         string Code,
         string TextTr
    );

    public record ApiError(string Error, string Message);
}

