using WordGenerator.Api.Domain.Enums;

namespace WordGenerator.Api.Domain.Entities
{
    public class ContentElement
    {
        public long Id { get; set; }
        public ContentElementType Type { get; set; }
        public int Order { get; set; }

        // برای پاراگراف
        public string? ParagraphText { get; set; }

        // برای تصویر
        public long? ImageId { get; set; }
        public virtual ImageItem? Image { get; set; }

        // برای جدول
        public long? TableId { get; set; }
        public virtual DynamicTable? Table { get; set; }

        // برای Bullet List
        public long? BulletListId { get; set; }
        public virtual BulletList? BulletList { get; set; }

        // ارتباط با بخش‌های اصلی
        public long? MasterSectionId { get; set; }
        public virtual MasterSection? MasterSection { get; set; }

        public long? SubSectionId { get; set; }
        public virtual SubSection? SubSection { get; set; }

        public long? TemplateSectionId { get; set; }
        public virtual TemplateSection? TemplateSection { get; set; }

        public long? CoverPageId { get; set; }
        public virtual CoverPageTemplate? CoverPage { get; set; }

        // ===== ارتباط با بخش‌های انتهای سند =====
        public long? AttachmentSectionId { get; set; }
        public virtual AttachmentSection? AttachmentSection { get; set; }

        public long? ReferenceSectionId { get; set; }
        public virtual ReferenceSection? ReferenceSection { get; set; }

        public long? DocumentSectionId { get; set; }
        public virtual DocumentSection? DocumentSection { get; set; }

        public long? PrefaceSectionId { get; set; }
        public virtual PrefaceSection? PrefaceSection { get; set; }

        public long? ConceptsSectionId { get; set; }
        public virtual ConceptsSection? ConceptsSection { get; set; }
    }
}
