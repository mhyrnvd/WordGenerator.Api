namespace WordGenerator.Api.Application.DTOs
{
    public class CreateCoverPageDto
    {
        public string? Title { get; set; }
        public List<CreateCoverPageItemDto> Items { get; set; } = new();
        public List<ContentElementDto> Elements { get; set; } = new(); // جدید
    }
}
