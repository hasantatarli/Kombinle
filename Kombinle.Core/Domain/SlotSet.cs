using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain
{
    public class SlotRequirement
    {
        public Slot Slot { get; set; }
        public RequirementLevel Level { get; set; } = RequirementLevel.Hard;

        // Bu slotu hangi category(ler) doldurabilir?
        public List<Category> AllowedCategories { get; set; } = new();
        public List<string> AllowedTraits { get; set; } = new();
        public List<Slot> AllowedSlots { get; set; } = new();

    }

    public class SlotSet
    {
        // ----------------------------
        // Derived helpers
        // ----------------------------
        public List<SlotRequirement> Requirements { get; set; } = new();

        public IEnumerable<SlotRequirement> HardSlots =>
            Requirements.Where(r => r.Level == RequirementLevel.Hard);

        public IEnumerable<SlotRequirement> SoftSlots =>
            Requirements.Where(r => r.Level == RequirementLevel.Soft);

        public IEnumerable<SlotRequirement> OptionalSlots =>
            Requirements.Where(r => r.Level == RequirementLevel.Optional);

        public SlotRequirement? Get(Slot slot) =>
            Requirements.FirstOrDefault(r => r.Slot == slot);


    }
}
