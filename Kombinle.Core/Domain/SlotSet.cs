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


        // ----------------------------
        // FACTORIES
        // ----------------------------

        /// <summary>
        /// Jacket + Top + Bottom + Shoes (Formal scenarios)
        /// Used for: Business Meeting, Interview
        /// </summary>
        public static SlotSet JacketFormal()
        {
            return new SlotSet
            {
                Requirements = new List<SlotRequirement>
            {
                new()
                {
                    Slot = Slot.Anchor,
                    Level = RequirementLevel.Soft,
                    AllowedCategories = new List<Category> { Category.Jacket }
                },
                new()
                {
                    Slot = Slot.Top,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Shirt, Category.Blouse }
                },
                new()
                {
                    Slot = Slot.Bottom,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Pants, Category.Skirt }
                },
                new()
                {
                    Slot = Slot.Shoes,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Shoes }
                },
                new()
                {
                    Slot = Slot.Outerwear,
                    Level = RequirementLevel.Optional,
                    AllowedCategories = new List<Category> { Category.Coat, Category.Jacket }
                }
            }
            };
        }

        // ----------------------------
        // FACTORIES
        // ----------------------------

        /// <summary>
        /// Jacket + Top + Bottom + Shoes (Formal scenarios)
        /// Used for: Business Meeting, Interview
        /// </summary>
        public static SlotSet JacketOrDressFormal()
        {
            return new SlotSet
            {
                Requirements = new List<SlotRequirement>
            {
                new()
                {
                    Slot = Slot.Anchor,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Jacket , Category.Dress}
                },
                new()
                {
                    Slot = Slot.Top,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Shirt, Category.Blouse }
                },
                new()
                {
                    Slot = Slot.Bottom,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Pants, Category.Skirt }
                },
                new()
                {
                    Slot = Slot.Shoes,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Shoes }
                },
            }
            };
        }

        /// <summary>
        /// Dress + Shoes (Formal, minimal)
        /// Used for: Wedding (DressOnly)
        /// </summary>
        public static SlotSet DressOnlyFormal()
        {
            return new SlotSet
            {
                Requirements = new List<SlotRequirement>
            {
                new()
                {
                    Slot = Slot.Anchor,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Dress }
                },
                new()
                {
                    Slot = Slot.Shoes,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Shoes }
                },
                new()
                {
                    Slot = Slot.Outerwear,
                    Level = RequirementLevel.Optional,
                    AllowedCategories = new List<Category> { Category.Jacket, Category.Coat }
                }
            }
            };
        }

        /// <summary>
        /// Casual scenarios
        /// Anchor = Top or Dress
        /// Bottom + Shoes required
        /// Outerwear optional
        /// </summary>
        public static SlotSet Casual()
        {
            return new SlotSet
            {
                Requirements = new List<SlotRequirement>
            {
                new()
                {
                    Slot = Slot.Anchor,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Shirt, Category.Blouse, Category.Dress }
                },
                new()
                {
                    Slot = Slot.Bottom,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Pants, Category.Skirt }
                },
                new()
                {
                    Slot = Slot.Shoes,
                    Level = RequirementLevel.Hard,
                    AllowedCategories = new List<Category> { Category.Shoes }
                },
                new()
                {
                    Slot = Slot.Outerwear,
                    Level = RequirementLevel.Optional,
                    AllowedCategories = new List<Category> { Category.Jacket, Category.Coat }
                }
            }
            };
        }
    }
}
