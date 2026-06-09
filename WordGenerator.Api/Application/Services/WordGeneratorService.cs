using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WordGenerator.Api.Application.Requests;
using WordGenerator.Api.Domain.Entities;
using WordGenerator.Api.Infra.Context;
using Docx = DocumentFormat.OpenXml.Wordprocessing;
using Drawing = DocumentFormat.OpenXml.Drawing;
using DrawingCharts = DocumentFormat.OpenXml.Drawing.Charts;

namespace WordGenerator.Api.Application.Services
{
    public class WordGeneratorService
    {
        private readonly AppDbContext _context;

        public WordGeneratorService(AppDbContext context)
        {
            _context = context;
        }


        //private void UpdateAllFields(WordprocessingDocument doc)
        //{
        //    // آپدیت خودکار فیلدها هنگام باز شدن سند
        //    var settingsPart = doc.MainDocumentPart.DocumentSettingsPart;
        //    if (settingsPart == null)
        //    {
        //        settingsPart = doc.MainDocumentPart.AddNewPart<DocumentSettingsPart>();
        //        settingsPart.Settings = new Settings();
        //    }

        //    settingsPart.Settings.Append(new UpdateFieldsOnOpen { Val = true });
        //    settingsPart.Settings.Save();
        //}

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

            using (var doc = WordprocessingDocument.Create(ms,
                       WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();

                // اضافه کردن استایل‌ها به سند
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
                // Page Break بعد از کاور
                body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));

                body.Append(CreateHeading("فهرست مطالب"));

                // فیلد TOC
                var tocParagraph = CreateTableOfContents();
                body.Append(tocParagraph);

                // اضافه کردن فاصله
                body.Append(new Paragraph(new Run(new Text(""))));

                // اضافه کردن Page Break برای رفتن به صفحه بعد
                body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));

                // ========== صفحات اصلی ==========
                // اضافه کردن هدینگ‌ها
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
                //UpdateAllFields(doc);
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        private Docx.Table CreateTable(DynamicTable dynamicTable)
        {
            // ایجاد جدول با عرض کامل صفحه
            var table = new Docx.Table();

            // تنظیم خصوصیات جدول
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

            table.AppendChild(tableProps);

            // محاسبه عرض ستون‌ها
            var columns = dynamicTable.Columns.OrderBy(x => x.Order).ToList();
            var totalWidth = 5000;
            var fixedWidthColumns = columns.Where(c => c.Width > 0).ToList();
            var autoWidthColumns = columns.Where(c => c.Width == 0).ToList();

            var fixedWidth = fixedWidthColumns.Sum(c => c.Width * 50);
            var remainingWidth = totalWidth - fixedWidth;
            var autoWidth = autoWidthColumns.Count > 0 ? remainingWidth / autoWidthColumns.Count : 0;

            // ایجاد ردیف هدر
            var headerRow = new Docx.TableRow();
            headerRow.Append(new TableRowProperties(
                new TableRowHeight { Val = 400, HeightType = HeightRuleValues.AtLeast }
            ));

            // اضافه کردن ستون شماره ردیف اگر فعال باشد
            if (dynamicTable.ShowRowNumbers)
            {
                var rowNumberCell = CreateHeaderCell(
                    dynamicTable.RowNumberHeader ?? "ردیف",
                    true,
                    dynamicTable.ShowRowNumbers ? 10 : 0
                );
                headerRow.Append(rowNumberCell);
            }

            // اضافه کردن سایر ستون‌ها
            foreach (var column in columns)
            {
                var columnWidth = column.Width > 0 ? column.Width :
                    (autoWidthColumns.Contains(column) ? autoWidth / 50 : 20);

                var headerCell = CreateHeaderCell(column.Header, true, columnWidth);
                headerRow.Append(headerCell);
            }

            table.Append(headerRow);

            // ایجاد ردیف‌های داده
            foreach (var row in dynamicTable.Rows.OrderBy(x => x.RowNumber))
            {
                var dataRow = new Docx.TableRow();
                dataRow.Append(new TableRowProperties(
                    new TableRowHeight { Val = 300, HeightType = HeightRuleValues.AtLeast }
                ));

                // اضافه کردن شماره ردیف اگر فعال باشد
                if (dynamicTable.ShowRowNumbers)
                {
                    var rowNumberCell = CreateDataCell(row.RowNumber.ToString());
                    dataRow.Append(rowNumberCell);
                }

                // اضافه کردن سلول‌های داده به ترتیب ستون‌ها
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

            // هدر با فونت بولد و سایز 11
            paragraph.Append(
                CreateTableCellRun(
                    PrepareRTLText(text),
                    true,
                    true
                )
            );
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

            // تشخیص خودکار زبان و ایجاد Run‌های مناسب
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
                    Ascii = isPersian ? "B Nazanin" : "Times New Roman",
                    HighAnsi = isPersian ? "B Nazanin" : "Times New Roman",
                    ComplexScript = isPersian ? "B Nazanin" : "Times New Roman"
                },
                new FontSize { Val = isHeader ? "22" : (isPersian ? "22" : "20") }
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

        // تابع کمکی برای آماده‌سازی متن RTL
        private string PrepareRTLText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = Regex.Replace(
                text,
                @"\((.*?)\)",
                m => "\u200F)" + m.Groups[1].Value + "(\u200F"
            );

            return "\u202B" + text + "\u202C";
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
                // اضافه کردن Label
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(item.Label + ":"), true, false),
                    new Run(new Break())
                );

                // آماده‌سازی متن Value با تشخیص خودکار زبان
                var runs = new List<Run>();
                var current = new List<char>();
                bool? currentIsPersian = null;

                var preparedText = PrepareRTLText(item.Value);

                foreach (var c in preparedText)
                {
                    bool isPersian = !IsEnglish(c); // اگر انگلیسی نباشد، فارسی است

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

                // اضافه کردن runs به پاراگراف
                paragraph.Append(runs);

                // اضافه کردن Break بعد از Value
                paragraph.Append(new Run(new Break()));
            }

            return paragraph;
        }

        private Run CreateRunForCover(string text, bool isPersian, bool isTitle)
        {
            return new Run(
                new RunProperties(
                    new RunFonts
                    {
                        Ascii = isPersian ? "B Nazanin" : "Times New Roman",
                        HighAnsi = isPersian ? "B Nazanin" : "Times New Roman",
                        ComplexScript = isPersian ? "B Nazanin" : "Times New Roman"
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

            // ایجاد فیلد TOC
            var run = new Run();
            var fieldCode = new FieldCode { Text = "TOC \\o \"1-1\" \\h \\* MERGEFORMAT" };
            var fieldChar1 = new FieldChar { FieldCharType = FieldCharValues.Begin };
            var fieldChar2 = new FieldChar { FieldCharType = FieldCharValues.Separate };
            var fieldChar3 = new FieldChar { FieldCharType = FieldCharValues.End };

            // متن placeholder که قبل از آپدیت نمایش داده می‌شود
            var placeholderText = new Text("【اینجا کلیک کرده و F9 بزنید】");

            run.Append(fieldChar1);
            run.Append(fieldCode);
            run.Append(fieldChar2);
            run.Append(placeholderText);  // اضافه کردن متن placeholder
            run.Append(fieldChar3);

            paragraph.Append(run);

            return paragraph;
        }

        private void AddStylesToDocument(MainDocumentPart mainPart)
        {
            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            var styles = new Docx.Styles();  // استفاده از Docx.Styles به جای Styles

            // استایل Normal
            var normalStyle = new Docx.Style  // استفاده از Docx.Style
            {
                Type = StyleValues.Paragraph,
                StyleId = "Normal",
                Default = true
            };
            var normalName = new Docx.StyleName { Val = "Normal" };  // استفاده از Docx.StyleName
            normalStyle.Append(normalName);
            var normalRunProps = new Docx.StyleRunProperties(  // استفاده از Docx.StyleRunProperties
                new RunFonts
                {
                    Ascii = "B Nazanin",
                    HighAnsi = "B Nazanin",
                    ComplexScript = "B Nazanin"
                },
                new FontSize { Val = "24" }
            );
            normalStyle.Append(normalRunProps);
            styles.Append(normalStyle);

            // ایجاد استایل Heading1
            var heading1Style = new Docx.Style  // استفاده از Docx.Style
            {
                Type = StyleValues.Paragraph,
                StyleId = "Heading1",
                Default = false,
                CustomStyle = false
            };

            var heading1Name = new Docx.StyleName { Val = "heading 1" };  // استفاده از Docx.StyleName
            heading1Style.Append(heading1Name);

            var heading1ParaProps = new Docx.StyleParagraphProperties(  // استفاده از Docx.StyleParagraphProperties
                new ParagraphStyleId { Val = "Heading1" },
                new Justification { Val = JustificationValues.Left },
                new SpacingBetweenLines { After = "240", Line = "240" },
                new OutlineLevel { Val = 0 }
            );

            var heading1RunProps = new Docx.StyleRunProperties(  // استفاده از Docx.StyleRunProperties
                new RunFonts
                {
                    Ascii = "B Nazanin",
                    HighAnsi = "B Nazanin",
                    ComplexScript = "B Nazanin"
                },
                new FontSize { Val = "32" },
                new Bold()
            );

            heading1Style.Append(heading1ParaProps, heading1RunProps);
            styles.Append(heading1Style);

            stylesPart.Styles = styles;
        }

        private bool IsEnglish(char c)
        {
            // حروف انگلیسی
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                return true;
    
            // اعداد انگلیسی
            if (c >= '0' && c <= '9')
                return true;
    
            // کاراکترهای پرانتز و علائم انگلیسی
            return /*c == '(' || c == ')' || */c == '[' || c == ']' || c == '{' || c == '}' ||
                   c == '.' || c == ',' || c == ';' || c == ':' || c == '!' || c == '?' ||
                   c == '@' || c == '#' || c == '$' || c == '%' || c == '^' || c == '&' ||
                   c == '*' || c == '+' || c == '=' || c == '<' || c == '>' || c == '/' ||
                   c == '\\' || c == '|' || c == '~' || c == '`' || c == '_' || c == '-'; // فاصله هم جزو انگلیسی محسوب شود
        }

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

            // آماده‌سازی متن با RLE و PDF
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

        private Run CreateRun(string text, bool isEnglish)
        {
            if (isEnglish)
            {
                return new Run(
                    new RunProperties(
                        new RunFonts()
                        {
                            Ascii = "Times New Roman",
                            HighAnsi = "Times New Roman",
                            ComplexScript = "Times New Roman"
                        },
                        new FontSize() { Val = "24" }
                    ),
                    new Text(text)
                );
            }
            else
            {
                // فارسی
                return new Run(
                    new RunProperties(
                        new RunFonts()
                        {
                            Ascii = "B Nazanin",
                            HighAnsi = "B Nazanin",
                            ComplexScript = "B Nazanin",
                        },
                        new FontSize() { Val = "28" }
                    ),
                    new Text(text)
                );
            }
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
                            Ascii = "B Nazanin",
                            HighAnsi = "B Nazanin",
                            ComplexScript = "B Nazanin"
                        },
                        new FontSize() { Val = "32" },
                        new Bold()
                    ),
                    new Text(PrepareRTLText(text))
                )
            );
        }
    }
}