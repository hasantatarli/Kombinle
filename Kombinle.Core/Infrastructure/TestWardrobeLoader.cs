using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kombinle.Core.Infrastructure
{
    public static class TestWardrobeLoader
    {
        private static readonly Lazy<Dictionary<string, TestWardrobeProfile>> _cache =
            new(() =>
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Resources", "test_wardrobes.json");

                if (!File.Exists(path))
                    throw new FileNotFoundException($"Test wardrobe file not found: {path}");

                var json = File.ReadAllText(path);

                var data = JsonSerializer.Deserialize<Dictionary<string, TestWardrobeProfile>>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return data ?? new Dictionary<string, TestWardrobeProfile>();
            });

        public static List<Garment> Load(string profileId)
        {
            if (!_cache.Value.TryGetValue(profileId, out var profile))
                throw new Exception($"Wardrobe profile not found: {profileId}");

            return profile.Items.Select(x => new Garment
            {
                CategoryId = x.Category.Trim(),
                ColorFamily = Enum.Parse<ColorFamily>(x.ColorFamily, true),
                Formality = Enum.Parse<Formality>(x.Formality, true)
            }).ToList();
        }
    }

    //public static List<WardrobeProfileSummaryDto> ListProfiles()
    //    {
    //        return new List<WardrobeProfileSummaryDto>
    //{
    //    new("female_basic_v1", "Kadın Basic"),
    //    new("female_balanced_v1", "Kadın Balanced"),
    //    new("male_basic_v1", "Erkek Basic"),
    //    new("male_balanced_v1", "Erkek Balanced"),
    //    new("male_extended_v1", "Erkek Extended"),
    //    new("color_test_v1", "Color Test")
    //};
    //    }

    public class TestWardrobeProfile
    {
        public string Name { get; set; } = "";
        public List<TestGarmentDto> Items { get; set; } = new();
    }

    public class TestGarmentDto
    {
        public string Category { get; set; } = "";
        public string ColorFamily { get; set; } = "";
        public string Formality { get; set; } = "";
    }
}
