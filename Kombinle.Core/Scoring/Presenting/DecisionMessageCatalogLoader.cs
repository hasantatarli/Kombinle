using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring.Presenting
{
    internal static class DecisionMessageCatalogLoader
    {
        private sealed class DecisionMessageDto
        {
            public string? HeadlineTr { get; set; }
            public string? HeadlineEn { get; set; }
            public string? SubtextTr { get; set; }
            public string? SubtextEn { get; set; }
            public int? Priority { get; set; }
        }

        public static Dictionary<string, DecisionMessage> LoadFromJsonFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is required.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Decision messages json file not found.", filePath);

            var json = File.ReadAllText(filePath);

            var dtoMap = JsonSerializer.Deserialize<Dictionary<string, DecisionMessageDto>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dtoMap == null)
                throw new InvalidOperationException("Failed to deserialize decision messages.");

            var result = new Dictionary<string, DecisionMessage>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in dtoMap)
            {
                var code = kv.Key;
                var dto = kv.Value ?? new DecisionMessageDto();

                if (string.IsNullOrWhiteSpace(dto.HeadlineTr) || string.IsNullOrWhiteSpace(dto.HeadlineEn))
                    throw new InvalidOperationException($"Decision message '{code}' is missing headline fields.");

                result[code] = new DecisionMessage(
                    HeadlineTr: dto.HeadlineTr!,
                    HeadlineEn: dto.HeadlineEn!,
                    SubtextTr: dto.SubtextTr,
                    SubtextEn: dto.SubtextEn,
                    Priority: dto.Priority ?? 1000
                );
            }

            return result;
        }
    }
}