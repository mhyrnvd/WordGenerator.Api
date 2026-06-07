namespace WordGenerator.Api.Domain.Entities
{
    public class SectionParagraph
    {
        public long Id { get; set; }

        public long TemplateSectionId { get; set; }
        public TemplateSection Section { get; set; } = null!;

        public string Text { get; set; } = null!;

        public int Order { get; set; }
    }
}
