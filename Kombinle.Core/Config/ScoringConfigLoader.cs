using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kombinle.Core.Config
{
    public static class ScoringConfigLoader
    {
        public static ScoringConfig LoadFromJsonFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is required.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Scoring config json file not found.", filePath);

            var json = File.ReadAllText(filePath);

            var config = JsonSerializer.Deserialize<ScoringConfig>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (config == null)
                throw new InvalidOperationException("Failed to deserialize scoring config.");

            return config;
        }
    }
}
