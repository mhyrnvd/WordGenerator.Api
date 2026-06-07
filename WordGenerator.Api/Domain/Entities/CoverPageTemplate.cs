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
    }
}
