using Kombinle.Core.Domain;

namespace Kombinle.Core.Domain.Occasions
{

    public static class OccasionCatalog
    {
        private static readonly Dictionary<string, Occasion> _map = BuildMap();

        private static Dictionary<string, Occasion> BuildMap()
        {
            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "Resources", "occasions.json");

            if (!File.Exists(path))
                throw new FileNotFoundException("Occasions catalog file not found.", path);

            var loaded = OccasionCatalogLoader.LoadFromJsonFile(path);

            if (loaded.Count == 0)
                throw new InvalidOperationException("Occasion catalog is empty.");

            return loaded;
        }

        public static bool TryGet(string occasionId, out Occasion occasion)
            => _map.TryGetValue(occasionId, out occasion!);

        public static IReadOnlyDictionary<string, Occasion> All() 
            => _map;
    }
}