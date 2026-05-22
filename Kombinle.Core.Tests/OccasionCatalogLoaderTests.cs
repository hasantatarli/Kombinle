using System;
using System.IO;
using Kombinle.Core.Domain.Occasions;
using Xunit;

public class OccasionCatalogLoaderTests
{
    //[Fact]
    //public void LoadFromJsonFile_ShouldLoadKnownOccasion()
    //{
    //    var json = """
    //    {
    //      "business_meeting_formal": {
    //        "name": "Business Meeting (Formal)",
    //        "requiredFormality": "Formal",
    //        "preferredAnchorColors": ["Navy", "Black"],
    //        "defaultContext": { "weather": "Rain", "setting": "Outdoor", "timeOfDay": "Night" },
    //        "slotSet": {
    //          "requirements": [
    //            { "slot": "Anchor", "level": "Soft", "allowedCategories": ["Jacket"] },
    //            { "slot": "Top", "level": "Hard", "allowedCategories": ["Shirt"] },
    //            { "slot": "Bottom", "level": "Hard", "allowedCategories": ["Pants"] },
    //            { "slot": "Shoes", "level": "Hard", "allowedCategories": ["Shoes"] }
    //          ]
    //        }
    //      }
    //    }
    //    """;

    //    var path = Path.Combine(Path.GetTempPath(), $"occasions_{Guid.NewGuid():N}.json");
    //    File.WriteAllText(path, json);

    //    try
    //    {
    //        var map = OccasionCatalogLoader.LoadFromJsonFile(path);
    //        Assert.True(map.ContainsKey("business_meeting_formal"));
    //        Assert.Equal("Business Meeting (Formal)", map["business_meeting_formal"].Name);
    //    }
    //    finally
    //    {
    //        if (File.Exists(path)) File.Delete(path);
    //    }
    //}
}
