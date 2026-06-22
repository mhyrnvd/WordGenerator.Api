namespace WordGenerator.Api.Application.DTOs
{
    public class CreateDocumentTemplateDto
    {
        public string Name { get; set; } = null!;
        public CreateCoverPageDto? CoverPage { get; set; }
        public List<CreateMasterSectionDto> MasterSections { get; set; } = new();
        public List<CreateTemplateSectionDto> Sections { get; set; } = new();
    }
}
