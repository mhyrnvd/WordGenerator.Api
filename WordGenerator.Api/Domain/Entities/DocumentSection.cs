namespace WordGenerator.Api.Domain.Entities
{
    /// <summary>
    /// بخش مدارک (پ) - انتهای سند
    /// </summary>
    public class DocumentSection
    {
        public long Id { get; set; }
        public string Title { get; set; } = "پ) مدارک";
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 3;

        // ارتباط با DocumentTemplate
        public long DocumentTemplateId { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;

        // المان‌های این بخش
        public virtual ICollection<ContentElement> Elements { get; set; } = new List<ContentElement>();
    }
}