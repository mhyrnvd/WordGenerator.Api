namespace WordGenerator.Api.Domain.Entities
{
    public class TemplateSection
    {
        public long Id { get; set; }
        public long TemplateId { get; set; }
        public virtual DocumentTemplate Template { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int Order { get; set; }

        // به جای Paragraphs, Images, Tables
        public virtual ICollection<ContentElement> Elements { get; set; } = new List<ContentElement>();
    }
}
