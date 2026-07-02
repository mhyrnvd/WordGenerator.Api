namespace WordGenerator.Api.Application.DTOs
{
    public class PageHeaderDto
    {
        public long? Id { get; set; }
        public string? HeaderText { get; set; }
        public bool IsActive { get; set; } = true;
        public List<HeaderLogoDto> Logos { get; set; } = new();
    }

    public class HeaderLogoDto
    {
        public long? Id { get; set; }
        public string FileName { get; set; } = null!;
        public int Order { get; set; }
        public double Width { get; set; } = 60;
        public double Height { get; set; } = 40;
        public string? ImageBase64 { get; set; }
    }
}
