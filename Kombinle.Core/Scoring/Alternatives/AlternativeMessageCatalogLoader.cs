using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Kombinle.Core.Scoring.Alternatives;

internal static class AlternativeMessageCatalogLoader
{
    private sealed class AlternativeMessageDto
    {
        public string? TitleTr { get; set; }
        public string? TitleEn { get; set; }
        public string? DetailTr { get; set; }
        public string? DetailEn { get; set; }
        public int? Priority { get; set; }
        public string? Group { get; set; }
    }

    public static Dictionary<string, AlternativeMessage> LoadFromJsonFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("filePath is required.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Alternative messages json file not found.", filePath);

        var json = File.ReadAllText(filePath);

        var dtoMap = JsonSerializer.Deserialize<Dictionary<string, AlternativeMessageDto>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (dtoMap == null)
            throw new InvalidOperationException("Failed to deserialize alternative messages.");

        var result = new Dictionary<string, AlternativeMessage>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in dtoMap)
        {
            var code = kv.Key;
            var dto = kv.Value ?? new AlternativeMessageDto();

            if (string.IsNullOrWhiteSpace(dto.TitleTr) || string.IsNullOrWhiteSpace(dto.TitleEn))
                throw new InvalidOperationException($"Alternative message '{code}' is missing title fields.");

            result[code] = new AlternativeMessage(
                TitleTr: dto.TitleTr!,
                TitleEn: dto.TitleEn!,
                DetailTr: dto.DetailTr ?? string.Empty,
                DetailEn: dto.DetailEn ?? string.Empty,
                Priority: dto.Priority ?? 1000,
                Group: dto.Group ?? string.Empty
            );
        }

        return result;
    }
}