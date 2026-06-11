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
        private const string HeaderFontSize = "32";
        private const string TableHeaderFontSize = "22";

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
                    body.Append(CreateCoverPage(document.CoverPage));

                    foreach (var table in document.CoverPage.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTable(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }

                    body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                }

                // Table of Contents
                if (document.IncludeTableOfContents)
                {
                    body.Append(CreateHeading("فهرست مطالب"));
                    body.Append(CreateTableOfContents());
                    body.Append(new Paragraph(new Run(new Text(""))));
                    body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                }

                // Sections
                foreach (var section in document.Sections.OrderBy(x => x.Order))
                {
                    body.Append(CreateHeading(section.Title));

                    foreach (var paragraph in section.Paragraphs.OrderBy(x => x.Order))
                    {
                        body.Append(CreateParagraph(paragraph.Text));
                    }

                    foreach (var table in section.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTable(table));
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

                if (template.CoverPage != null)
                {
                    body.Append(CreateCoverPage(template.CoverPage));

                    foreach (var table in template.CoverPage.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTable(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }
                }

                body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                body.Append(CreateHeading("فهرست مطالب"));
                body.Append(CreateTableOfContents());
                body.Append(new Paragraph(new Run(new Text(""))));
                body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));

                foreach (var section in selectedSections)
                {
                    body.Append(CreateHeading(section.Title));

                    foreach (var paragraph in section.Paragraphs.OrderBy(x => x.Order))
                    {
                        body.Append(CreateParagraph(paragraph.Text));
                    }

                    foreach (var table in section.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTable(table));
                        body.Append(new Paragraph(new Run(new Break())));
                    }
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        #endregion

        #region Table Creation - DTO Version

        private Docx.Table CreateTable(TableDataDto tableData)
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

            var columns = tableData.Columns.OrderBy(x => x.Order).ToList();

            // Calculate column widths
            var totalWidth = 5000;
            var fixedWidthColumns = columns.Where(c => c.Width > 0).ToList();
            var autoWidthColumns = columns.Where(c => c.Width == 0).ToList();
            var fixedWidth = fixedWidthColumns.Sum(c => c.Width * 50);
            var remainingWidth = totalWidth - fixedWidth;
            var autoWidth = autoWidthColumns.Count > 0 ? remainingWidth / autoWidthColumns.Count : 0;

            // Header row
            var headerRow = new Docx.TableRow();
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

            // Data rows
            foreach (var row in tableData.Rows.OrderBy(x => x.RowNumber))
            {
                var dataRow = new Docx.TableRow();

                if (tableData.ShowRowNumbers)
                {
                    dataRow.Append(CreateDataCell(row.RowNumber.ToString()));
                }

                foreach (var column in columns)
                {
                    var cell = row.Cells.FirstOrDefault(c => c.ColumnId == column.Id);
                    var cellValue = cell?.Value ?? "";
                    dataRow.Append(CreateDataCell(cellValue));
                }

                table.Append(dataRow);
            }

            return table;
        }

        #endregion

        #region Table Creation - Entity Version

        private Docx.Table CreateTable(DynamicTable dynamicTable)
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
                var rowNumberCell = CreateHeaderCell(dynamicTable.RowNumberHeader ?? "ردیف", true, dynamicTable.ShowRowNumbers ? 10 : 0);
                headerRow.Append(rowNumberCell);
            }

            foreach (var column in columns)
            {
                var columnWidth = column.Width > 0 ? column.Width : (autoWidthColumns.Contains(column) ? autoWidth / 50 : 20);
                var headerCell = CreateHeaderCell(column.Header, true, columnWidth);
                headerRow.Append(headerCell);
            }
            table.Append(headerRow);

            // Data rows
            foreach (var row in dynamicTable.Rows.OrderBy(x => x.RowNumber))
            {
                var dataRow = new Docx.TableRow();
                dataRow.Append(new TableRowProperties(new TableRowHeight { Val = 300, HeightType = HeightRuleValues.AtLeast }));

                if (dynamicTable.ShowRowNumbers)
                {
                    var rowNumberCell = CreateDataCell(row.RowNumber.ToString());
                    dataRow.Append(rowNumberCell);
                }

                foreach (var column in columns)
                {
                    var cell = row.Cells.FirstOrDefault(c => c.TableColumnDefinitionId == column.Id);
                    var cellValue = cell?.Value ?? "";
                    var dataCell = CreateDataCell(cellValue);
                    dataRow.Append(dataCell);
                }

                table.Append(dataRow);
            }

            return table;
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
                ),
                new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "E7E6E6" }
            );

            cell.Append(cellProps);
            return cell;
        }

        private Docx.TableCell CreateDataCell(string text)
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
                        runs.Add(CreateRunForCover(new string(current.ToArray()), currentIsPersian.Value, false));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
            {
                runs.Add(CreateRunForCover(new string(current.ToArray()), currentIsPersian ?? false, false));
            }

            return runs;
        }

        private Run CreateRunForCover(string text, bool isPersian, bool isTitle)
        {
            return new Run(
                new RunProperties(
                    new RunFonts
                    {
                        Ascii = isPersian ? PersianFont : EnglishFont,
                        HighAnsi = isPersian ? PersianFont : EnglishFont,
                        ComplexScript = isPersian ? PersianFont : EnglishFont
                    },
                    new FontSize { Val = isPersian ? "32" : "28" },
                    new Bold()
                ),
                new Text(text)
                {
                    Space = SpaceProcessingModeValues.Preserve
                }
            );
        }

        private Run CreateRun(string text, bool isEnglish)
        {
            if (isEnglish)
            {
                return new Run(
                    new RunProperties(
                        new RunFonts()
                        {
                            Ascii = EnglishFont,
                            HighAnsi = EnglishFont,
                            ComplexScript = EnglishFont
                        },
                        new FontSize() { Val = EnglishFontSize }
                    ),
                    new Text(text)
                );
            }
            else
            {
                return new Run(
                    new RunProperties(
                        new RunFonts()
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont,
                        },
                        new FontSize() { Val = PersianFontSize }
                    ),
                    new Text(text)
                );
            }
        }

        #endregion

        #region Cover Page Creation

        private Paragraph CreateCoverPage(CoverPageDataDto cover)
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
                    CreateRunForCover(PrepareRTLText(cover.Title), true, true),
                    new Run(new Break())
                );
            }

            foreach (var item in cover.Items.OrderBy(x => x.Order))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(item.Label + ":"), true, false),
                    new Run(new Break())
                );

                var runs = CreateRunsForText(PrepareRTLText(item.Value));
                paragraph.Append(runs);
                paragraph.Append(new Run(new Break()));
            }

            return paragraph;
        }

        private Paragraph CreateCoverPage(CoverPageTemplate cover)
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
                    CreateRunForCover(PrepareRTLText(cover.Title), true, true),
                    new Run(new Break())
                );
            }

            foreach (var item in cover.Items.OrderBy(x => x.Order))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(item.Label + ":"), true, false),
                    new Run(new Break())
                );

                var runs = new List<Run>();
                var current = new List<char>();
                bool? currentIsPersian = null;

                var preparedText = PrepareRTLText(item.Value);

                foreach (var c in preparedText)
                {
                    bool isPersian = !IsEnglish(c);

                    if (currentIsPersian == null)
                    {
                        currentIsPersian = isPersian;
                    }

                    if (currentIsPersian != isPersian)
                    {
                        runs.Add(CreateRunForCover(new string(current.ToArray()), currentIsPersian.Value, false));
                        current.Clear();
                        currentIsPersian = isPersian;
                    }

                    current.Add(c);
                }

                if (current.Count > 0)
                {
                    runs.Add(CreateRunForCover(new string(current.ToArray()), currentIsPersian ?? false, false));
                }

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
            bool? currentIsEnglish = null;
            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isEng = IsEnglish(c);

                if (currentIsEnglish == null)
                {
                    currentIsEnglish = isEng;
                }

                if (currentIsEnglish != isEng)
                {
                    runs.Add(CreateRun(new string(current.ToArray()), currentIsEnglish.Value));
                    current.Clear();
                    currentIsEnglish = isEng;
                }

                current.Add(c);
            }

            if (current.Count > 0)
            {
                runs.Add(CreateRun(new string(current.ToArray()), currentIsEnglish ?? false));
            }

            paragraph.Append(runs);
            return paragraph;
        }

        private Paragraph CreateHeading(string text)
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
                        new FontSize() { Val = HeaderFontSize },
                        new Bold()
                    ),
                    new Text(PrepareRTLText(text))
                )
            );
        }

        private Paragraph CreateTableOfContents()
        {
            var paragraph = new Paragraph();

            var paraProps = new ParagraphProperties(
                new ParagraphStyleId { Val = "Normal" },
                new Justification { Val = JustificationValues.Right },
                new BiDi(),
                new SpacingBetweenLines { After = "120" }
            );
            paragraph.Append(paraProps);

            var run = new Run();
            var fieldCode = new FieldCode { Text = "TOC \\o \"1-1\" \\h \\* MERGEFORMAT" };
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
                new Justification { Val = JustificationValues.Left },
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
                new FontSize { Val = HeaderFontSize },
                new Bold()
            );

            heading1Style.Append(heading1ParaProps, heading1RunProps);
            styles.Append(heading1Style);

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