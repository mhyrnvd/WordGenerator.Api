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

        public virtual PrefaceSection? PrefaceSection { get; set; }
        public virtual ConceptsSection? ConceptsSection { get; set; }

        // ===== بخش‌های جدید انتهای سند =====
        public virtual AttachmentSection? AttachmentSection { get; set; }
        public virtual ReferenceSection? ReferenceSection { get; set; }
        public virtual DocumentSection? DocumentSection { get; set; }
        public bool IsDeleted { get; set; } = false; // این رو اضافه کنید
        public DateTime? DeletedAt { get; set; }
    }
}
