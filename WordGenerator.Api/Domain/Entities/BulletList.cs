namespace WordGenerator.Api.Domain.Entities
{
    public class BulletList
    {
        public long Id { get; set; }
        public string? Title { get; set; }  // عنوان اختیاری
        public int Order { get; set; }

        // آیتم‌های لیست
        public virtual ICollection<BulletListItem> Items { get; set; } = new List<BulletListItem>();

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

    public class BulletListItem
    {
        public long Id { get; set; }
        public string Text { get; set; } = null!;
        public int Order { get; set; }
        public int Level { get; set; } = 0;  // برای ساب‌بولت‌ها (0 = اصلی, 1 = زیرمجموعه)

        public long BulletListId { get; set; }
        public virtual BulletList BulletList { get; set; } = null!;
    }
}