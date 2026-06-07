namespace WordGenerator.Api.Domain.Entities
{
    public class DocumentTemplate
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public CoverPageTemplate? CoverPage { get; set; }

        public ICollection<TemplateSection> Sections { get; set; }
            = new List<TemplateSection>();
    }
}
