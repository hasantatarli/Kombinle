using System.Collections.Generic;
using Xunit;

using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;

namespace Kombinle.Core.Tests
{
    public class ContextOutdoor_GatingTests
    {
        [Fact]
        public void ClearOutdoorDay_ShouldNotApplyOutdoorPenalties()
        {
            // Arrange
            var wardrobe = new List<Garment>
            {
                new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Casual },
                new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Grey,  Formality = Formality.Casual },
                new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Casual }
                // Outerwear intentionally missing
            };

            var occasion = Occasion.CasualWeekend();

            var context = new ContextInput(
                Weather: Weather.Clear,
                Setting: Setting.Outdoor,
                Time: TimeOfDay.Day
            );

            var best = TestHarness.BestScored(wardrobe, occasion, context);

            // Assert
            Assert.Equal(0, best.ContextDelta);
            Assert.Empty(best.ContextWarningCodes);
            Assert.Empty(best.ContextReasons);
        }
    }
}
