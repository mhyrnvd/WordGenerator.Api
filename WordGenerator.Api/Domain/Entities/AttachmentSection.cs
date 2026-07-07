namespace WordGenerator.Api.Domain.Entities
{
    /// <summary>
    /// بخش پیوست‌ها (الف) - انتهای سند
    /// </summary>
    public class AttachmentSection
    {
        public long Id { get; set; }
        public string Title { get; set; } = "الف) پیوست‌ها";
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 1;

        // ارتباط با DocumentTemplate
        public long DocumentTemplateId { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;

        // المان‌های این بخش
        public virtual ICollection<ContentElement> Elements { get; set; } = new List<ContentElement>();
    }
}