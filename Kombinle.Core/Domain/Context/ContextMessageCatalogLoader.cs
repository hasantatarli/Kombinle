using Kombinle.Core.Scoring.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Kombinle.Core.Domain.Context
{
    internal static class ContextMessageCatalogLoader
    {
        private sealed class ContextMessageDto
        {
            public string? Kind { get; set; }
            public string? TitleTr { get; set; }
            public string? TitleEn { get; set; }
            public string? DetailTr { get; set; }
            public string? DetailEn { get; set; }
        }

        public static Dictionary<string, ContextMessage> LoadFromJsonFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is required.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Context messages json file not found.", filePath);

            var json = File.ReadAllText(filePath);

            var dtoMap = JsonSerializer.Deserialize<Dictionary<string, ContextMessageDto>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (dtoMap == null)
                throw new InvalidOperationException("Failed to deserialize context messages.");

            var result = new Dictionary<string, ContextMessage>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in dtoMap)
            {
                var code = kv.Key;
                var d = kv.Value ?? new ContextMessageDto();

                var kind = ParseKindOrDefault(d.Kind);

                // Minimal validation
                if (string.IsNullOrWhiteSpace(d.TitleTr) || string.IsNullOrWhiteSpace(d.TitleEn))
                    throw new InvalidOperationException($"Context message '{code}' is missing title fields.");

                result[code] = new ContextMessage(
                    Kind: kind,
                    TitleTr: d.TitleTr!,
                    TitleEn: d.TitleEn!,
                    DetailTr: d.DetailTr ?? "",
                    DetailEn: d.DetailEn ?? ""
                );
            }

            return result;
        }


        private static MessageKind ParseKindOrDefault(string? kind)
        {
            if (string.IsNullOrWhiteSpace(kind))
                return MessageKind.ContextWarning; // default (geri uyum)

            return Enum.TryParse<MessageKind>(kind.Trim(), ignoreCase: true, out var parsed)
                ? parsed
                : MessageKind.ContextWarning;
        }
    }
}
