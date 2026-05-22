using System;
using System.IO;
using Kombinle.Core.Domain.Context;
using Xunit;

namespace Kombinle.Core.Tests
{
    public class ContextMessageCatalogLoaderTests
    {
        //[Fact]
        //public void LoadFromJsonFile_ShouldLoadKnownMessage()
        //{
        //    // Arrange
        //    var json = """
        //    {
        //      "RAIN_SUEDE_SHOES": {
        //        "titleTr": "Yağmurda süet ayakkabı riskli",
        //        "titleEn": "Suede shoes are risky in rain",
        //        "detailTr": "Detay TR",
        //        "detailEn": "Detail EN"
        //      }
        //    }
        //    """;

        //    var path = Path.Combine(Path.GetTempPath(), $"context_messages_{Guid.NewGuid():N}.json");
        //    File.WriteAllText(path, json);

        //    try
        //    {
        //        // Act
        //        var map = ContextMessageCatalogLoader.LoadFromJsonFile(path);

        //        // Assert
        //        Assert.True(map.ContainsKey("RAIN_SUEDE_SHOES"));
        //        Assert.Equal("Yağmurda süet ayakkabı riskli", map["RAIN_SUEDE_SHOES"].TitleTr);
        //        Assert.Equal("Detay TR", map["RAIN_SUEDE_SHOES"].DetailTr);
        //    }
        //    finally
        //    {
        //        if (File.Exists(path)) File.Delete(path);
        //    }
        //}
    }
}
