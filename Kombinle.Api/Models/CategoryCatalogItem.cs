namespace Kombinle.Api.Models
{
    public class CategoryCatalogItem
    {
        public string Id { get; set; } = "";
        public string DisplayNameTr { get; set; } = "";
        public string DisplayNameEn { get; set; } = "";
        public string Group { get; set; } = "";
        public List<string> Slots { get; set; } = [];
        public List<string> Traits { get; set; } = [];
    }
}
