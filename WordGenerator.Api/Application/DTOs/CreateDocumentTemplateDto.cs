namespace WordGenerator.Api.Application.DTOs
{
    public class CreateDocumentTemplateDto
    {
        public string Name { get; set; } = null!;
        public CreateCoverPageDto? CoverPage { get; set; }
        public PageHeaderDto? PageHeader { get; set; }
        public List<CreateMasterSectionDto> MasterSections { get; set; } = new();
        public List<CreateTemplateSectionDto> Sections { get; set; } = new();

        // ===== بخش‌های جدید انتهای سند =====
        public AttachmentSectionDto? AttachmentSection { get; set; }
        public ReferenceSectionDto? ReferenceSection { get; set; }
        public DocumentSectionDto? DocumentSection { get; set; }
    }
}
