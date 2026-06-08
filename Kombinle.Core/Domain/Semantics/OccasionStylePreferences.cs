using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Semantics
{
    public static class OccasionStylePreferences
    {
        public static string? Get(string occasionId)
        {
            return occasionId switch
            {
                "business_meeting_formal"
                    => StyleTraits.BusinessAppropriate,

                "smart_casual_dinner"
                    => StyleTraits.SmartCasualAppropriate,

                "casual_weekend"
                    => StyleTraits.CasualAppropriate,

                _ => null
            };
        }
    }
}
