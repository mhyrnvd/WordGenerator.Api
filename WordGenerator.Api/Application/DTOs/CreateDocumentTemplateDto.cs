namespace WordGenerator.Api.Application.DTOs
{
    public class CreateDocumentTemplateDto
    {
        public string Name { get; set; } = null!;

        public CreateCoverPageDto? CoverPage { get; set; }

        // بخش‌های قدیمی (برای سازگاری با عقب - می‌توان بعداً حذف کرد)
        public List<CreateTemplateSectionDto> Sections { get; set; } = new();

        // بخش‌های جدید سلسله‌مراتبی
        public List<CreateMasterSectionDto> MasterSections { get; set; } = new();
        //public List<ImageItemDto>? CoverPageImages { get; set; }
    }
}
