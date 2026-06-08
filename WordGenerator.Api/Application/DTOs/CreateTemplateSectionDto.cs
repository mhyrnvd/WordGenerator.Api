namespace WordGenerator.Api.Application.DTOs
{
    public class CreateTemplateSectionDto
    {
        public string? Title { get; set; }
        public int Order { get; set; }

        public List<CreateParagraphDto> Paragraphs { get; set; } = new();
        public List<CreateDynamicTableDto> Tables { get; set; } = new();
    }
}
