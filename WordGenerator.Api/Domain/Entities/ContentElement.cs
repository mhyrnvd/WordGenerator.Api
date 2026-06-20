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

        // ارتباط با بخش‌ها
        public long? MasterSectionId { get; set; }
        public virtual MasterSection? MasterSection { get; set; }

        public long? SubSectionId { get; set; }
        public virtual SubSection? SubSection { get; set; }

        public long? TemplateSectionId { get; set; }
        public virtual TemplateSection? TemplateSection { get; set; }

        public long? CoverPageId { get; set; }
        public virtual CoverPageTemplate? CoverPage { get; set; }
    }
}
