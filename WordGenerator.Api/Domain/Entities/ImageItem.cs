namespace WordGenerator.Api.Domain.Entities
{
    public class ImageItem
    {
        public long Id { get; set; }
        public string FileName { get; set; } = null!;
        public byte[] ImageData { get; set; } = null!;
        public string? Caption { get; set; }
        public int Order { get; set; }
        public double Width { get; set; } = 500;
        public double Height { get; set; } = 0;

        public long? TemplateSectionId { get; set; }
        public virtual TemplateSection? Section { get; set; }

        public long? MasterSectionId { get; set; }
        public virtual MasterSection? MasterSection { get; set; }

        public long? SubSectionId { get; set; }
        public virtual SubSection? SubSection { get; set; }

        public long? CoverPageId { get; set; }
        public virtual CoverPageTemplate? CoverPage { get; set; }
    }
}
