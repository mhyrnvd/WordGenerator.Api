// Domain/Entities/SubSection.cs
namespace WordGenerator.Api.Domain.Entities
{
    /// <summary>
    /// زیربخش (فرزند) - مانند 1-1، 1-2، etc.
    /// </summary>
    public class SubSection
    {
        public long Id { get; set; }
        public long MasterSectionId { get; set; }
        public virtual MasterSection MasterSection { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public bool ShowInToc { get; set; } = true;

        // به جای Paragraphs, Images, Tables
        public virtual ICollection<ContentElement> Elements { get; set; } = new List<ContentElement>();
    }
}