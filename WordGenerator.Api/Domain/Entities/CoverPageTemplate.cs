namespace WordGenerator.Api.Domain.Entities
{
    public class CoverPageTemplate
    {
        public long Id { get; set; }

        public string? Title { get; set; } = null!;

        public long DocumentTemplateId { get; set; }
        public DocumentTemplate DocumentTemplate { get; set; } = null!;

        public ICollection<CoverPageItem> Items { get; set; }
            = new List<CoverPageItem>();

        // اضافه کردن جداول به کاورپیج
        public ICollection<DynamicTable> Tables { get; set; } = new List<DynamicTable>();
        public ICollection<ImageItem> Images { get; set; } = new List<ImageItem>();
        public ICollection<ImageGroup> ImageGroups { get; set; } = new List<ImageGroup>();
    }
}
