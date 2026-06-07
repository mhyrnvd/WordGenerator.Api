namespace WordGenerator.Api.Domain.Entities
{
    public class CoverPageItem
    {
        public long Id { get; set; }

        public string Label { get; set; } = null!;

        public string Value { get; set; } = null!;

        public int Order { get; set; }

        public long CoverPageTemplateId { get; set; }

        public CoverPageTemplate CoverPageTemplate { get; set; } = null!;
    }
}
