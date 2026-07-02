namespace WordGenerator.Api.Domain.Entities
{
    public class DocumentTemplate
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public CoverPageTemplate? CoverPage { get; set; }
        public virtual PageHeader? PageHeader { get; set; }

        // بخش‌های قدیمی (برای سازگاری با عقب)
        public ICollection<TemplateSection> Sections { get; set; } = new List<TemplateSection>();

        // بخش‌های جدید سلسله‌مراتبی
        public ICollection<MasterSection> MasterSections { get; set; } = new List<MasterSection>();
    }
}
