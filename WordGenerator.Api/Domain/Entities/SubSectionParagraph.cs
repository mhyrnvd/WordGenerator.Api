// Domain/Entities/SubSectionParagraph.cs
namespace WordGenerator.Api.Domain.Entities
{
    /// <summary>
    /// پاراگراف زیربخش
    /// </summary>
    public class SubSectionParagraph
    {
        public long Id { get; set; }

        public string Text { get; set; } = null!;

        public int Order { get; set; }

        public long SubSectionId { get; set; }

        public SubSection SubSection { get; set; } = null!;
    }
}