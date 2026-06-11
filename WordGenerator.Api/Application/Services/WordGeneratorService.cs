using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WordGenerator.Api.Application.DTOs;
using WordGenerator.Api.Application.Requests;
using WordGenerator.Api.Domain.Entities;
using WordGenerator.Api.Infra.Context;
using Docx = DocumentFormat.OpenXml.Wordprocessing;

namespace WordGenerator.Api.Application.Services
{
    public class WordGeneratorService
    {
        private readonly AppDbContext _context;

        // Font and size constants
        private const string PersianFont = "B Nazanin";
        private const string EnglishFont = "Times New Roman";
        private const string PersianFontSize = "28";
        private const string EnglishFontSize = "24";
        private const string MasterHeaderFontSize = "32";     // 16 pt for Master Section
        private const string SubHeaderFontSize = "28";        // 14 pt for Sub Section
        private const string TableHeaderFontSize = "22";
        private const string CoverTitleFontSize = "32";       // 16 pt for cover title

        public WordGeneratorService(AppDbContext context)
        {
            _context = context;
        }

        #region Public Methods

        public byte[] Generate(DocumentGenerationDto document)
        {
            using var ms = new MemoryStream();

            using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                AddStylesToDocument(mainPart);
                var body = new Body();

                // Cover Page
                if (document.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromDto(document.CoverPage));

                    foreach (var table in document.CoverPage.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }

                    body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                }

                // Table of Contents
                if (document.IncludeTableOfContents)
                {
                    body.Append(CreateHeading("فهرست مطالب", "32"));
                    body.Append(CreateTableOfContents());
                    body.Append(new Paragraph(new Run(new Text(""))));
                    body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                }

                // Master Sections (Hierarchical)
                int masterCounter = 0;
                foreach (var masterSection in document.MasterSections.OrderBy(x => x.Order))
                {
                    masterCounter++;

                    // Master Section with page break and center alignment
                    body.Append(CreateMasterHeading(masterSection.Title, masterCounter.ToString()));

                    // Direct paragraphs of MasterSection
                    foreach (var paragraph in masterSection.Paragraphs.OrderBy(x => x.Order))
                    {
                        body.Append(CreateParagraph(paragraph.Text));
                    }

                    // Direct tables of MasterSection
                    foreach (var table in masterSection.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }

                    // Sub Sections
                    int subCounter = 0;
                    foreach (var subSection in masterSection.SubSections.OrderBy(x => x.Order))
                    {
                        subCounter++;
                        body.Append(CreateSubHeading(subSection.Title, $"{subCounter}-{masterCounter}"));

                        foreach (var paragraph in subSection.Paragraphs.OrderBy(x => x.Order))
                        {
                            body.Append(CreateParagraph(paragraph.Text));
                        }

                        foreach (var table in subSection.Tables.OrderBy(x => x.Order))
                        {
                            body.Append(CreateTableFromDto(table));
                            body.Append(new Paragraph(new Run(new Break())));
                        }
                    }
                }

                // Legacy Sections (for backward compatibility)
                foreach (var section in document.Sections.OrderBy(x => x.Order))
                {
                    body.Append(CreateHeading(section.Title, "32"));

                    foreach (var paragraph in section.Paragraphs.OrderBy(x => x.Order))
                    {
                        body.Append(CreateParagraph(paragraph.Text));
                    }

                    foreach (var table in section.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        public async Task<byte[]> GenerateAsync(GenerateDocumentRequest request)
        {
            var template = await _context.DocumentTemplates
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Items)
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Tables)
                        .ThenInclude(t => t.Columns)
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Tables)
                        .ThenInclude(t => t.Rows)
                            .ThenInclude(r => r.Cells)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Paragraphs)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Paragraphs)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Tables)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Tables)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Tables)
                        .ThenInclude(t => t.Columns)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Tables)
                        .ThenInclude(t => t.Rows)
                            .ThenInclude(r => r.Cells)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Paragraphs)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Tables)
                        .ThenInclude(t => t.Columns)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Tables)
                        .ThenInclude(t => t.Rows)
                            .ThenInclude(r => r.Cells)
                .FirstAsync(x => x.Id == request.TemplateId);

            var selectedMasterSections = template.MasterSections
                .Where(x => request.SelectedSectionIds.Contains(x.Id))
                .OrderBy(x => x.Order)
                .ToList();

            var selectedSections = template.Sections
                .Where(x => request.SelectedSectionIds.Contains(x.Id))
                .OrderBy(x => x.Order)
                .ToList();

            using var ms = new MemoryStream();

            using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                AddStylesToDocument(mainPart);
                var body = new Body();

                // Cover Page
                if (template.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromEntity(template.CoverPage));

                    foreach (var table in template.CoverPage.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromEntity(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }

                    body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                }

                // Table of Contents
                body.Append(CreateHeading("فهرست مطالب", "32"));
                body.Append(CreateTableOfContents());
                body.Append(new Paragraph(new Run(new Text(""))));
                body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));

                // Master Sections
                int masterCounter = 0;
                foreach (var masterSection in selectedMasterSections)
                {
                    masterCounter++;
                    body.Append(CreateMasterHeading(masterSection.Title, masterCounter.ToString()));

                    foreach (var paragraph in masterSection.Paragraphs.OrderBy(x => x.Order))
                    {
                        body.Append(CreateParagraph(paragraph.Text));
                    }

                    foreach (var table in masterSection.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromEntity(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }

                    int subCounter = 0;
                    foreach (var subSection in masterSection.SubSections.OrderBy(x => x.Order))
                    {
                        subCounter++;
                        body.Append(CreateSubHeading(subSection.Title, $"{subCounter}-{masterCounter}"));

                        foreach (var paragraph in subSection.Paragraphs.OrderBy(x => x.Order))
                        {
                            body.Append(CreateParagraph(paragraph.Text));
                        }

                        foreach (var table in subSection.Tables.OrderBy(x => x.Order))
                        {
                            body.Append(CreateTableFromEntity(table));
                            body.Append(new Paragraph(new Run(new Break())));
                        }
                    }
                }

                // Legacy Sections
                foreach (var section in selectedSections)
                {
                    body.Append(CreateHeading(section.Title, "32"));

                    foreach (var paragraph in section.Paragraphs.OrderBy(x => x.Order))
                    {
                        body.Append(CreateParagraph(paragraph.Text));
                    }

                    foreach (var table in section.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromEntity(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        public async Task<byte[]> GenerateFromDtoAsync(DocumentGenerationDto request)
        {
            using var ms = new MemoryStream();

            using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                AddStylesToDocument(mainPart);
                var body = new Body();

                // Cover Page
                if (request.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromDto(request.CoverPage));

                    foreach (var table in request.CoverPage.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }

                    body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                }

                // Table of Contents
                if (request.IncludeTableOfContents)
                {
                    body.Append(CreateHeading("فهرست مطالب", "32"));
                    body.Append(CreateTableOfContents());
                    body.Append(new Paragraph(new Run(new Text(""))));
                    body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                }

                // Master Sections (Hierarchical)
                int masterCounter = 0;
                foreach (var masterSection in request.MasterSections.OrderBy(x => x.Order))
                {
                    masterCounter++;

                    body.Append(CreateMasterHeading(masterSection.Title, masterCounter.ToString()));

                    foreach (var paragraph in masterSection.Paragraphs.OrderBy(x => x.Order))
                    {
                        body.Append(CreateParagraph(paragraph.Text));
                    }

                    foreach (var table in masterSection.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }

                    int subCounter = 0;
                    foreach (var subSection in masterSection.SubSections.OrderBy(x => x.Order))
                    {
                        subCounter++;
                        body.Append(CreateSubHeading(subSection.Title, $"{subCounter}-{masterCounter}"));

                        foreach (var paragraph in subSection.Paragraphs.OrderBy(x => x.Order))
                        {
                            body.Append(CreateParagraph(paragraph.Text));
                        }

                        foreach (var table in subSection.Tables.OrderBy(x => x.Order))
                        {
                            body.Append(CreateTableFromDto(table));
                            body.Append(new Paragraph(new Run(new Break())));
                        }
                    }
                }

                // Legacy Sections
                foreach (var section in request.Sections.OrderBy(x => x.Order))
                {
                    body.Append(CreateHeading(section.Title, "32"));

                    foreach (var paragraph in section.Paragraphs.OrderBy(x => x.Order))
                    {
                        body.Append(CreateParagraph(paragraph.Text));
                    }

                    foreach (var table in section.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        #endregion

        #region Heading Creation Methods

        private Paragraph CreateMasterHeading(string text, string sectionNumber)
        {
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = "Heading1" },
                    new Justification() { Val = JustificationValues.Center },
                    new BiDi(),
                    new SpacingBetweenLines { After = "240", Before = "240" },
                    new PageBreakBefore() // هر بخش اصلی در صفحه جدید شروع شود
                ),
                new Run(
                    new RunProperties(
                        new RunFonts()
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new FontSize() { Val = MasterHeaderFontSize },
                        new Bold()
                    ),
                    new Text(PrepareRTLText(text))
                )
            );

            return paragraph;
        }

        private Paragraph CreateSubHeading(string text, string sectionNumber)
        {
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = "Heading2" },
                    new Justification() { Val = JustificationValues.Left },
                    new BiDi(),
                    new SpacingBetweenLines { After = "120", Before = "120" }
                ),
                new Run(
                    new RunProperties(
                        new RunFonts()
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new FontSize() { Val = SubHeaderFontSize },
                        new Bold()
                    ),
                    new Text(PrepareRTLText($"{sectionNumber}- {text}"))
                )
            );

            return paragraph;
        }

        private Paragraph CreateHeading(string text, string fontSize)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = "Heading1" },
                    new Justification() { Val = JustificationValues.Left },
                    new BiDi(),
                    new SpacingBetweenLines { After = "240" }
                ),
                new Run(
                    new RunProperties(
                        new RunFonts()
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new FontSize() { Val = fontSize },
                        new Bold()
                    ),
                    new Text(PrepareRTLText(text))
                )
            );
        }

        #endregion

        #region Table Creation - DTO Version

        private Docx.Table CreateTableFromDto(TableDataDto tableData)
        {
            var table = new Docx.Table();

            // تنظیم Borderهای جدول
            var tableProperties = new TableProperties();
            var tableBorders = new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 12, Color = "2E75B6" },     // border بالا پررنگ
                new BottomBorder { Val = BorderValues.Single, Size = 12, Color = "2E75B6" },  // border پایین پررنگ
                new LeftBorder { Val = BorderValues.Nil },
                new RightBorder { Val = BorderValues.Nil },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 1, Color = "AAAAAA" },
                new InsideVerticalBorder { Val = BorderValues.Nil }
            );

            tableProperties.Append(tableBorders);
            tableProperties.Append(new TableWidth { Width = "100%", Type = TableWidthUnitValues.Pct });
            tableProperties.Append(new TableLayout { Type = TableLayoutValues.Autofit });
            tableProperties.Append(new Justification { Val = JustificationValues.Center });

            table.Append(tableProperties);

            var columns = tableData.Columns.OrderBy(x => x.Order).ToList();

            // محاسبه عرض ستون‌ها
            var totalWidth = 5000;
            var fixedWidthColumns = columns.Where(c => c.Width > 0).ToList();
            var autoWidthColumns = columns.Where(c => c.Width == 0).ToList();
            var fixedWidth = fixedWidthColumns.Sum(c => c.Width * 50);
            var remainingWidth = totalWidth - fixedWidth;
            var autoWidth = autoWidthColumns.Count > 0 ? remainingWidth / autoWidthColumns.Count : 0;

            // Header row (بدون رنگ پس‌زمینه)
            var headerRow = new Docx.TableRow();
            headerRow.Append(new TableRowProperties(new TableRowHeight { Val = 400, HeightType = HeightRuleValues.AtLeast }));

            if (tableData.ShowRowNumbers)
            {
                headerRow.Append(CreateHeaderCell(tableData.RowNumberHeader ?? "ردیف", true, 10));
            }

            foreach (var column in columns)
            {
                var columnWidth = column.Width > 0 ? column.Width : (autoWidthColumns.Contains(column) ? autoWidth / 50 : 20);
                headerRow.Append(CreateHeaderCell(column.Header, true, columnWidth));
            }
            table.Append(headerRow);

            // Data rows - با رنگ‌بندی ردیف‌های فرد و زوج
            int rowIndex = 0;
            foreach (var row in tableData.Rows.OrderBy(x => x.RowNumber))
            {
                var dataRow = new Docx.TableRow();
                dataRow.Append(new TableRowProperties(new TableRowHeight { Val = 300, HeightType = HeightRuleValues.AtLeast }));

                // تعیین رنگ پس‌زمینه برای این ردیف
                string rowBackgroundColor = (rowIndex % 2 == 0) ? "DAE9F7" : null;  // ردیف‌های فرد (0,2,4,...) رنگ بگیرند

                if (tableData.ShowRowNumbers)
                {
                    dataRow.Append(CreateDataCell(row.RowNumber.ToString(), rowBackgroundColor));
                }

                foreach (var column in columns)
                {
                    var cell = row.Cells.FirstOrDefault(c => c.ColumnId == column.Id);
                    var cellValue = cell?.Value ?? "";
                    dataRow.Append(CreateDataCell(cellValue, rowBackgroundColor));
                }

                table.Append(dataRow);
                rowIndex++;
            }

            return table;
        }
        #endregion

        #region Table Creation - Entity Version

        private Docx.Table CreateTableFromEntity(DynamicTable dynamicTable)
        {
            var table = new Docx.Table();

            var tableProps = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4, Color = "000000" },
                    new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "000000" },
                    new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "000000" },
                    new RightBorder { Val = BorderValues.Single, Size = 4, Color = "000000" },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 2, Color = "000000" },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 2, Color = "000000" }
                ),
                new TableWidth { Width = "100%", Type = TableWidthUnitValues.Pct },
                new TableLayout { Type = TableLayoutValues.Autofit },
                new Justification { Val = JustificationValues.Center }
            );
            table.Append(tableProps);

            var columns = dynamicTable.Columns.OrderBy(x => x.Order).ToList();
            var totalWidth = 5000;
            var fixedWidthColumns = columns.Where(c => c.Width > 0).ToList();
            var autoWidthColumns = columns.Where(c => c.Width == 0).ToList();
            var fixedWidth = fixedWidthColumns.Sum(c => c.Width * 50);
            var remainingWidth = totalWidth - fixedWidth;
            var autoWidth = autoWidthColumns.Count > 0 ? remainingWidth / autoWidthColumns.Count : 0;

            // Header row
            var headerRow = new Docx.TableRow();
            headerRow.Append(new TableRowProperties(new TableRowHeight { Val = 400, HeightType = HeightRuleValues.AtLeast }));

            if (dynamicTable.ShowRowNumbers)
            {
                headerRow.Append(CreateHeaderCell(dynamicTable.RowNumberHeader ?? "ردیف", true, 10));
            }

            foreach (var column in columns)
            {
                var columnWidth = column.Width > 0 ? column.Width : (autoWidthColumns.Contains(column) ? autoWidth / 50 : 20);
                headerRow.Append(CreateHeaderCell(column.Header, true, columnWidth));
            }
            table.Append(headerRow);

            // Data rows
            foreach (var row in dynamicTable.Rows.OrderBy(x => x.RowNumber))
            {
                var dataRow = new Docx.TableRow();
                dataRow.Append(new TableRowProperties(new TableRowHeight { Val = 300, HeightType = HeightRuleValues.AtLeast }));

                if (dynamicTable.ShowRowNumbers)
                {
                    dataRow.Append(CreateDataCell(row.RowNumber.ToString()));
                }

                foreach (var column in columns)
                {
                    var cell = row.Cells.FirstOrDefault(c => c.TableColumnDefinitionId == column.Id);
                    var cellValue = cell?.Value ?? "";
                    dataRow.Append(CreateDataCell(cellValue));
                }

                table.Append(dataRow);
            }

            return table;
        }

        #endregion

        #region Cover Page Creation

        private Paragraph CreateCoverPageFromDto(CoverPageDataDto cover)
        {
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new BiDi(),
                    new SpacingBetweenLines { After = "300" }
                )
            );

            if (!string.IsNullOrWhiteSpace(cover.Title))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(cover.Title), true, true, CoverTitleFontSize),
                    new Run(new Break())
                );
            }

            foreach (var item in cover.Items.OrderBy(x => x.Order))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(item.Label + ":"), true, false, "32"),
                    new Run(new Break())
                );

                var runs = CreateRunsForText(PrepareRTLText(item.Value));
                paragraph.Append(runs);
                paragraph.Append(new Run(new Break()));
            }

            return paragraph;
        }

        private Paragraph CreateCoverPageFromEntity(CoverPageTemplate cover)
        {
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new BiDi(),
                    new SpacingBetweenLines { After = "300" }
                )
            );

            if (!string.IsNullOrWhiteSpace(cover.Title))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(cover.Title), true, true, CoverTitleFontSize),
                    new Run(new Break())
                );
            }

            foreach (var item in cover.Items.OrderBy(x => x.Order))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(item.Label + ":"), true, false, "32"),
                    new Run(new Break())
                );

                var runs = CreateRunsForText(PrepareRTLText(item.Value));
                paragraph.Append(runs);
                paragraph.Append(new Run(new Break()));
            }

            return paragraph;
        }

        #endregion

        #region Paragraph Creation

        private Paragraph CreateParagraph(string text)
        {
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new BiDi(),
                    new Justification() { Val = JustificationValues.Left },
                    new Indentation() { FirstLine = "397" },
                    new SpacingBetweenLines()
                    {
                        Line = "360",
                        LineRule = LineSpacingRuleValues.Auto
                    }
                )
            );

            var runs = new List<Run>();
            var current = new List<char>();
            bool? currentIsPersian = null;
            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                {
                    currentIsPersian = isPersian;
                }

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                        runs.Add(CreateRunForParagraph(new string(current.ToArray()), currentIsPersian.Value));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
            {
                runs.Add(CreateRunForParagraph(new string(current.ToArray()), currentIsPersian ?? false));
            }

            paragraph.Append(runs);
            return paragraph;
        }

        private Run CreateRunForParagraph(string text, bool isPersian)
        {
            return new Run(
                new RunProperties(
                    new RunFonts()
                    {
                        Ascii = isPersian ? PersianFont : EnglishFont,
                        HighAnsi = isPersian ? PersianFont : EnglishFont,
                        ComplexScript = isPersian ? PersianFont : EnglishFont
                    },
                    new FontSize() { Val = isPersian ? PersianFontSize : EnglishFontSize }
                ),
                new Text(text)
            );
        }

        #endregion

        #region Table Cell Creation

        private Docx.TableCell CreateHeaderCell(string text, bool isHeader, int widthPercentage)
        {
            var cell = new Docx.TableCell();

            var paragraph = new Docx.Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new BiDi(),
                    new SpacingBetweenLines { After = "0" }
                )
            );

            paragraph.Append(CreateTableCellRun(PrepareRTLText(text), true, true));
            cell.Append(paragraph);

            var cellProps = new TableCellProperties(
                new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = (widthPercentage * 50).ToString() },
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center },
                new TableCellMargin(
                    new TopMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                    new BottomMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                    new LeftMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                    new RightMargin { Width = "100", Type = TableWidthUnitValues.Dxa }
                )
            );

            // اضافه کردن رنگ سفید به هدرها
            var shading = new Shading
            {
                Val = ShadingPatternValues.Clear,
                Color = "auto",
                Fill = "FFFFFF"  // رنگ سفید برای هدرها
            };
            cellProps.Append(shading);

            cell.Append(cellProps);
            return cell;
        }

        private Docx.TableCell CreateDataCell(string text, string backgroundColor = null)
        {
            var cell = new Docx.TableCell();

            var paragraph = new Docx.Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Right },
                    new BiDi(),
                    new SpacingBetweenLines { After = "0" }
                )
            );

            var runs = CreateRunsForTableCell(text);
            paragraph.Append(runs);
            cell.Append(paragraph);

            var cellProps = new TableCellProperties(
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center },
                new TableCellMargin(
                    new TopMargin { Width = "80", Type = TableWidthUnitValues.Dxa },
                    new BottomMargin { Width = "80", Type = TableWidthUnitValues.Dxa },
                    new LeftMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                    new RightMargin { Width = "100", Type = TableWidthUnitValues.Dxa }
                )
            );

            // اضافه کردن رنگ پس‌زمینه اگر مقدار داده شده باشد
            if (!string.IsNullOrEmpty(backgroundColor))
            {
                var shading = new Shading
                {
                    Val = ShadingPatternValues.Clear,
                    Color = "auto",
                    Fill = backgroundColor
                };
                cellProps.Append(shading);
            }

            cell.Append(cellProps);
            return cell;
        }
        #endregion

        #region Run Creation Helpers

        private List<Run> CreateRunsForTableCell(string text)
        {
            var runs = new List<Run>();
            var current = new List<char>();
            bool? currentIsPersian = null;

            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                {
                    currentIsPersian = isPersian;
                }

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                        runs.Add(CreateTableCellRun(new string(current.ToArray()), currentIsPersian.Value, false));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
            {
                runs.Add(CreateTableCellRun(new string(current.ToArray()), currentIsPersian ?? false, false));
            }

            if (runs.Count == 0)
            {
                runs.Add(CreateTableCellRun("", false, false));
            }

            return runs;
        }

        private Run CreateTableCellRun(string text, bool isPersian, bool isHeader)
        {
            var runProperties = new RunProperties(
                new RunFonts
                {
                    Ascii = isPersian ? PersianFont : EnglishFont,
                    HighAnsi = isPersian ? PersianFont : EnglishFont,
                    ComplexScript = isPersian ? PersianFont : EnglishFont
                },
                new FontSize { Val = isHeader ? TableHeaderFontSize : (isPersian ? PersianFontSize : EnglishFontSize) }
            );

            if (isHeader)
            {
                runProperties.Append(new Bold());
            }

            var run = new Run(runProperties);

            if (!string.IsNullOrWhiteSpace(text))
            {
                run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            }
            else
            {
                run.Append(new Text(" "));
            }

            return run;
        }

        private List<Run> CreateRunsForText(string text)
        {
            var runs = new List<Run>();
            var current = new List<char>();
            bool? currentIsPersian = null;

            foreach (var c in text)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                {
                    currentIsPersian = isPersian;
                }

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                        runs.Add(CreateRunForCover(new string(current.ToArray()), currentIsPersian.Value, false, "28"));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
            {
                runs.Add(CreateRunForCover(new string(current.ToArray()), currentIsPersian ?? false, false, "28"));
            }

            return runs;
        }

        private Run CreateRunForCover(string text, bool isPersian, bool isTitle, string fontSize)
        {
            return new Run(
                new RunProperties(
                    new RunFonts
                    {
                        Ascii = isPersian ? PersianFont : EnglishFont,
                        HighAnsi = isPersian ? PersianFont : EnglishFont,
                        ComplexScript = isPersian ? PersianFont : EnglishFont
                    },
                    new FontSize { Val = fontSize },
                    new Bold()
                ),
                new Text(text)
                {
                    Space = SpaceProcessingModeValues.Preserve
                }
            );
        }

        #endregion

        #region Table of Contents

        private Paragraph CreateTableOfContents()
        {
            var paragraph = new Paragraph();

            var paraProps = new ParagraphProperties(
                new ParagraphStyleId { Val = "Normal" },
                new Justification { Val = JustificationValues.Left },
                new BiDi(),
                new SpacingBetweenLines { After = "120" }
            );
            paragraph.Append(paraProps);

            var run = new Run();
            var fieldCode = new FieldCode { Text = "TOC \\o \"1-2\" \\h \\* MERGEFORMAT" };
            var fieldChar1 = new FieldChar { FieldCharType = FieldCharValues.Begin };
            var fieldChar2 = new FieldChar { FieldCharType = FieldCharValues.Separate };
            var fieldChar3 = new FieldChar { FieldCharType = FieldCharValues.End };
            var placeholderText = new Text("【اینجا کلیک کرده و F9 بزنید】");

            run.Append(fieldChar1);
            run.Append(fieldCode);
            run.Append(fieldChar2);
            run.Append(placeholderText);
            run.Append(fieldChar3);

            paragraph.Append(run);
            return paragraph;
        }

        #endregion

        #region Styles

        private void AddStylesToDocument(MainDocumentPart mainPart)
        {
            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            var styles = new Docx.Styles();

            // Normal Style
            var normalStyle = new Docx.Style
            {
                Type = StyleValues.Paragraph,
                StyleId = "Normal",
                Default = true
            };
            var normalName = new Docx.StyleName { Val = "Normal" };
            normalStyle.Append(normalName);
            var normalRunProps = new Docx.StyleRunProperties(
                new RunFonts
                {
                    Ascii = PersianFont,
                    HighAnsi = PersianFont,
                    ComplexScript = PersianFont
                },
                new FontSize { Val = "24" }
            );
            normalStyle.Append(normalRunProps);
            styles.Append(normalStyle);

            // Heading1 Style
            var heading1Style = new Docx.Style
            {
                Type = StyleValues.Paragraph,
                StyleId = "Heading1",
                Default = false,
                CustomStyle = false
            };

            var heading1Name = new Docx.StyleName { Val = "heading 1" };
            heading1Style.Append(heading1Name);

            var heading1ParaProps = new Docx.StyleParagraphProperties(
                new ParagraphStyleId { Val = "Heading1" },
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { After = "240", Line = "240" },
                new OutlineLevel { Val = 0 }
            );

            var heading1RunProps = new Docx.StyleRunProperties(
                new RunFonts
                {
                    Ascii = PersianFont,
                    HighAnsi = PersianFont,
                    ComplexScript = PersianFont
                },
                new FontSize { Val = MasterHeaderFontSize },
                new Bold()
            );

            heading1Style.Append(heading1ParaProps, heading1RunProps);
            styles.Append(heading1Style);

            // Heading2 Style
            var heading2Style = new Docx.Style
            {
                Type = StyleValues.Paragraph,
                StyleId = "Heading2",
                Default = false,
                CustomStyle = false
            };

            var heading2Name = new Docx.StyleName { Val = "heading 2" };
            heading2Style.Append(heading2Name);

            var heading2ParaProps = new Docx.StyleParagraphProperties(
                new ParagraphStyleId { Val = "Heading2" },
                new Justification { Val = JustificationValues.Right },
                new SpacingBetweenLines { After = "120", Line = "240" },
                new OutlineLevel { Val = 1 }
            );

            var heading2RunProps = new Docx.StyleRunProperties(
                new RunFonts
                {
                    Ascii = PersianFont,
                    HighAnsi = PersianFont,
                    ComplexScript = PersianFont
                },
                new FontSize { Val = SubHeaderFontSize },
                new Bold()
            );

            heading2Style.Append(heading2ParaProps, heading2RunProps);
            styles.Append(heading2Style);

            stylesPart.Styles = styles;
        }

        #endregion

        #region Text Helpers

        private string PrepareRTLText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = Regex.Replace(text, @"\((.*?)\)", m => "\u200F)" + m.Groups[1].Value + "(\u200F");
            return "\u202B" + text + "\u202C";
        }

        private bool IsEnglish(char c)
        {
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                return true;

            if (c >= '0' && c <= '9')
                return true;

            return c == '[' || c == ']' || c == '{' || c == '}' ||
                   c == '.' || c == ',' || c == ';' || c == ':' || c == '!' || c == '?' ||
                   c == '@' || c == '#' || c == '$' || c == '%' || c == '^' || c == '&' ||
                   c == '*' || c == '+' || c == '=' || c == '<' || c == '>' || c == '/' ||
                   c == '\\' || c == '|' || c == '~' || c == '`' || c == '_' || c == '-';
        }

        #endregion
    }
}