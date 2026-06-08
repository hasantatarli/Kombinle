using Kombinle.Core.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{

    public class Occasion
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Formality RequiredFormality { get; set; } = Formality.Casual;
        public List<ColorFamily> PreferredAnchorColors { get; set; } = new();

        // Tie-break bonusu (küçük tut)
        public int PreferredAnchorColorTieBreakBonus { get; set; } = 1;

        // H2: artık AnchorCategories yerine SlotSet var
        public SlotSet SlotSet { get; set; } = new();

        public ContextInput? DefaultContext { get; init; }

        public List<string> CombinationModes { get; set; } = new();

        //// Hazır örnekler (istersen çoğaltırız)
        //public static Occasion WeddingFormal_JacketOrDress()
        //{
        //    return new Occasion
        //    {
        //        Name = "Wedding (Dress or Jacket Formal)",
        //        RequiredFormality = Formality.Formal,
        //        SlotSet = SlotSet.JacketOrDressFormal(),
        //        PreferredAnchorColors = new List<ColorFamily>
        //        {
        //            ColorFamily.Navy,
        //            ColorFamily.Black,
        //            ColorFamily.Grey
        //        },
        //        PreferredAnchorColorTieBreakBonus = 1
        //    };
        //}

        //public static Occasion WeddingFormal_DressOnly()
        //{
        //    return new Occasion
        //    {
        //        Name = "Wedding (DressOnly)",
        //        RequiredFormality = Formality.Formal,
        //        SlotSet = SlotSet.DressOnlyFormal(),
        //        PreferredAnchorColors = new() { ColorFamily.Navy, ColorFamily.Black, ColorFamily.Grey },
        //        PreferredAnchorColorTieBreakBonus = 1
        //    };
        //}

        // ----------------------------
        // BUSINESS MEETING
        // ----------------------------
        //public static Occasion BusinessMeeting_Formal()
        //{
        //    return new Occasion
        //    {
        //        Name = "Business Meeting (Formal)",
        //        RequiredFormality = Formality.Formal,
        //        SlotSet = SlotSet.JacketFormal(),
        //        PreferredAnchorColors = new() { ColorFamily.Navy, ColorFamily.Black, ColorFamily.Grey },
        //        DefaultContext = new ContextInput(
        //            Weather: Weather.Rain,
        //            Setting: Setting.Outdoor,
        //            Time: TimeOfDay.Night
        //        )
        //    };
        //}

        //// ----------------------------
        //// CASUAL WEEKEND
        //// ----------------------------
        //public static Occasion CasualWeekend()
        //{
        //    return new Occasion
        //    {
        //        Name = "Casual Weekend",
        //        RequiredFormality = Formality.Casual,
        //        SlotSet = SlotSet.Casual(),
        //        PreferredAnchorColors = new() { ColorFamily.Navy, ColorFamily.Grey, ColorFamily.Beige },
        //        DefaultContext = new ContextInput(
        //            Weather: Weather.Clear,
        //            Setting: Setting.Outdoor,
        //            Time: TimeOfDay.Day
        //        )
        //    };
        //}

        //// ----------------------------
        //// INTERVIEW
        //// ----------------------------
        //public static Occasion Interview_Formal()
        //{
        //    return new Occasion
        //    {
        //        Name = "Interview (Formal)",
        //        RequiredFormality = Formality.Formal,
        //        SlotSet = SlotSet.JacketFormal(),
        //        PreferredAnchorColors = new() { ColorFamily.Navy, ColorFamily.Black },
        //        DefaultContext = new ContextInput(
        //            Weather: Weather.Clear,
        //            Setting: Setting.Indoor,
        //            Time: TimeOfDay.Day
        //        )
        //    };
        //}
    }

}
