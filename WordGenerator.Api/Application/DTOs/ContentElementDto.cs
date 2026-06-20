namespace WordGenerator.Api.Application.DTOs
{
    public class ContentElementDto
    {
        public long? Id { get; set; }
        public string Type { get; set; } = "paragraph"; // "paragraph", "image", "table"
        public int Order { get; set; }

        // برای پاراگراف
        public string? Text { get; set; }

        // برای تصویر
        public ImageItemDto? Image { get; set; }

        // برای جدول - استفاده از TableDataDto
        public TableDataDto? Table { get; set; }
    }

}
