namespace WordGenerator.Api.Domain.Entities
{
    public class PageHeader
    {
        public long Id { get; set; }
        public string? HeaderText { get; set; }  // متن سمت راست هدر
        public bool IsActive { get; set; } = true;

        // لوگوها
        public virtual ICollection<HeaderLogo> Logos { get; set; } = new List<HeaderLogo>();

        // ارتباط با تمپلیت
        public long DocumentTemplateId { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;
    }
}
