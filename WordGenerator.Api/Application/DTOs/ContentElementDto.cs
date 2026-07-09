namespace WordGenerator.Api.Application.DTOs
{
    public class ContentElementDto
    {
        public long? Id { get; set; }
        public string Type { get; set; } = "paragraph"; // "paragraph", "image", "table", "bulletlist"
        public int Order { get; set; }

        // برای پاراگراف
        public string? Text { get; set; }

        // برای تصویر
        public ImageItemDto? Image { get; set; }

        // برای جدول
        public TableDataDto? Table { get; set; }

        // برای Bullet List (جدید)
        public BulletListDto? BulletList { get; set; }
    }
}
