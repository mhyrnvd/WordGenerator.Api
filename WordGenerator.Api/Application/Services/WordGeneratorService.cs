using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WordGenerator.Api.Application.Requests;
using WordGenerator.Api.Domain.Entities;
using WordGenerator.Api.Infra.Context;

namespace WordGenerator.Api.Application.Services
{
    public class WordGeneratorService
    {
        private readonly AppDbContext _context;

        public WordGeneratorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateAsync(GenerateDocumentRequest request)
        {
            var template = await _context.DocumentTemplates
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Items)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Paragraphs)
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
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        // تابع کمکی برای آماده‌سازی متن RTL
        private string PrepareRTLText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // اضافه کردن کاراکترهای RLE و PDF برای نمایش درست متن مختلط
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
                    CreateRun(PrepareRTLText(cover.Title), true, true),
                    new Run(new Break())
                );
            }

            foreach (var item in cover.Items.OrderBy(x => x.Order))
            {
                paragraph.Append(
                    CreateRun(PrepareRTLText(":" + item.Label), true, false),
                    new Run(new Break()),
                    CreateRun(PrepareRTLText(item.Value), false, false),
                    new Run(new Break())
                );
            }

            return paragraph;
        }

        private Run CreateRun(string text, bool isPersian, bool isTitle)
        {
            return new Run(
                new RunProperties(
                    new RunFonts
                    {
                        Ascii = isPersian ? "B Nazanin" : "Times New Roman",
                        HighAnsi = isPersian ? "B Nazanin" : "Times New Roman",
                        ComplexScript = isPersian ? "B Nazanin" : "Times New Roman"
                    },
                    new FontSize { Val = isTitle ? "32" : "28" },
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
            var styles = new Styles();

            // استایل Normal
            var normalStyle = new Style
            {
                Type = StyleValues.Paragraph,
                StyleId = "Normal",
                Default = true
            };
            var normalName = new StyleName { Val = "Normal" };
            normalStyle.Append(normalName);
            var normalRunProps = new StyleRunProperties(
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
            var heading1Style = new Style
            {
                Type = StyleValues.Paragraph,
                StyleId = "Heading1",
                Default = false,
                CustomStyle = false
            };

            var heading1Name = new StyleName { Val = "heading 1" };
            heading1Style.Append(heading1Name);

            var heading1ParaProps = new StyleParagraphProperties(
                new ParagraphStyleId { Val = "Heading1" },
                new Justification { Val = JustificationValues.Left },
                new SpacingBetweenLines { After = "240", Line = "240" },
                new OutlineLevel { Val = 0 }
            );

            var heading1RunProps = new StyleRunProperties(
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
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
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