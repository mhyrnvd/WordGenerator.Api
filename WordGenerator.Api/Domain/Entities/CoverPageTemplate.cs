namespace WordGenerator.Api.Domain.Entities
{
    public class CoverPageTemplate
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public long DocumentTemplateId { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;
        public virtual ICollection<CoverPageItem> Items { get; set; } = new List<CoverPageItem>();

        // به جای Images, Tables
        public virtual ICollection<ContentElement> Elements { get; set; } = new List<ContentElement>();
    }
}
