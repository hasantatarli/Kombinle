//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.Run();

using Kombinle.Api.Contracts;
using Kombinle.Api.Mapping;
using Kombinle.Api.Services;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// JSON options (camelCase default zaten var; explicit istersen)
builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// Core engine facade
builder.Services.AddSingleton<IDecisionService, DecisionService>();

var app = builder.Build();

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

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

public partial class Program { }