using Kombinle.Core.Domain;
using Kombinle.Core.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Tests
{
    public class JacketFormal_WhenAnchorMissing_ShouldStillGenerateTests
    {
    //    [Fact]
    //    public void JacketFormal_WhenAnchorMissing_ShouldStillGenerate()
    //    {
    //        // Arrange
    //        var wardrobe = new List<Garment>
    //{
    //    new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Formal },
    //    new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey, Formality = Formality.Formal },
    //    new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Formal }
    //    // Jacket intentionally missing
    //};

    //        var occasion = Occasion.BusinessMeeting_Formal();
    //        var gen = new CombinationGenerator();

    //        // Act
    //        var combos = gen.Generate(wardrobe, occasion, maxResults: 10);

    //        // Assert
    //        Assert.True(combos.Count > 0);
    //    }
    }
}
