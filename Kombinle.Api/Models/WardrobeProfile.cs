namespace Kombinle.Api.Models
{
    public class WardrobeProfile
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public List<WardrobeItem> Items { get; set; } = [];
    }

    public class WardrobeItem
    {
        public string Id { get; set; } = "";
        public string Category { get; set; } = "";
        public string ColorFamily { get; set; } = "";
        public string Formality { get; set; } = "";
    }
}
