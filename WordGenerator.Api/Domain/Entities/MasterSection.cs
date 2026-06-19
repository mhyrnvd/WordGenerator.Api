// Domain/Entities/MasterSection.cs
namespace WordGenerator.Api.Domain.Entities
{
    /// <summary>
    /// بخش اصلی (والد) - مانند مقدمه، فصل اول، etc.
    /// هر بخش اصلی در یک صفحه جداگانه قرار می‌گیرد و وسط‌چین است
    /// </summary>
    public class MasterSection
    {
        public long Id { get; set; }

        /// <summary>
        /// عنوان بخش اصلی (مثل "مقدمه"، "فصل اول: معرفی")
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// ترتیب نمایش (1، 2، 3، ...)
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// آیا این بخش در فهرست مطالب نمایش داده شود؟
        /// </summary>
        public bool ShowInToc { get; set; } = true;

        /// <summary>
        /// آی‌دی تمپلیت والد
        /// </summary>
        public long DocumentTemplateId { get; set; }

        /// <summary>
        /// تمپلیت والد
        /// </summary>
        public DocumentTemplate DocumentTemplate { get; set; } = null!;

        /// <summary>
        /// زیربخش‌های این بخش اصلی
        /// </summary>
        public ICollection<SubSection> SubSections { get; set; } = new List<SubSection>();

        /// <summary>
        /// پاراگراف‌های مستقیم زیر بخش اصلی (اختیاری)
        /// </summary>
        public ICollection<MasterSectionParagraph> Paragraphs { get; set; } = new List<MasterSectionParagraph>();

        /// <summary>
        /// جداول مستقیم زیر بخش اصلی (اختیاری)
        /// </summary>
        public ICollection<DynamicTable> Tables { get; set; } = new List<DynamicTable>();
    }
}