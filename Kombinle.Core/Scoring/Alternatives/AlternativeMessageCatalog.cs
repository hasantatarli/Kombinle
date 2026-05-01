using System;
using System.Collections.Generic;
using System.IO;

namespace Kombinle.Core.Scoring.Alternatives;

public static class AlternativeMessageCatalog
{
    private static readonly Dictionary<string, AlternativeMessage> _map = BuildMap();

    private static Dictionary<string, AlternativeMessage> BuildMap()
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Resources", "alternative_messages.json");

        if (!File.Exists(path))
            throw new FileNotFoundException("alternative_messages.json not found.", path);

        var loaded = AlternativeMessageCatalogLoader.LoadFromJsonFile(path);

        if (loaded.Count == 0)
            throw new InvalidOperationException("Alternative message catalog is empty.");

        return loaded;
    }

    public static bool TryGet(string code, out AlternativeMessage message)
        => _map.TryGetValue(code, out message!);
}