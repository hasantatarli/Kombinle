using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Occasions;
using System;

namespace Kombinle.Api.Mapping;

public static class OccasionResolver
{
    //public static Occasion Resolve(string occasionId)
    //    => occasionId switch
    //    {
    //        "business_meeting_formal" => Occasion.BusinessMeeting_Formal(),
    //        "casual_weekend" => Occasion.CasualWeekend(),
    //        "interview_formal" => Occasion.Interview_Formal(),
    //        _ => throw new NotSupportedException($"OccasionId '{occasionId}' is not supported.")
    //    };

    public static Occasion Resolve(string occasionId)
    {
        if (string.IsNullOrWhiteSpace(occasionId))
            throw new ArgumentException("occasionId is required.", nameof(occasionId));

        if (OccasionCatalog.TryGet(occasionId, out var occasion))
            return occasion;

        throw new NotSupportedException($"OccasionId '{occasionId}' is not supported.");
    }
}
