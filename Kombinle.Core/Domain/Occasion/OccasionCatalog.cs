using Kombinle.Core.Domain;

namespace Kombinle.Core.Domain.Occasions
{

    public static class OccasionCatalog
    {
        private static readonly Dictionary<string, Occasion> _map = BuildMap();

        private static Dictionary<string, Occasion> BuildMap()
        {
            // 1) JSON'dan dene
            try
            {
                var baseDir = AppContext.BaseDirectory;
                var path = Path.Combine(baseDir, "Resources", "occasions.json");

                if (File.Exists(path))
                {
                    var loaded = OccasionCatalogLoader.LoadFromJsonFile(path);
                    if (loaded.Count > 0) return loaded;
                }
            }
            catch
            {
                // fallback'e düş
            }

            // 2) Fallback (mevcut factory’ler)
            return new Dictionary<string, Occasion>(StringComparer.OrdinalIgnoreCase)
            {
                ["business_meeting_formal"] = Occasion.BusinessMeeting_Formal(),
                ["casual_weekend"] = Occasion.CasualWeekend(),
                ["interview_formal"] = Occasion.Interview_Formal()
            };
        }

        public static bool TryGet(string occasionId, out Occasion occasion)
            => _map.TryGetValue(occasionId, out occasion!);
    }
}