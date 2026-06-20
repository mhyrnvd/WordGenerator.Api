namespace WordGenerator.Api.Application.DTOs
{
    public class CreateTemplateSectionDto
    {
        public string? Title { get; set; }
        public int Order { get; set; }

        public List<CreateParagraphDto> Paragraphs { get; set; } = new();
        public List<CreateDynamicTableDto> Tables { get; set; } = new();
        public List<ImageItemDto> Images { get; set; } = new();
        public List<ImageGroupDto> ImageGroups { get; set; } = new();

    }
}
