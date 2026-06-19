namespace WordGenerator.Api.Domain.Entities
{
    public class ImageGroup
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public int Order { get; set; }
        public int ImagesPerRow { get; set; } = 2; // تعداد عکس در هر ردیف

        // ارتباط با بخش‌ها
        public long? MasterSectionId { get; set; }
        public virtual MasterSection? MasterSection { get; set; }

        public long? SubSectionId { get; set; }
        public virtual SubSection? SubSection { get; set; }

        public long? TemplateSectionId { get; set; }
        public virtual TemplateSection? TemplateSection { get; set; }

        public long? CoverPageId { get; set; }
        public virtual CoverPageTemplate? CoverPage { get; set; }

        // عکس‌های داخل گروه
        public virtual ICollection<ImageItem> Images { get; set; } = new List<ImageItem>();
    }
}
