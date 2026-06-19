// Domain/Entities/SubSection.cs
namespace WordGenerator.Api.Domain.Entities
{
    /// <summary>
    /// زیربخش (فرزند) - مانند 1-1، 1-2، etc.
    /// </summary>
    public class SubSection
    {
        public long Id { get; set; }

        /// <summary>
        /// عنوان زیربخش (مثل "پیش‌گفتار"، "اهداف")
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// ترتیب زیربخش درون بخش والد (1، 2، 3، ...)
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// آیا این زیربخش در فهرست مطالب نمایش داده شود؟
        /// </summary>
        public bool ShowInToc { get; set; } = true;

        /// <summary>
        /// آی‌دی بخش اصلی والد
        /// </summary>
        public long MasterSectionId { get; set; }

        /// <summary>
        /// بخش اصلی والد
        /// </summary>
        public MasterSection MasterSection { get; set; } = null!;

        /// <summary>
        /// پاراگراف‌های این زیربخش
        /// </summary>
        public ICollection<SubSectionParagraph> Paragraphs { get; set; } = new List<SubSectionParagraph>();

        /// <summary>
        /// جداول این زیربخش
        /// </summary>
        public ICollection<DynamicTable> Tables { get; set; } = new List<DynamicTable>();
    }
}