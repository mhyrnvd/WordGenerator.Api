namespace WordGenerator.Api.Domain.Entities
{
    /// <summary>
    /// بخش منابع (ب) - انتهای سند - چپ‌چین و انگلیسی
    /// </summary>
    public class ReferenceSection
    {
        public long Id { get; set; }
        public string Title { get; set; } = "ب) References";
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 2;

        // ارتباط با DocumentTemplate
        public long DocumentTemplateId { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;

        // المان‌های این بخش
        public virtual ICollection<ContentElement> Elements { get; set; } = new List<ContentElement>();
    }
}