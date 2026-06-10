using System.Text.Json;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;

namespace Kombinle.Core.Domain.Occasions
{

    internal static class OccasionCatalogLoader
    {
        private sealed class OccasionDto
        {
            public string? Name { get; set; }
            public string? RequiredFormality { get; set; }
            public List<string>? PreferredAnchorColors { get; set; }
            public ContextDto? DefaultContext { get; set; }
            public SlotSetDto? SlotSet { get; set; }
            public List<string>? CombinationModes { get; set; }

        }

        private sealed class ContextDto
        {
            public string? Weather { get; set; }
            public string? Setting { get; set; }
            public string? TimeOfDay { get; set; }
        }

        private sealed class SlotSetDto
        {
            public List<SlotRequirementDto>? Requirements { get; set; }
        }

        private sealed class SlotRequirementDto
        {
            public string? Slot { get; set; }
            public string? Level { get; set; }
            public List<string>? AllowedCategories { get; set; }
            public List<string>? AllowedTraits { get; set; }
            public List<string>? AllowedSlots { get; set; }
        }

        public static Dictionary<string, Occasion> LoadFromJsonFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is required.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Occasions json file not found.", filePath);

            var json = File.ReadAllText(filePath);

            var dtoMap = JsonSerializer.Deserialize<Dictionary<string, OccasionDto>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (dtoMap == null)
                throw new InvalidOperationException("Failed to deserialize occasions.");

            var result = new Dictionary<string, Occasion>(StringComparer.OrdinalIgnoreCase);

            foreach (var (id, dto) in dtoMap)
            {
                if (dto == null)
                    throw new InvalidOperationException($"Occasion '{id}' is null.");

                var occ = MapOccasion(id, dto);
                result[id] = occ;
            }

            return result;
        }

        private static Occasion MapOccasion(string id, OccasionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException($"Occasion '{id}' is missing name.");

            var requiredFormality = ParseEnumOrThrow<Formality>(dto.RequiredFormality, $"Occasion '{id}' requiredFormality");

            var slotSet = MapSlotSet(id, dto.SlotSet);

            var preferredColors = (dto.PreferredAnchorColors ?? new List<string>())
                .Select(x => ParseEnumOrThrow<ColorFamily>(x, $"Occasion '{id}' preferredAnchorColors"))
                .ToList();

            var defaultContext = dto.DefaultContext == null
                ? new ContextInput(Weather.Clear, Setting.Indoor, TimeOfDay.Day) // safe default
                : new ContextInput(
                    Weather: ParseEnumOrThrow<Weather>(dto.DefaultContext.Weather, $"Occasion '{id}' defaultContext.weather"),
                    Setting: ParseEnumOrThrow<Setting>(dto.DefaultContext.Setting, $"Occasion '{id}' defaultContext.setting"),
                    Time: ParseEnumOrThrow<TimeOfDay>(dto.DefaultContext.TimeOfDay, $"Occasion '{id}' defaultContext.timeOfDay")
                );

            return new Occasion
            {
                Id = id,
                Name = dto.Name!,
                RequiredFormality = requiredFormality,
                SlotSet = slotSet,
                PreferredAnchorColors = preferredColors,
                DefaultContext = defaultContext,
                CombinationModes = dto.CombinationModes ?? new List<string>()
            };
        }

        private static SlotSet MapSlotSet(string id, SlotSetDto? dto)
        {
            if (dto?.Requirements == null || dto.Requirements.Count == 0)
                throw new InvalidOperationException($"Occasion '{id}' slotSet.requirements is missing.");

            var reqs = new List<SlotRequirement>();

            foreach (var r in dto.Requirements)
            {
                if (r == null) continue;

                var slot = ParseEnumOrThrow<Slot>(r.Slot, $"Occasion '{id}' slotSet.requirements.slot");
                var level = ParseEnumOrThrow<RequirementLevel>(r.Level, $"Occasion '{id}' slotSet.requirements.level");

                var allowed = (r.AllowedCategories ?? new List<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList();

                var allowedTraits = r.AllowedTraits ?? new List<string>();
                var allowedSlots = (r.AllowedSlots ?? new List<string>())
                    .Select(x => ParseEnumOrThrow<Slot>(x, $"Occasion '{id}' slotSet.requirements.allowedSlots"))
                    .ToList();

                //if (allowed.Count == 0 && allowedTraits.Count == 0)
                //{
                //    throw new InvalidOperationException(
                //        $"Occasion '{id}' slot '{slot}' has no allowedCategories or allowedTraits.");
                //}

                if (allowed.Count == 0 &&
                    allowedTraits.Count == 0 &&
                    allowedSlots.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Occasion '{id}' slot '{slot}' has no allowedCategories, allowedTraits or allowedSlots.");
                }

                reqs.Add(new SlotRequirement
                {
                    Slot = slot,
                    Level = level,
                    AllowedCategories = allowed,
                    AllowedTraits = allowedTraits,
                    AllowedSlots = allowedSlots
                });
            }

            return new SlotSet { Requirements = reqs };
        }

        private static T ParseEnumOrThrow<T>(string? value, string label) where T : struct
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"{label} is missing.");

            if (Enum.TryParse<T>(value.Trim(), ignoreCase: true, out var parsed))
                return parsed;

            throw new InvalidOperationException($"{label} has invalid value '{value}'.");
        }
    }
}