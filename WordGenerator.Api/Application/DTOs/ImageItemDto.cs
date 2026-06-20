namespace WordGenerator.Api.Application.DTOs
{
    public class ImageItemDto
    {
        public long? Id { get; set; }
        public string FileName { get; set; } = null!;
        public string? Caption { get; set; }
        public int Order { get; set; }
        public double Width { get; set; } = 500;
        public double Height { get; set; } = 0;
        public string? ImageBase64 { get; set; } // برای ارسال تصویر به صورت Base64
    }
}
