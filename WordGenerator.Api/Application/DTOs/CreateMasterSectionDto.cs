// Application/DTOs/CreateMasterSectionDto.cs
namespace WordGenerator.Api.Application.DTOs
{
    /// <summary>
    /// DTO برای ایجاد/ویرایش بخش اصلی (والد)
    /// </summary>
    public class CreateMasterSectionDto
    {
        public string? Title { get; set; }
        public int Order { get; set; }
        public bool ShowInToc { get; set; } = true;
        public List<ContentElementDto> Elements { get; set; } = new(); // جدید
        public List<CreateSubSectionDto> SubSections { get; set; } = new();
    }

    public class CreateSubSectionDto
    {
        public string? Title { get; set; }
        public int Order { get; set; }
        public bool ShowInToc { get; set; } = true;
        public List<ContentElementDto> Elements { get; set; } = new(); // جدید
    }

    public class CreateMasterParagraphDto
    {
        public string Text { get; set; } = null!;
        public int Order { get; set; }
    }

    public class CreateSubParagraphDto
    {
        public string Text { get; set; } = null!;
        public int Order { get; set; }
    }

    public class MasterSectionDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public bool ShowInToc { get; set; }
        public string SectionNumber { get; set; } = null!; // شماره بخش مانند "1"

        public List<SubSectionDto> SubSections { get; set; } = new();
        public List<MasterParagraphDto> Paragraphs { get; set; } = new();
        public List<DynamicTableDto> Tables { get; set; } = new();
    }

    public class SubSectionDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public bool ShowInToc { get; set; }
        public string SectionNumber { get; set; } = null!; // شماره مانند "1-1"

        public List<SubParagraphDto> Paragraphs { get; set; } = new();
        public List<DynamicTableDto> Tables { get; set; } = new();
    }

    public class MasterParagraphDto
    {
        public long Id { get; set; }
        public string Text { get; set; } = null!;
        public int Order { get; set; }
    }

    public class SubParagraphDto
    {
        public long Id { get; set; }
        public string Text { get; set; } = null!;
        public int Order { get; set; }
    }
}