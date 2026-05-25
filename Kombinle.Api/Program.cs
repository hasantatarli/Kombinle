//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.Run();

using Kombinle.Api.Contracts;
using Kombinle.Api.Services;
using Kombinle.Core.Domain;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// JSON options (camelCase default zaten var; explicit istersen)
builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// Core engine facade
builder.Services.AddSingleton<IDecisionService, DecisionService>();
builder.Services.AddHttpClient<WeatherContextService>();
builder.Services.AddSingleton<WardrobeProfileService>();
builder.Services.AddSingleton<CategoryCatalogService>();

var app = builder.Build();

var categoryCatalogService =
    app.Services.GetRequiredService<CategoryCatalogService>();

CategorySemantics.Provider =
    new JsonCategorySemanticsProvider(categoryCatalogService);

app.MapGet("/api/v1/weather/context",
    async (
        string? city,
        double? lat,
        double? lon,
        WeatherContextService weatherService,
        CancellationToken ct) =>
    {
        if (!string.IsNullOrWhiteSpace(city))
        {
            var byCity = await weatherService.GetContextByCityAsync(city, ct);
            return Results.Ok(byCity);
        }

        if (lat.HasValue && lon.HasValue)
        {
            var byCoords = await weatherService.GetContextAsync(
                lat.Value,
                lon.Value,
                ct);

            return Results.Ok(byCoords);
        }

        return Results.BadRequest("Either city or lat/lon is required.");
    });

app.MapPost("/api/v1/decision", (DecisionRequest req, IDecisionService svc) =>
{
    // Minimal validation
    if (string.IsNullOrWhiteSpace(req.OccasionId))
        return Results.BadRequest(new ApiError("InvalidOccasionId", "OccasionId is required."));

    if ((req.Items == null || req.Items.Count == 0)
    && string.IsNullOrWhiteSpace(req.WardrobeProfileId))
    {
        return Results.BadRequest(new ApiError("EmptyItems", "At least 1 item is required."));
    }
    try
    {
        var resp = svc.Decide(req);
        return Results.Ok(resp);
    }
    catch (NotSupportedException ex)
    {
        return Results.NotFound(new ApiError("NotSupported", ex.Message));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ApiError("BadRequest", ex.Message));
    }
});

app.MapGet("/api/v1/wardrobes", (WardrobeProfileService service) =>
{
    var profiles = service
        .GetProfiles()
        .Select(x => new
        {
            id = x.Id,
            displayName = x.DisplayName
        });

    return Results.Ok(profiles);
});

app.MapGet("/api/v1/wardrobes/{id}", (string id, WardrobeProfileService wardrobeService, CategoryCatalogService categoryCatalogService) =>
{
    var profile = wardrobeService.GetProfile(id);

    if (profile is null)
        return Results.NotFound();

    var categories = categoryCatalogService
        .GetAll()
        .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

    return Results.Ok(new
    {
        id = profile.Id,
        displayName = profile.DisplayName,
        itemCount = profile.Items.Count,
        items = profile.Items.Select(item =>
        {
            categories.TryGetValue(item.Category, out var category);

            return new
            {
                id = item.Id,
                category = item.Category,
                categoryDisplayNameTr = category?.DisplayNameTr ?? item.Category,
                categoryDisplayNameEn = category?.DisplayNameEn ?? item.Category,
                categoryGroup = category?.Group ?? "Other",
                colorFamily = item.ColorFamily,
                formality = item.Formality
            };
        })
    });
});

app.MapGet("/api/v1/catalog/categories",
    (CategoryCatalogService service) =>
    {
        return Results.Ok(service.GetAll());
    });

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

public partial class Program { }