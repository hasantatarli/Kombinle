using System.Text.Json;
using Kombinle.Api.Models;

namespace Kombinle.Api.Services;

public class CategoryCatalogService
{
    private readonly string _path;

    public CategoryCatalogService(IWebHostEnvironment env)
    {
        _path = Path.Combine(
            env.ContentRootPath,
            "App_Data",
            "Catalogs",
            "category_catalog.json");
    }

    public List<CategoryCatalogItem> GetAll()
    {
        var json = File.ReadAllText(_path);

        return JsonSerializer.Deserialize<List<CategoryCatalogItem>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];
    }
}