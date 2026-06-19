// Domain/Entities/MasterSectionParagraph.cs
namespace WordGenerator.Api.Domain.Entities
{
    /// <summary>
    /// پاراگراف مستقیم زیر بخش اصلی (بدون زیربخش)
    /// </summary>
    public class MasterSectionParagraph
    {
        public long Id { get; set; }

        public string Text { get; set; } = null!;

        public int Order { get; set; }

        public long MasterSectionId { get; set; }

        public MasterSection MasterSection { get; set; } = null!;
    }
}