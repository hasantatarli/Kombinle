using System.Text.Json;
using Kombinle.Api.Models;

namespace Kombinle.Api.Services;

public class WardrobeProfileService
{
    private readonly string _basePath;

    public WardrobeProfileService(IWebHostEnvironment env)
    {
        _basePath = Path.Combine(
            env.ContentRootPath,
            "App_Data",
            "Wardrobes");
    }

    public List<WardrobeProfile> GetProfiles()
    {
        var files = Directory.GetFiles(_basePath, "*.json");

        return files
            .Select(ReadProfile)
            .Where(x => x != null)
            .ToList()!;
    }

    public WardrobeProfile? GetProfile(string id)
    {
        var path = Path.Combine(_basePath, $"{id}.json");

        if (!File.Exists(path))
            return null;

        return ReadProfile(path);
    }

    private WardrobeProfile? ReadProfile(string path)
    {
        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<WardrobeProfile>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
}