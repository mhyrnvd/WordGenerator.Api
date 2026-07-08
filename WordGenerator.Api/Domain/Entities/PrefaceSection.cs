namespace WordGenerator.Api.Domain.Entities
{
    public class PrefaceSection
    {
        public long Id { get; set; }
        public string Title { get; set; } = "پیش‌گفتار";
        public bool IsActive { get; set; } = true;

        public long DocumentTemplateId { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;

        public virtual ICollection<ContentElement> Elements { get; set; } = new List<ContentElement>();
    }
}
