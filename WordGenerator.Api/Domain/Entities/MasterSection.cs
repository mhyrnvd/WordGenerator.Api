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
        public long TemplateId { get; set; }
        public virtual DocumentTemplate Template { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public bool ShowInToc { get; set; } = true;

        // به جای Paragraphs, Images, Tables
        public virtual ICollection<ContentElement> Elements { get; set; } = new List<ContentElement>();
        public virtual ICollection<SubSection> SubSections { get; set; } = new List<SubSection>();
    }
}