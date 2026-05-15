using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Kombinle.Api.Tests;

public class DecisionResponseSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DecisionResponseSmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SafeScenario_ShouldReturnSafeDecision()
    {
        var json = """
        {
          "occasionId": "business_meeting_formal",
          "context": {
            "weather": "Clear",
            "setting": "Indoor",
            "timeOfDay": "Day"
          },
          "items": [
            { "tempId": "s1", "category": "Jacket", "colorFamily": "Navy", "formality": "Formal" },
            { "tempId": "s2", "category": "Shirt", "colorFamily": "White", "formality": "Formal" },
            { "tempId": "s3", "category": "Pants", "colorFamily": "Grey", "formality": "Formal" },
            { "tempId": "s4", "category": "Shoes", "colorFamily": "Black", "formality": "Formal", "shoe": { "material": "Leather" } }
          ]
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();
        payload.Should().NotBeNull();

        payload!.Decision.BestContextHealth.Should().Be("Good");
        payload.RecommendedAlternative.Should().BeNull();
        payload.WardrobeFeedback.Should().BeNull();
        payload.Alternatives.Should().NotBeNull();
        payload.Decision.Outfit.Items.Should().HaveCount(4);
    }

    [Fact]
    public async Task RainSuedeScenario_ShouldAvoidSuedeShoes_WhenAlternativeExists()
    {
        var json = """
        {
          "occasionId": "business_meeting_formal",
          "context": {
            "weather": "Rain",
            "setting": "Outdoor",
            "timeOfDay": "Day"
          },
          "items": [
            { "tempId": "w1", "category": "Jacket", "colorFamily": "Navy", "formality": "Formal" },
            { "tempId": "w2", "category": "Shirt", "colorFamily": "White", "formality": "Formal" },
            { "tempId": "w3", "category": "Pants", "colorFamily": "Grey", "formality": "Formal" },
            { "tempId": "w4", "category": "Shoes", "colorFamily": "Black", "formality": "Formal", "shoe": { "material": "Suede" } },
            { "tempId": "w5", "category": "Shoes", "colorFamily": "Brown", "formality": "Formal", "shoe": { "material": "Leather" } }
          ]
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();
        //Console.WriteLine(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        //{
        //    WriteIndented = true
        //}));

        payload!.Decision.BestContextHealth.Should().Be("Okay");

        payload.Decision.Outfit.Items
            .Should()
            .Contain(x => x.Category == "Shoes" && x.ColorFamily == "Brown");

        payload.Decision.Outfit.Items
            .Should()
            .NotContain(x => x.Category == "Shoes" && x.ColorFamily == "Black");

        payload.RecommendedAlternative.Should().BeNull();
        payload.WardrobeFeedback.Should().BeNull();
        payload.Alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public async Task NoBestScenario_ShouldReturnEmptyOutfit()
    {
        var json = """
        {
          "occasionId": "business_meeting_formal",
          "context": {
            "weather": "Clear",
            "setting": "Indoor",
            "timeOfDay": "Day"
          },
          "items": [
            { "tempId": "n1", "category": "Shirt", "colorFamily": "White", "formality": "Formal" },
            { "tempId": "n2", "category": "Shoes", "colorFamily": "Black", "formality": "Formal", "shoe": { "material": "Leather" } }
          ]
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();
        payload.Should().NotBeNull();

        payload!.Decision.Outfit.Items.Should().BeEmpty();
        payload.RecommendedAlternative.Should().BeNull();
        payload.Alternatives.Should().BeEmpty();
        payload.WardrobeFeedback.Should().BeNull();
    }

    [Fact]
    public async Task SoftWarningScenario_ShouldUseSoftTone()
    {
        var json = """
        {
          "occasionId": "casual_weekend",
          "context": {
            "weather": "Clear",
            "setting": "Outdoor",
            "timeOfDay": "Night"
          },
          "items": [
            { "tempId": "b1", "category": "Shirt", "colorFamily": "White", "formality": "Casual" },
            { "tempId": "b2", "category": "Pants", "colorFamily": "Black", "formality": "Casual" },
            { "tempId": "b3", "category": "Shoes", "colorFamily": "Black", "formality": "Casual" }
          ]
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();
        payload.Should().NotBeNull();

        payload!.Decision.HeadlineTr.Should().Contain("kullanılabilir");
        payload.WardrobeFeedback.Should().NotBeNull();
        payload.WardrobeFeedback!.Code.Should().Be("SOFT_ANCHOR_MISSING");
        payload.RecommendedAlternative.Should().BeNull();
    }

    private Task<HttpResponseMessage> PostJson(string json)
    {
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return _client.PostAsync("/api/v1/decision", content);
    }

    [Fact]
    public async Task WeddingDressScenario_ShouldReturnDressBasedOutfit()
    {
        var json = """
    {
      "occasionId": "wedding_formal_dress",
      "context": {
        "weather": "Clear",
        "setting": "Outdoor",
        "timeOfDay": "Day"
      },
      "items": [
        { "tempId": "d1", "category": "Dress", "colorFamily": "Black", "formality": "Formal" },
        { "tempId": "d2", "category": "Shoes", "colorFamily": "Black", "formality": "Formal" }
      ]
    }
    """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();
        payload.Should().NotBeNull();

        payload!.ScenarioTitle.Should().Be("Wedding (Dress Only)");
        payload.Decision.BestContextHealth.Should().Be("Good");
        payload.RecommendedAlternative.Should().BeNull();
        payload.WardrobeFeedback.Should().BeNull();
        payload.Alternatives.Should().BeEmpty();

        payload.Decision.Outfit.Items.Should().HaveCount(2);
        payload.Decision.Outfit.Items.Should().ContainSingle(x =>
            x.Slot == "Anchor" &&
            x.Category == "Dress" &&
            x.ColorFamily == "Black");

        payload.Decision.Outfit.Items.Should().ContainSingle(x =>
            x.Slot == "Shoes" &&
            x.Category == "Shoes" &&
            x.ColorFamily == "Black");

        payload.Decision.Outfit.ShortTr.Should().Be("Black Dress + Black Shoes");
    }

    [Fact]
    public async Task WeddingFlexible_DressPath_ShouldReturnDressOutfit()
    {
        var json = """
    {
      "occasionId": "wedding_formal_flexible",
      "context": {
        "weather": "Clear",
        "setting": "Outdoor",
        "timeOfDay": "Day"
      },
      "items": [
        { "tempId": "wf1", "category": "Dress", "colorFamily": "Black", "formality": "Formal" },
        { "tempId": "wf2", "category": "Shoes", "colorFamily": "Black", "formality": "Formal" }
      ]
    }
    """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();
        payload.Should().NotBeNull();

        payload!.ScenarioTitle.Should().Be("Wedding (Flexible)");
        payload.Decision.BestContextHealth.Should().Be("Good");
        payload.RecommendedAlternative.Should().BeNull();
        payload.WardrobeFeedback.Should().BeNull();
        payload.Alternatives.Should().BeEmpty();

        payload.Decision.Outfit.Items.Should().HaveCount(2);

        payload.Decision.Outfit.Items.Should().ContainSingle(x =>
            x.Slot == "Anchor" &&
            x.Category == "Dress" &&
            x.ColorFamily == "Black");

        payload.Decision.Outfit.Items.Should().ContainSingle(x =>
            x.Slot == "Shoes" &&
            x.Category == "Shoes" &&
            x.ColorFamily == "Black");

        payload.Decision.Outfit.ShortTr.Should().Be("Black Dress + Black Shoes");
    }

    [Fact]
    public async Task WeddingFlexible_TopBottomPath_ShouldReturnSeparatedOutfit()
    {
        var json = """
    {
      "occasionId": "wedding_formal_flexible",
      "context": {
        "weather": "Clear",
        "setting": "Outdoor",
        "timeOfDay": "Day"
      },
      "items": [
        { "tempId": "wf3", "category": "Blouse", "colorFamily": "White", "formality": "Formal" },
        { "tempId": "wf4", "category": "Skirt", "colorFamily": "Black", "formality": "Formal" },
        { "tempId": "wf5", "category": "Shoes", "colorFamily": "Black", "formality": "Formal" }
      ]
    }
    """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();
        payload.Should().NotBeNull();

        payload!.ScenarioTitle.Should().Be("Wedding (Flexible)");
        payload.Decision.BestContextHealth.Should().Be("Good");
        payload.RecommendedAlternative.Should().BeNull();
        payload.Alternatives.Should().BeEmpty();

        payload.WardrobeFeedback.Should().NotBeNull();
        payload.WardrobeFeedback!.Code.Should().Be("SOFT_ANCHOR_MISSING");

        payload.Decision.Outfit.Items.Should().HaveCount(3);

        payload.Decision.Outfit.Items.Should().ContainSingle(x =>
            x.Slot == "Top" &&
            x.Category == "Blouse" &&
            x.ColorFamily == "White");

        payload.Decision.Outfit.Items.Should().ContainSingle(x =>
            x.Slot == "Bottom" &&
            x.Category == "Skirt" &&
            x.ColorFamily == "Black");

        payload.Decision.Outfit.Items.Should().ContainSingle(x =>
            x.Slot == "Shoes" &&
            x.Category == "Shoes" &&
            x.ColorFamily == "Black");

        payload.Decision.Outfit.Items.Should().NotContain(x => x.Slot == "Anchor");
        payload.Decision.Outfit.ShortTr.Should().Be("White Blouse + Black Skirt + Black Shoes");
    }

    [Fact]
    public async Task WeddingFlexible_MixedWardrobe_ShouldNotProduceHybridOutfit()
    {
        var json = """
    {
      "occasionId": "wedding_formal_flexible",
      "context": {
        "weather": "Clear",
        "setting": "Outdoor",
        "timeOfDay": "Day"
      },
      "items": [
        { "tempId": "d1", "category": "Dress", "colorFamily": "Black", "formality": "Formal" },
        { "tempId": "t1", "category": "Blouse", "colorFamily": "White", "formality": "Formal" },
        { "tempId": "b1", "category": "Skirt", "colorFamily": "Black", "formality": "Formal" },
        { "tempId": "s1", "category": "Shoes", "colorFamily": "Black", "formality": "Formal" }
      ]
    }
    """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        // 🔥 EN KRİTİK ASSERT
        payload!.Decision.Outfit.Items.Should().NotContain(x =>
            x.Category == "Dress" &&
            payload.Decision.Outfit.Items.Any(i => i.Slot == "Top") &&
            payload.Decision.Outfit.Items.Any(i => i.Slot == "Bottom")
        );
        payload.Decision.Outfit.Items.Should().Contain(x => x.Category == "Dress");
        payload.Decision.Outfit.Items.Should().NotContain(x => x.Slot == "Top");
        payload.Decision.Outfit.Items.Should().NotContain(x => x.Slot == "Bottom");
    }

    [Fact]
    public async Task WeddingFlexible_DressWorseColor_ShouldPreferTopBottom()
    {
        var json = """
    {
      "occasionId": "wedding_formal_flexible",
      "context": {
        "weather": "Clear",
        "setting": "Outdoor",
        "timeOfDay": "Day"
      },
      "items": [
        { "tempId": "d1", "category": "Dress", "colorFamily": "Brown", "formality": "Formal" },
        { "tempId": "t1", "category": "Blouse", "colorFamily": "White", "formality": "Formal" },
        { "tempId": "b1", "category": "Skirt", "colorFamily": "Black", "formality": "Formal" },
        { "tempId": "s1", "category": "Shoes", "colorFamily": "Black", "formality": "Formal" }
      ]
    }
    """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        // 👉 Dress yerine TopBottom seçilmeli
        payload!.Decision.Outfit.Items.Should().Contain(x => x.Slot == "Top");
        payload.Decision.Outfit.Items.Should().Contain(x => x.Slot == "Bottom");
        payload.Decision.Outfit.Items.Should().NotContain(x => x.Category == "Dress");
    }

    [Fact]
    public async Task WeddingFlexible_AllCandidatesHardFail_ShouldReturnNoViableResult()
    {
        var json = """
    {
      "occasionId": "wedding_formal_flexible",
      "context": {
        "weather": "Clear",
        "setting": "Outdoor",
        "timeOfDay": "Day"
      },
      "items": [
        { "tempId": "d1", "category": "Dress", "colorFamily": "Black", "formality": "Formal" },
        { "tempId": "t1", "category": "Blouse", "colorFamily": "White", "formality": "Formal" },
        { "tempId": "b1", "category": "Skirt", "colorFamily": "Black", "formality": "Formal" },
        { "tempId": "s1", "category": "Shoes", "colorFamily": "Brown", "formality": "Formal" }
      ]
    }
    """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();
        payload.Should().NotBeNull();

        payload!.Decision.Should().NotBeNull();
        payload.Decision.Outfit.Should().NotBeNull();

        payload.Decision.Outfit.Items.Should().BeEmpty();
        payload.Alternatives.Should().BeEmpty();
        payload.RecommendedAlternative.Should().BeNull();

        //payload.WardrobeFeedback.Should().NotBeNull();
        //payload.WardrobeFeedback!.Code.Should().Be("INCOMPLETE_OUTFIT");
    }


}

public sealed class DecisionResponseDto
{
    public string ScenarioTitle { get; set; } = "";
    public DecisionCardDto Decision { get; set; } = new();
    public AlternativeCardDto? RecommendedAlternative { get; set; }
    public WardrobeFeedbackCardDto? WardrobeFeedback { get; set; }
    public List<AlternativeCardDto> Alternatives { get; set; } = new();
    public DebugDto Debug { get; set; } = new();
}

public sealed class DecisionCardDto
{
    public string HeadlineTr { get; set; } = "";
    public string? SubtextTr { get; set; }
    public OutfitDto Outfit { get; set; } = new();
    public string BestContextHealth { get; set; } = "";
}

public sealed class OutfitDto
{
    public List<OutfitItemDto> Items { get; set; } = new();
    public string ShortTr { get; set; } = "";
}

public sealed class OutfitItemDto
{
    public string Slot { get; set; } = "";
    public string Category { get; set; } = "";
    public string ColorFamily { get; set; } = "";
}

public sealed class AlternativeCardDto
{
    public string ShortTr { get; set; } = "";
    public List<AlternativeReasonDto> Reasons { get; set; } = new();
}

public sealed class AlternativeReasonDto
{
    public string Code { get; set; } = "";
    public string TitleTr { get; set; } = "";
    public string? DetailTr { get; set; }
}

public sealed class WardrobeFeedbackCardDto
{
    public string LineTr { get; set; } = "";
    public string? DetailTr { get; set; }
    public string Code { get; set; } = "";
}

public sealed class DebugDto
{
    public int GeneratedCount { get; set; }
    public int RankedCount { get; set; }
    public double ContextAvgDelta { get; set; }
    public double ContextPenaltyRate { get; set; }
    public double ContextWarningRate { get; set; }
}