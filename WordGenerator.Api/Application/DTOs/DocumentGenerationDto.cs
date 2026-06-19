// DocumentGenerationDto.cs
namespace WordGenerator.Api.Application.DTOs
{
    public class DocumentGenerationDto
    {
        public CoverPageDataDto? CoverPage { get; set; }
        public bool IncludeTableOfContents { get; set; } = true;

        // بخش‌های قدیمی
        public List<SectionDataDto> Sections { get; set; } = new();

        // بخش‌های جدید سلسله‌مراتبی
        public List<MasterSectionDataDto> MasterSections { get; set; } = new();
    }

    /// <summary>
    /// DTO برای بخش اصلی در درخواست تولید سند
    /// </summary>
    public class MasterSectionDataDto
    {
        public long? Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public bool ShowInToc { get; set; } = true;

        public List<SubSectionDataDto> SubSections { get; set; } = new();
        public List<ParagraphDataDto> Paragraphs { get; set; } = new();
        public List<TableDataDto> Tables { get; set; } = new();
        public List<ImageItemDto> Images { get; set; } = new();
    }

    /// <summary>
    /// DTO برای زیربخش در درخواست تولید سند
    /// </summary>
    public class SubSectionDataDto
    {
        public long? Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public bool ShowInToc { get; set; } = true;

        public List<ParagraphDataDto> Paragraphs { get; set; } = new();
        public List<TableDataDto> Tables { get; set; } = new();
        public List<ImageItemDto> Images { get; set; } = new();
    }

    public class CoverPageDataDto
    {
        public long? Id { get; set; }  // optional
        public string? Title { get; set; }
        public List<CoverPageItemDto> Items { get; set; } = new();
        public List<TableDataDto> Tables { get; set; } = new();
        public List<ImageItemDto> Images { get; set; } = new();
    }

    public class CoverPageItemDto
    {
        public long? Id { get; set; }  // optional
        public string Label { get; set; } = null!;
        public string Value { get; set; } = null!;
        public int Order { get; set; }
    }

    public class SectionDataDto
    {
        public long? Id { get; set; }  // optional
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public List<ParagraphDataDto> Paragraphs { get; set; } = new();
        public List<TableDataDto> Tables { get; set; } = new();
        public List<ImageItemDto> Images { get; set; } = new();
    }

    public class ParagraphDataDto
    {
        public long? Id { get; set; }  // optional
        public string Text { get; set; } = null!;
        public int Order { get; set; }
    }

    public class TableDataDto
    {
        public long? Id { get; set; }  // optional
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public bool ShowRowNumbers { get; set; }
        public string? RowNumberHeader { get; set; }
        public List<ColumnDataDto> Columns { get; set; } = new();
        public List<RowDataDto> Rows { get; set; } = new();
    }

    public class ColumnDataDto
    {
        public long? Id { get; set; }  // optional
        public string Header { get; set; } = null!;
        public int Width { get; set; }
        public int Order { get; set; }
        public bool IsRowNumberColumn { get; set; }  // optional
    }

    public class RowDataDto
    {
        public long? Id { get; set; }  // optional
        public int RowNumber { get; set; }
        public List<CellDataDto> Cells { get; set; } = new();
    }

    public class CellDataDto
    {
        public long? Id { get; set; }  // optional
        public long ColumnId { get; set; }
        public string Value { get; set; } = null!;
    }
}