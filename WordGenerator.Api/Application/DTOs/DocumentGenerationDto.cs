// DocumentGenerationDto.cs
namespace WordGenerator.Api.Application.DTOs
{
    public class DocumentGenerationDto
    {
        public CoverPageDataDto? CoverPage { get; set; }
        public bool IncludeTableOfContents { get; set; } = true;
        public List<MasterSectionDataDto> MasterSections { get; set; } = new();
        public List<SectionDataDto> Sections { get; set; } = new();
        public PageHeaderDto? PageHeader { get; set; }

        // ===== بخش‌های جدید قبل از فهرست مطالب =====
        public PrefaceSectionDto? Preface { get; set; }      // پیش‌گفتار
        public ConceptsSectionDto? Concepts { get; set; }    // مفاهیم

        public AttachmentSectionDto? Attachments { get; set; }      // الف) پیوست‌ها
        public ReferenceSectionDto? References { get; set; }        // ب) References (انگلیسی - چپ‌چین)
        public DocumentSectionDto? Documents { get; set; }          // پ) مدارک
    }

    /// <summary>
    /// بخش پیش‌گفتار - راست‌چین
    /// </summary>
    public class PrefaceSectionDto
    {
        public string Title { get; set; } = "پیش‌گفتار";
        public List<ContentElementDto> Elements { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// بخش مفاهیم - راست‌چین
    /// </summary>
    public class ConceptsSectionDto
    {
        public string Title { get; set; } = "مفاهیم";
        public List<ContentElementDto> Elements { get; set; } = new();
        public bool IsActive { get; set; } = true;
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
        public List<ContentElementDto> Elements { get; set; } = new(); // ← جدید
        public List<SubSectionDataDto> SubSections { get; set; } = new();
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
        public List<ContentElementDto> Elements { get; set; } = new(); // ← جدید
    }

    public class CoverPageDataDto
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public List<CoverPageItemDto> Items { get; set; } = new();
        public List<ContentElementDto> Elements { get; set; } = new(); // ← جدید
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
        public long? Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public List<ContentElementDto> Elements { get; set; } = new(); // ← جدید
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

        public AttachmentSectionDto? Attachments { get; set; }      // الف) پیوست‌ها
        public ReferenceSectionDto? References { get; set; }        // ب) References (انگلیسی - چپ‌چین)
        public DocumentSectionDto? Documents { get; set; }          // پ) مدارک
    }

    public class RowDataDto
    {
        public long? Id { get; set; }
        public int RowNumber { get; set; }
        public List<string> Values { get; set; } = new();
        public List<CellDataDto> Cells { get; set; } = new();
    }

    public class CellDataDto
    {
        public long? Id { get; set; }  // optional
        public long ColumnId { get; set; }
        public string Value { get; set; } = null!;
    }
}