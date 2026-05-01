using Kombinle.Api.Contracts;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Domain.Traits;
using System;

namespace Kombinle.Api.Mapping;

public static class MappingHelpers
{
    public static ContextInput ToContextInput(ContextDto dto)
        => new ContextInput(
            Weather: ParseEnum<Weather>(dto.Weather, nameof(dto.Weather)),
            Setting: ParseEnum<Setting>(dto.Setting, nameof(dto.Setting)),
            Time: ParseEnum<TimeOfDay>(dto.TimeOfDay, nameof(dto.TimeOfDay))
        );

    public static Garment ToGarment(GarmentInputDto dto)
    {
        var g = new Garment
        {
            Category = ParseEnum<Category>(dto.Category, nameof(dto.Category)),
            ColorFamily = ParseEnum<ColorFamily>(dto.ColorFamily, nameof(dto.ColorFamily)),
            Formality = ParseEnum<Formality>(dto.Formality, nameof(dto.Formality))
        };

        if (g.Category == Category.Shoes)
        {
            g.Shoe ??= new ShoeTraits();
            if (!string.IsNullOrWhiteSpace(dto.Shoe?.Material))
            {
                var mat = ParseEnum<ShoeMaterial>(dto.Shoe.Material!, "Shoe.Material");
                g.Shoe.Material = new TagValue<ShoeMaterial>(mat, TagSource.User, 1.0);
            }
        }

        return g;
    }

    public static ColorFamily ParseColorFamily(string s) => ParseEnum<ColorFamily>(s, "FavoriteColors");

    private static T ParseEnum<T>(string value, string field) where T : struct
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{field} is required.");

        if (!Enum.TryParse<T>(value, ignoreCase: true, out var parsed))
            throw new ArgumentException($"{field} '{value}' is not supported.");

        return parsed;
    }
}
