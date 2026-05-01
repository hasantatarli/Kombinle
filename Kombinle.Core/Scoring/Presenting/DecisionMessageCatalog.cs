using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring.Presenting
{
    public static class DecisionMessageCatalog
    {
        private static readonly Dictionary<string, DecisionMessage> _map = BuildMap();

        private static Dictionary<string, DecisionMessage> BuildMap()
        {
            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "Resources", "decision_messages.json");

            if (!File.Exists(path))
                throw new FileNotFoundException("decision_messages.json not found.", path);

            var loaded = DecisionMessageCatalogLoader.LoadFromJsonFile(path);

            if (loaded.Count == 0)
                throw new InvalidOperationException("Decision message catalog is empty.");

            return loaded;
        }

        public static bool TryGet(string code, out DecisionMessage message)
            => _map.TryGetValue(code, out message!);
    }
}