namespace WordGenerator.Api.Application.DTOs
{
    /// <summary>
    /// DTO برای بخش پیوست‌ها
    /// </summary>
    public class AttachmentSectionDto
    {
        public long? Id { get; set; }
        public string Title { get; set; } = "الف) پیوست‌ها";
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 1;
        public List<ContentElementDto> Elements { get; set; } = new();
    }

    /// <summary>
    /// DTO برای بخش منابع (انگلیسی - چپ‌چین)
    /// </summary>
    public class ReferenceSectionDto
    {
        public long? Id { get; set; }
        public string Title { get; set; } = "ب) References";
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 2;
        public List<ContentElementDto> Elements { get; set; } = new();
    }

    /// <summary>
    /// DTO برای بخش مدارک
    /// </summary>
    public class DocumentSectionDto
    {
        public long? Id { get; set; }
        public string Title { get; set; } = "پ) مدارک";
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 3;
        public List<ContentElementDto> Elements { get; set; } = new();
    }
}