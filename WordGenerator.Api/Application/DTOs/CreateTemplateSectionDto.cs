namespace WordGenerator.Api.Application.DTOs
{
    public class CreateTemplateSectionDto
    {
        public string? Title { get; set; }
        public int Order { get; set; }
        public List<ContentElementDto> Elements { get; set; } = new(); // جدید
    }
}
