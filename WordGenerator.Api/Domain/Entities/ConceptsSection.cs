namespace WordGenerator.Api.Domain.Entities
{
    public class ConceptsSection
    {
        public long Id { get; set; }
        public string Title { get; set; } = "مفاهیم";
        public bool IsActive { get; set; } = true;

        public long DocumentTemplateId { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;

        public virtual ICollection<ContentElement> Elements { get; set; } = new List<ContentElement>();
    }
}