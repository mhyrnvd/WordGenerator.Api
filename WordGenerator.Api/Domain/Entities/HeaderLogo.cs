namespace WordGenerator.Api.Domain.Entities
{
    public class HeaderLogo
    {
        public long Id { get; set; }
        public string FileName { get; set; } = null!;
        public byte[] ImageData { get; set; } = null!;
        public int Order { get; set; }
        public double Width { get; set; } = 60;
        public double Height { get; set; } = 40;

        public long PageHeaderId { get; set; }
        public virtual PageHeader PageHeader { get; set; } = null!;
    }
}
