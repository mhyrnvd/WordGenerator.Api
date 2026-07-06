namespace WordGenerator.Api.Application.DTOs
{
    public class ExcelImportDto
    {
        public string FileName { get; set; } = null!;
        public string Base64Content { get; set; } = null!;
        public int SheetIndex { get; set; } = 0;
        public bool HasHeader { get; set; } = true;
        public string? TableTitle { get; set; }
        public bool ShowRowNumbers { get; set; } = true;
        public string? RowNumberHeader { get; set; } = "ردیف";
    }
}