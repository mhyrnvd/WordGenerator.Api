namespace WordGenerator.Api.Application.DTOs
{
    public class BulletListDto
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public int Order { get; set; }
        public List<BulletListItemDto> Items { get; set; } = new();
    }

    public class BulletListItemDto
    {
        public long? Id { get; set; }
        public string Text { get; set; } = null!;
        public int Order { get; set; }
        public int Level { get; set; } = 0;  // 0 = اصلی, 1 = زیرمجموعه
    }
}