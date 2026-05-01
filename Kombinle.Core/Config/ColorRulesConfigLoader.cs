using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kombinle.Core.Config
{
    public static class ColorRulesConfigLoader
    {
        private sealed class ColorRulesConfigDto
        {
            public List<ColorPairRuleDto>? StrongPairs { get; set; }
            public List<ColorPairRuleDto>? WeakPairs { get; set; }
            public List<ColorPairRuleDto>? ClashPairs { get; set; }
            public List<string>? NeutralColors { get; set; }
            public List<string>? BrightColors { get; set; }
        }

        private sealed class ColorPairRuleDto
        {
            public string? A { get; set; }
            public string? B { get; set; }
        }

        public static ColorRulesConfig LoadFromJsonFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is required.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Color rules json file not found.", filePath);

            var json = File.ReadAllText(filePath);

            var dto = JsonSerializer.Deserialize<ColorRulesConfigDto>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto == null)
                throw new InvalidOperationException("Failed to deserialize color rules.");

            var config = new ColorRulesConfig
            {
                StrongPairs = MapPairs(dto.StrongPairs, "strongPairs"),
                WeakPairs = MapPairs(dto.WeakPairs, "weakPairs"),
                ClashPairs = MapPairs(dto.ClashPairs, "clashPairs")
            };

            foreach (var pair in dto.ClashPairs ?? new List<ColorPairRuleDto>())
            {
                if (string.IsNullOrWhiteSpace(pair.A) || string.IsNullOrWhiteSpace(pair.B))
                    throw new InvalidOperationException("Color clash pair is missing color fields.");

                if (!Enum.TryParse<ColorFamily>(pair.A, ignoreCase: true, out var colorA))
                    throw new InvalidOperationException($"Unknown color family in clash pair: '{pair.A}'.");

                if (!Enum.TryParse<ColorFamily>(pair.B, ignoreCase: true, out var colorB))
                    throw new InvalidOperationException($"Unknown color family in clash pair: '{pair.B}'.");

                config.ClashPairs.Add(new ColorPairRule
                {
                    A = colorA,
                    B = colorB
                });
            }

            foreach (var color in dto.NeutralColors ?? new List<string>())
            {
                if (!Enum.TryParse<ColorFamily>(color, ignoreCase: true, out var parsed))
                    throw new InvalidOperationException($"Unknown neutral color family: '{color}'.");

                config.NeutralColors.Add(parsed);
            }

            foreach (var color in dto.BrightColors ?? new List<string>())
            {
                if (!Enum.TryParse<ColorFamily>(color, ignoreCase: true, out var parsed))
                    throw new InvalidOperationException($"Unknown bright color family: '{color}'.");

                config.BrightColors.Add(parsed);
            }

            return config;
        }

        private static List<ColorPairRule> MapPairs(
            List<ColorPairRuleDto>? pairs,
            string ruleName)
        {
            var result = new List<ColorPairRule>();

            foreach (var pair in pairs ?? new List<ColorPairRuleDto>())
            {
                if (string.IsNullOrWhiteSpace(pair.A) || string.IsNullOrWhiteSpace(pair.B))
                    throw new InvalidOperationException($"{ruleName} color pair is missing color fields.");

                if (!Enum.TryParse<ColorFamily>(pair.A, ignoreCase: true, out var colorA))
                    throw new InvalidOperationException($"Unknown color family in {ruleName}: '{pair.A}'.");

                if (!Enum.TryParse<ColorFamily>(pair.B, ignoreCase: true, out var colorB))
                    throw new InvalidOperationException($"Unknown color family in {ruleName}: '{pair.B}'.");

                result.Add(new ColorPairRule
                {
                    A = colorA,
                    B = colorB
                });
            }

            return result;
        }
    }
}
