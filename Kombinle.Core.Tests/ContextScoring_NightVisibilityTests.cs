using System.Collections.Generic;
using Xunit;

using Kombinle.Core.Domain;
using Kombinle.Core.Domain.Context;

namespace Kombinle.Core.Tests
{
    public class ContextScoring_NightVisibilityTests
    {
        [Fact]
        public void NightOutdoor_WithNoBrightColors_ShouldApplySoftPenalty()
        {
            // Arrange: bright yok (White/Grey/Beige yok)
            var wardrobe = new List<Garment>
            {
                new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.Navy,  Formality = Formality.Casual },
                new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Black, Formality = Formality.Casual },
                new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Casual },
                // Outerwear opsiyonel: koyu seçiyoruz
                new Garment { Category = Category.Coat,  ColorFamily = ColorFamily.Navy,  Formality = Formality.Casual }
            };

            var occasion = Occasion.CasualWeekend();

            var context = new ContextInput(
                Weather: Weather.Clear,
                Setting: Setting.Outdoor,
                Time: TimeOfDay.Night
            );

            // Act
            var best = TestHarness.BestScored(wardrobe, occasion, context);

            Console.WriteLine($"ANCHOR: {best.Candidate.Anchor?.Category} / {best.Candidate.Anchor?.ColorFamily}");

            foreach (var kv in best.Candidate.SlotToItem)
            {
                Console.WriteLine($"SLOT: {kv.Key} -> {kv.Value.Category} / {kv.Value.ColorFamily}");
            }

            foreach (var note in best.ContextUserNotes)
            {
                Console.WriteLine($"NOTE: {note.Code}");
            }

            // Assert
            Assert.True(best.ContextDelta < 0);
            Assert.Contains(best.ContextUserNotes, n => n.Code == "NIGHT_LOW_VISIBILITY");
        }

        [Fact]
        public void NightOutdoor_WithBrightColor_ShouldNotApplyVisibilityPenalty()
        {
            // Arrange: bright var (White)
            var wardrobe = new List<Garment>
            {
                new Garment { Category = Category.Shirt, ColorFamily = ColorFamily.White, Formality = Formality.Casual }, // bright
                new Garment { Category = Category.Pants, ColorFamily = ColorFamily.Black, Formality = Formality.Casual },
                new Garment { Category = Category.Shoes, ColorFamily = ColorFamily.Black, Formality = Formality.Casual }
            };

            var occasion = Occasion.CasualWeekend();

            var context = new ContextInput(
                Weather: Weather.Clear,
                Setting: Setting.Outdoor,
                Time: TimeOfDay.Night
            );

            // Act
            var best = TestHarness.BestScored(wardrobe, occasion, context);

            Console.WriteLine("=== BRIGHT TEST ===");
            Console.WriteLine($"ANCHOR: {best.Candidate.Anchor?.Category} / {best.Candidate.Anchor?.ColorFamily}");

            foreach (var kv in best.Candidate.SlotToItem)
            {
                Console.WriteLine($"SLOT: {kv.Key} -> {kv.Value.Category} / {kv.Value.ColorFamily}");
            }

            foreach (var note in best.ContextUserNotes)
            {
                Console.WriteLine($"NOTE: {note.Code}");
            }

            // Assert: visibility notu gelmesin (delta 0 olmak zorunda değil; başka kurallar ileride eklenebilir)
            Assert.DoesNotContain(best.ContextUserNotes, n => n.Code == "NIGHT_LOW_VISIBILITY");
        }
    }
}
