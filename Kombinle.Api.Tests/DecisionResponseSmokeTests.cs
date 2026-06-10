using FluentAssertions;
using Kombinle.Api.Contracts;
using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;
using Kombinle.Core.Domain.Occasions;
using Kombinle.Core.Generation;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.Utilities;
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

        //Console.WriteLine(payload!.Decision.Outfit.ShortTr);
        //foreach (var item in payload.Decision.Outfit.Items)
        //{
        //    Console.WriteLine($"{item.Slot} - {item.Category} - {item.ColorFamily}");
        //}
        //Console.WriteLine(payload.WardrobeFeedback?.Code);

        //Console.WriteLine($"RawBest: {payload.Debug?.RawBestSignature}");
        //Console.WriteLine($"ShownBest: {payload.Debug?.ShownBestSignature}");
        //Console.WriteLine($"RotationAttempt: {payload.Debug?.RotationAttempt}");
        //Console.WriteLine($"BestPoolCount: {payload.Debug?.BestPoolCount}");

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

    [Fact]
    public async Task SameRequest_ShouldRotateShownBestWithinBestPool()
    {
        var json = """
        {
          "occasionId": "smart_casual_dinner",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Clear",
            "season": "Spring",
            "setting": "Indoor",
            "timeOfDay": "Night"
          }
        }
        """;

        var json2 = """
        {
          "occasionId": "smart_casual_dinner",
          "wardrobeProfileId": "male_extended_v1",
          "rotationAttempt": 1,
          "context": {
            "weather": "Clear",
            "season": "Spring",
            "setting": "Indoor",
            "timeOfDay": "Night"
          }
        }
        """;

        var firstResponse = await PostJson(json);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstPayload =
            await firstResponse.Content.ReadFromJsonAsync<DecisionResponseDto>();

        firstPayload.Should().NotBeNull();

        var secondResponse = await PostJson(json2);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondPayload =
            await secondResponse.Content.ReadFromJsonAsync<DecisionResponseDto>();

        secondPayload.Should().NotBeNull();

        firstPayload!.Debug.BestPoolCount.Should().BeGreaterThan(1);
        secondPayload!.Debug.RotationAttempt.Should().BeGreaterThan(firstPayload.Debug.RotationAttempt);

        secondPayload.Debug.RawBestSignature.Should().Be(firstPayload.Debug.RawBestSignature);
        secondPayload.Debug.ShownBestSignature.Should().NotBe(firstPayload.Debug.ShownBestSignature);
    }

    [Fact]
    public async Task CasualWeekend_ShouldSelectShoesUsingAllowedTraits()
    {
        var json = """
        {
          "occasionId": "casual_weekend",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Clear",
            "season": "Spring",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        payload!.Decision.Outfit.Items
            .Should()
            .Contain(x => x.Slot == "Shoes");

        payload.Decision.Outfit.Items
            .Where(x => x.Slot == "Shoes")
            .Should()
            .OnlyContain(x =>
                x.Category == "Shoes" ||
                x.Category == "Sneakers");
    }

    [Fact]
    public async Task CasualWeekend_ShouldSelectTopUsingAllowedTraits()
    {
        var json = """
        {
          "occasionId": "casual_weekend",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Clear",
            "season": "Spring",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        payload!.Decision.Outfit.Items
            .Should()
            .Contain(x => x.Slot == "Top");

        payload.Decision.Outfit.Items
            .Where(x => x.Slot == "Top")
            .Should()
            .OnlyContain(x =>
                x.Category == "Tshirt" ||
                x.Category == "Shirt" ||
                x.Category == "Sweater");
    }

    [Fact]
    public async Task StructuredSmartAnchor_ShouldNotReturnFormalityWeakFeedback()
    {
        var json = """
        {
          "occasionId": "business_meeting_formal",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Clear",
            "season": "Spring",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();
        payload!.WardrobeFeedback.Should().BeNull("smart structured jackets are acceptable soft anchors for formal business meetings");

    }

    [Fact]
    public async Task CasualWeekend_ShouldSelectAnchorUsingAllowedSlots()
    {
        var json = """
        {
          "occasionId": "casual_weekend",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Clear",
            "season": "Spring",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        payload!.Decision.Outfit.Items
            .Should()
            .Contain(x => x.Slot == "Anchor");
    }
    [Fact]
    public async Task CasualWeekend_IndoorSpring_ShouldAvoidLightOuterwearAsBestAnchor()
    {
        var json = """
        {
          "occasionId": "casual_weekend",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Clear",
            "season": "Spring",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        var anchor =
            payload!.Decision.Outfit.Items
                .FirstOrDefault(x => x.Slot == "Anchor");

        anchor.Should().NotBeNull();

        anchor!.Category.Should().NotBe("LightOuterwear");
    }

    [Fact]
    public void Matches_ShouldMatchByAllowedSlots()
    {
        var garment = new Garment
        {
            CategoryId = "Hoodie",
            ColorFamily = ColorFamily.Black,
            Formality = Formality.Casual
        };

        var req = new SlotRequirement
        {
            Slot = Slot.Anchor,
            Level = RequirementLevel.Soft,
            AllowedSlots = new List<Slot> { Slot.Anchor }
        };

        SlotRequirementMatcher.Matches(garment, req)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Matches_ShouldNotMatch_WhenNoCategoryTraitOrSlotMatches()
    {
        var garment = new Garment
        {
            CategoryId = "Coat",
            ColorFamily = ColorFamily.Black,
            Formality = Formality.Formal
        };


        var req = new SlotRequirement
        {
            Slot = Slot.Top,
            Level = RequirementLevel.Hard,
            AllowedTraits = new List<string> { "Top" }
        };

        SlotRequirementMatcher.Matches(garment, req)
            .Should()
            .BeFalse();
    }


    [Fact]
    public async Task DressPath_ShouldUseSemanticShoesSlot()
    {
        var json = """
        {
          "occasionId": "wedding_formal_dress",
          "context": {
            "weather": "Clear",
            "season": "Spring",
            "setting": "Indoor",
            "timeOfDay": "Day"
          },
          "items": [
            {
              "tempId": "d1",
              "category": "Dress",
              "colorFamily": "Black",
              "formality": "Formal"
            },
            {
              "tempId": "s1",
              "category": "Sneakers",
              "colorFamily": "White",
              "formality": "Smart"
            }
          ]
        }
        """;

        var response = await PostJson(json);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        payload!.Decision.Outfit.Items
            .Should()
            .Contain(x =>
                x.Slot == "Shoes"
                && x.Category == "Sneakers");
    }

    [Fact]
    public async Task CasualWeekend_ColdOutdoor_ShouldNotStackMultipleProtectionLayers()
    {
        var json = """
    {
      "occasionId": "casual_weekend",
      "wardrobeProfileId": "male_extended_v1",
      "context": {
        "weather": "Cold",
        "season": "Winter",
        "setting": "Outdoor",
        "timeOfDay": "Day"
      }
    }
    """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        var categories = payload!.Decision.Outfit.Items
            .Select(x => x.Category)
            .ToList();

        var protectionLayerCount = categories.Count(x =>
            x == "Coat" ||
            x == "LightOuterwear");

        protectionLayerCount.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public async Task CasualWeekend_SummerColdOutdoor_ShouldNotUseCoatOrStackJacketWithLightOuterwear()
    {
        var json = """
        {
          "occasionId": "casual_weekend",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Cold",
            "season": "Summer",
            "setting": "Outdoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        var categories = payload!.Decision.Outfit.Items
            .Select(x => x.Category)
            .ToList();

        categories.Should().NotContain("Coat");

        var hasJacket = categories.Contains("Jacket");
        var hasLightOuterwear = categories.Contains("LightOuterwear");

        (hasJacket && hasLightOuterwear)
            .Should()
            .BeFalse("summer cold outdoor should use either a structure layer or a light protection layer, not both");
    }

    [Fact]
    public async Task SmartCasualDinner_WinterCold_ShouldPreferCoatInBestPool()
    {
        var json = """
        {
          "occasionId": "smart_casual_dinner",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Cold",
            "season": "Winter",
            "setting": "Outdoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();
        payload!.Debug.Should().NotBeNull();

        payload.Debug!.BestPoolCandidates.Should().Contain(x =>
            x.Signature.Contains("Outerwear:Coat"),
            "winter cold scenarios should prefer heavy protection layers when available");
    }

    [Fact]
    public async Task BusinessMeeting_MaleExtended_ShouldUseStructuredAnchor()
    {
        var json = """
        {
          "occasionId": "business_meeting_formal",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Hot",
            "season": "Summer",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        var categories = payload!.Decision.Outfit.Items
            .Select(x => x.Category)
            .ToList();

        categories.Should().Contain("Jacket",
            "business meeting should allow smart structured jackets as soft anchors even when target formality is formal");
    }

    [Fact]
    public async Task BusinessMeeting_IndoorColdWinter_ShouldNotIncludeOuterwear()
    {
        var json = """
        {
          "occasionId": "business_meeting_formal",
          "wardrobeProfileId": "male_extended_v1",
          "context": {
            "weather": "Cold",
            "season": "Winter",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        var categories = payload!.Decision.Outfit.Items
            .Select(x => x.Category)
            .ToList();

        categories.Should().NotContain("Coat");
        categories.Should().NotContain("LightOuterwear");
    }

    [Fact]
    public async Task BusinessMeeting_FemaleBalanced_ShouldPreferBusinessAppropriateStructuredOutfit()
    {
        var json = """
        {
          "occasionId": "business_meeting_formal",
          "wardrobeProfileId": "female_balanced_v1",
          "context": {
            "weather": "Clear",
            "season": "Summer",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        var categories = payload!.Decision.Outfit.Items
            .Select(x => x.Category)
            .ToList();

        categories.Should().Contain("Jacket");
        categories.Should().Contain("Blouse");
    }

    [Fact]
    public async Task SmartCasualDinner_SummerIndoor_ShouldAllowStructuredAnchorWithoutOuterwearPenalty()
    {
        var json = """
        {
          "occasionId": "smart_casual_dinner",
          "wardrobeProfileId": "female_balanced_v1",
          "context": {
            "weather": "Clear",
            "season": "Summer",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();
        payload!.Debug.Should().NotBeNull();

        payload.Debug!.BestPoolCandidates.Should().Contain(x =>
            x.Signature.Contains("Anchor:Jacket") &&
            x.ContextDelta == 0,
            "structured anchors should not be treated as unnecessary summer indoor outerwear");
    }

    [Fact]
    public async Task SmartCasualDinner_SummerClear_ShouldNotSuggestWarmTop()
    {
        var json = """
        {
          "occasionId": "smart_casual_dinner",
          "wardrobeProfileId": "female_balanced_v1",
          "context": {
            "weather": "Clear",
            "season": "Summer",
            "setting": "Indoor",
            "timeOfDay": "Day"
          }
        }
        """;

        var response = await PostJson(json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload =
            await response.Content.ReadFromJsonAsync<DecisionResponseDto>();

        payload.Should().NotBeNull();

        var outfitText = payload!.Decision.Outfit.Items
            .Select(x => x.Category)
            .ToList();

        outfitText.Should().NotContain("Sweater",
            "warm tops should not be suggested in summer unless the weather is cold");
    }
}

public sealed class DecisionResponseDto
{
    public string ScenarioTitle { get; set; } = "";
    public DecisionCardDto Decision { get; set; } = new();
    public AlternativeCardDto? RecommendedAlternative { get; set; }
    public WardrobeFeedbackCardDto? WardrobeFeedback { get; set; }
    public List<AlternativeCardDto> Alternatives { get; set; } = new();
    public DebugDto? Debug { get; set; }
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

//public sealed class DebugDto
//{
//    public int GeneratedCount { get; set; }
//    public int RankedCount { get; set; }
//    public double ContextAvgDelta { get; set; }
//    public double ContextPenaltyRate { get; set; }
//    public double ContextWarningRate { get; set; }
//}