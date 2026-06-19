namespace WordGenerator.Api.Domain.Entities
{
    public class TemplateSection
    {
        public long Id { get; set; }

        public long TemplateId { get; set; }
        public DocumentTemplate Template { get; set; } = null!;

        public string Title { get; set; } = null!;

        public int Order { get; set; }

        public ICollection<SectionParagraph> Paragraphs { get; set; }
            = new List<SectionParagraph>();

        // اضافه کردن جداول به سکشن
        public ICollection<DynamicTable> Tables { get; set; } = new List<DynamicTable>();
        public ICollection<ImageItem> Images { get; set; } = new List<ImageItem>();

    }
}
