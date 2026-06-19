using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WordGenerator.Api.Application.DTOs;
using WordGenerator.Api.Application.Requests;
using WordGenerator.Api.Domain.Entities;
using WordGenerator.Api.Infra.Context;

// ===== Using aliases =====
using W = DocumentFormat.OpenXml.Wordprocessing;
using D = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
// =========================
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
        private const string MasterHeaderFontSize = "32";
        private const string SubHeaderFontSize = "28";
        private const string TableHeaderFontSize = "22";
        private const string CoverTitleFontSize = "32";

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
                mainPart.Document = new W.Document();
                AddStylesToDocument(mainPart);
                var body = new W.Body();

                // Cover Page
                if (document.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromDto(document.CoverPage));

                    // تصاویر کاورپیج
                    foreach (var image in document.CoverPage.Images.OrderBy(x => x.Order))
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, image, mainPart);
                        body.Append(imageParagraph);
                    }

                    foreach (var table in document.CoverPage.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
                    }

                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
                }

                // Table of Contents
                if (document.IncludeTableOfContents)
                {
                    body.Append(CreateHeading("فهرست مطالب", "32"));
                    body.Append(CreateTableOfContents());
                    body.Append(new W.Paragraph(new W.Run(new W.Text(""))));
                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
                }

                // Master Sections (Hierarchical)
                int masterCounter = 0;
                foreach (var masterSection in document.MasterSections.OrderBy(x => x.Order))
                {
                    masterCounter++;

                    body.Append(CreateMasterHeading(masterSection.Title, masterCounter.ToString()));

                    foreach (var paragraph in masterSection.Paragraphs.OrderBy(x => x.Order))
                    {
                        body.Append(CreateParagraph(paragraph.Text));
                    }

                    // تصاویر بخش اصلی
                    foreach (var image in masterSection.Images.OrderBy(x => x.Order))
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, image, mainPart);
                        body.Append(imageParagraph);
                    }

                    foreach (var table in masterSection.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
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

                        // تصاویر زیربخش
                        foreach (var image in subSection.Images.OrderBy(x => x.Order))
                        {
                            var imageParagraph = new W.Paragraph(
                                new W.ParagraphProperties(
                                    new W.Justification { Val = W.JustificationValues.Center },
                                    new W.SpacingBetweenLines { After = "200" }
                                )
                            );
                            InsertImageToParagraph(imageParagraph, image, mainPart);
                            body.Append(imageParagraph);
                        }

                        foreach (var table in subSection.Tables.OrderBy(x => x.Order))
                        {
                            body.Append(CreateTableFromDto(table));
                            body.Append(new W.Paragraph(new W.Run(new W.Break())));
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

                    // تصاویر بخش قدیمی
                    foreach (var image in section.Images.OrderBy(x => x.Order))
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, image, mainPart);
                        body.Append(imageParagraph);
                    }

                    foreach (var table in section.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
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
                    .ThenInclude(x => x.Images)
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
                    .ThenInclude(m => m.Images)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Paragraphs)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Images)
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
                    .ThenInclude(s => s.Images)
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
                mainPart.Document = new W.Document();
                AddStylesToDocument(mainPart);
                var body = new W.Body();

                // Cover Page
                if (template.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromEntity(template.CoverPage));

                    // تصاویر کاورپیج
                    foreach (var image in template.CoverPage.Images.OrderBy(x => x.Order))
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, MapImageToDto(image), mainPart);
                        body.Append(imageParagraph);
                    }

                    foreach (var table in template.CoverPage.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromEntity(table));
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
                    }

                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
                }

                // Table of Contents
                body.Append(CreateHeading("فهرست مطالب", "32"));
                body.Append(CreateTableOfContents());
                body.Append(new W.Paragraph(new W.Run(new W.Text(""))));
                body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

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

                    // تصاویر بخش اصلی
                    foreach (var image in masterSection.Images.OrderBy(x => x.Order))
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, MapImageToDto(image), mainPart);
                        body.Append(imageParagraph);
                    }

                    foreach (var table in masterSection.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromEntity(table));
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
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

                        // تصاویر زیربخش
                        foreach (var image in subSection.Images.OrderBy(x => x.Order))
                        {
                            var imageParagraph = new W.Paragraph(
                                new W.ParagraphProperties(
                                    new W.Justification { Val = W.JustificationValues.Center },
                                    new W.SpacingBetweenLines { After = "200" }
                                )
                            );
                            InsertImageToParagraph(imageParagraph, MapImageToDto(image), mainPart);
                            body.Append(imageParagraph);
                        }

                        foreach (var table in subSection.Tables.OrderBy(x => x.Order))
                        {
                            body.Append(CreateTableFromEntity(table));
                            body.Append(new W.Paragraph(new W.Run(new W.Break())));
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

                    // تصاویر بخش قدیمی
                    foreach (var image in section.Images.OrderBy(x => x.Order))
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, MapImageToDto(image), mainPart);
                        body.Append(imageParagraph);
                    }

                    foreach (var table in section.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromEntity(table));
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
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
                mainPart.Document = new W.Document();
                AddStylesToDocument(mainPart);
                var body = new W.Body();

                // Cover Page
                if (request.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromDto(request.CoverPage));

                    // تصاویر کاورپیج
                    foreach (var image in request.CoverPage.Images.OrderBy(x => x.Order))
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, image, mainPart);
                        body.Append(imageParagraph);
                    }

                    foreach (var table in request.CoverPage.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
                    }

                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
                }

                // Table of Contents
                if (request.IncludeTableOfContents)
                {
                    body.Append(CreateHeading("فهرست مطالب", "32"));
                    body.Append(CreateTableOfContents());
                    body.Append(new W.Paragraph(new W.Run(new W.Text(""))));
                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
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

                    // تصاویر بخش اصلی
                    foreach (var image in masterSection.Images.OrderBy(x => x.Order))
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, image, mainPart);
                        body.Append(imageParagraph);
                    }

                    foreach (var table in masterSection.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
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

                        // تصاویر زیربخش
                        foreach (var image in subSection.Images.OrderBy(x => x.Order))
                        {
                            var imageParagraph = new W.Paragraph(
                                new W.ParagraphProperties(
                                    new W.Justification { Val = W.JustificationValues.Center },
                                    new W.SpacingBetweenLines { After = "200" }
                                )
                            );
                            InsertImageToParagraph(imageParagraph, image, mainPart);
                            body.Append(imageParagraph);
                        }

                        foreach (var table in subSection.Tables.OrderBy(x => x.Order))
                        {
                            body.Append(CreateTableFromDto(table));
                            body.Append(new W.Paragraph(new W.Run(new W.Break())));
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

                    // تصاویر بخش قدیمی
                    foreach (var image in section.Images.OrderBy(x => x.Order))
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, image, mainPart);
                        body.Append(imageParagraph);
                    }

                    foreach (var table in section.Tables.OrderBy(x => x.Order))
                    {
                        body.Append(CreateTableFromDto(table));
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
                    }
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        #endregion

        #region Image Methods

        private void InsertImageToParagraph(W.Paragraph paragraph, ImageItemDto imageDto, MainDocumentPart mainPart)
        {
            if (string.IsNullOrEmpty(imageDto.ImageBase64))
                return;

            try
            {
                // تبدیل Base64 به بایت
                var imageBytes = Convert.FromBase64String(imageDto.ImageBase64);

                // تشخیص نوع تصویر
                var imagePartType = ImagePartType.Jpeg;
                if (imageBytes.Length > 4)
                {
                    if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                        imagePartType = ImagePartType.Png;
                    else if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                        imagePartType = ImagePartType.Jpeg;
                }

                // اضافه کردن ImagePart
                var imagePart = mainPart.AddImagePart(imagePartType);
                using (var stream = new MemoryStream(imageBytes))
                {
                    imagePart.FeedData(stream);
                }
                var imagePartId = mainPart.GetIdOfPart(imagePart);

                // ابعاد (با مقدار ثابت برای تست)
                long cx = 3000000L;
                long cy = 2000000L;

                // ساختن تصویر دقیقاً مثل OpenXmlReportRenderer
                var run = new W.Run();

                // استفاده از W.Drawing و DW.Inline
                var drawing = new W.Drawing(
                    new DW.Inline(
                        new DW.Extent() { Cx = cx, Cy = cy },
                        new DW.DocProperties()
                        {
                            Id = (UInt32Value)1U,
                            Name = "Image"
                        },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new D.GraphicFrameLocks() { NoChangeAspect = true }
                        ),
                        new D.Graphic(
                            new D.GraphicData(
                                new PIC.Picture(
                                    new PIC.NonVisualPictureProperties(
                                        new PIC.NonVisualDrawingProperties()
                                        {
                                            Id = (UInt32Value)0U,
                                            Name = "img.jpg"
                                        },
                                        new PIC.NonVisualPictureDrawingProperties()
                                    ),
                                    new PIC.BlipFill(
                                        new D.Blip()
                                        {
                                            Embed = imagePartId
                                        },
                                        new D.Stretch(
                                            new D.FillRectangle()
                                        )
                                    ),
                                    new PIC.ShapeProperties(
                                        new D.Transform2D(
                                            new D.Offset() { X = 0L, Y = 0L },
                                            new D.Extents()
                                            {
                                                Cx = cx,
                                                Cy = cy
                                            }
                                        ),
                                        new D.PresetGeometry(
                                            new D.AdjustValueList()
                                        )
                                        { Preset = D.ShapeTypeValues.Rectangle }
                                    )
                                )
                            )
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                        )
                    )
                );

                run.AppendChild(drawing);
                paragraph.AppendChild(run);
            }
            catch (Exception ex)
            {
                // در صورت خطا، یک متن نمایش بده
                var run = new W.Run(
                    new W.RunProperties(
                        new W.FontSize { Val = "24" },
                        new W.Color { Val = "FF0000" }
                    ),
                    new W.Text($"[خطا در بارگذاری تصویر: {imageDto.FileName}]")
                );
                paragraph.AppendChild(run);
            }
        }

        private ImageItemDto MapImageToDto(ImageItem image)
        {
            return new ImageItemDto
            {
                Id = image.Id,
                FileName = image.FileName,
                Caption = image.Caption,
                Order = image.Order,
                Width = image.Width,
                Height = image.Height,
                ImageBase64 = Convert.ToBase64String(image.ImageData)
            };
        }

        #endregion

        #region Heading Creation Methods

        private W.Paragraph CreateMasterHeading(string text, string sectionNumber)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.ParagraphStyleId() { Val = "Heading1" },
                    new W.Justification() { Val = W.JustificationValues.Center },
                    new W.BiDi(),
                    new W.SpacingBetweenLines { After = "240", Before = "240" },
                    new W.PageBreakBefore()
                ),
                new W.Run(
                    new W.RunProperties(
                        new W.RunFonts()
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize() { Val = MasterHeaderFontSize },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText(text))
                )
            );

            return paragraph;
        }

        private W.Paragraph CreateSubHeading(string text, string sectionNumber)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.ParagraphStyleId() { Val = "Heading2" },
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.BiDi(),
                    new W.SpacingBetweenLines { After = "120", Before = "120" }
                ),
                new W.Run(
                    new W.RunProperties(
                        new W.RunFonts()
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize() { Val = SubHeaderFontSize },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText($"{sectionNumber}- {text}"))
                )
            );

            return paragraph;
        }

        private W.Paragraph CreateHeading(string text, string fontSize)
        {
            return new W.Paragraph(
                new W.ParagraphProperties(
                    new W.ParagraphStyleId() { Val = "Heading1" },
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.BiDi(),
                    new W.SpacingBetweenLines { After = "240" }
                ),
                new W.Run(
                    new W.RunProperties(
                        new W.RunFonts()
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize() { Val = fontSize },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText(text))
                )
            );
        }

        #endregion

        #region Table Creation - DTO Version

        private W.Table CreateTableFromDto(TableDataDto tableData)
        {
            var table = new W.Table();

            var tableProperties = new W.TableProperties();
            var tableBorders = new W.TableBorders(
                new W.TopBorder { Val = W.BorderValues.Single, Size = 12, Color = "2E75B6" },
                new W.BottomBorder { Val = W.BorderValues.Single, Size = 12, Color = "2E75B6" },
                new W.LeftBorder { Val = W.BorderValues.Nil },
                new W.RightBorder { Val = W.BorderValues.Nil },
                new W.InsideHorizontalBorder { Val = W.BorderValues.Single, Size = 1, Color = "AAAAAA" },
                new W.InsideVerticalBorder { Val = W.BorderValues.Nil }
            );

            tableProperties.Append(tableBorders);
            tableProperties.Append(new W.TableWidth { Width = "100%", Type = W.TableWidthUnitValues.Pct });
            tableProperties.Append(new W.TableLayout { Type = W.TableLayoutValues.Autofit });
            tableProperties.Append(new W.Justification { Val = W.JustificationValues.Center });

            table.Append(tableProperties);

            var columns = tableData.Columns.OrderBy(x => x.Order).ToList();

            var totalWidth = 5000;
            var fixedWidthColumns = columns.Where(c => c.Width > 0).ToList();
            var autoWidthColumns = columns.Where(c => c.Width == 0).ToList();
            var fixedWidth = fixedWidthColumns.Sum(c => c.Width * 50);
            var remainingWidth = totalWidth - fixedWidth;
            var autoWidth = autoWidthColumns.Count > 0 ? remainingWidth / autoWidthColumns.Count : 0;

            var headerRow = new W.TableRow();
            headerRow.Append(new W.TableRowProperties(new W.TableRowHeight { Val = 400, HeightType = W.HeightRuleValues.AtLeast }));

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

            int rowIndex = 0;
            foreach (var row in tableData.Rows.OrderBy(x => x.RowNumber))
            {
                var dataRow = new W.TableRow();
                dataRow.Append(new W.TableRowProperties(new W.TableRowHeight { Val = 300, HeightType = W.HeightRuleValues.AtLeast }));

                string rowBackgroundColor = (rowIndex % 2 == 0) ? "DAE9F7" : null;

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

        private W.Table CreateTableFromEntity(DynamicTable dynamicTable)
        {
            var table = new W.Table();

            var tableProps = new W.TableProperties(
                new W.TableBorders(
                    new W.TopBorder { Val = W.BorderValues.Single, Size = 4, Color = "000000" },
                    new W.BottomBorder { Val = W.BorderValues.Single, Size = 4, Color = "000000" },
                    new W.LeftBorder { Val = W.BorderValues.Single, Size = 4, Color = "000000" },
                    new W.RightBorder { Val = W.BorderValues.Single, Size = 4, Color = "000000" },
                    new W.InsideHorizontalBorder { Val = W.BorderValues.Single, Size = 2, Color = "000000" },
                    new W.InsideVerticalBorder { Val = W.BorderValues.Single, Size = 2, Color = "000000" }
                ),
                new W.TableWidth { Width = "100%", Type = W.TableWidthUnitValues.Pct },
                new W.TableLayout { Type = W.TableLayoutValues.Autofit },
                new W.Justification { Val = W.JustificationValues.Center }
            );
            table.Append(tableProps);

            var columns = dynamicTable.Columns.OrderBy(x => x.Order).ToList();
            var totalWidth = 5000;
            var fixedWidthColumns = columns.Where(c => c.Width > 0).ToList();
            var autoWidthColumns = columns.Where(c => c.Width == 0).ToList();
            var fixedWidth = fixedWidthColumns.Sum(c => c.Width * 50);
            var remainingWidth = totalWidth - fixedWidth;
            var autoWidth = autoWidthColumns.Count > 0 ? remainingWidth / autoWidthColumns.Count : 0;

            var headerRow = new W.TableRow();
            headerRow.Append(new W.TableRowProperties(new W.TableRowHeight { Val = 400, HeightType = W.HeightRuleValues.AtLeast }));

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

            foreach (var row in dynamicTable.Rows.OrderBy(x => x.RowNumber))
            {
                var dataRow = new W.TableRow();
                dataRow.Append(new W.TableRowProperties(new W.TableRowHeight { Val = 300, HeightType = W.HeightRuleValues.AtLeast }));

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

        private W.Paragraph CreateCoverPageFromDto(CoverPageDataDto cover)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification { Val = W.JustificationValues.Center },
                    new W.BiDi(),
                    new W.SpacingBetweenLines { After = "300" }
                )
            );

            if (!string.IsNullOrWhiteSpace(cover.Title))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(cover.Title), true, true, CoverTitleFontSize),
                    new W.Run(new W.Break())
                );
            }

            foreach (var item in cover.Items.OrderBy(x => x.Order))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(item.Label + ":"), true, false, "32"),
                    new W.Run(new W.Break())
                );

                var runs = CreateRunsForText(PrepareRTLText(item.Value));
                paragraph.Append(runs);
                paragraph.Append(new W.Run(new W.Break()));
            }

            return paragraph;
        }

        private W.Paragraph CreateCoverPageFromEntity(CoverPageTemplate cover)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification { Val = W.JustificationValues.Center },
                    new W.BiDi(),
                    new W.SpacingBetweenLines { After = "300" }
                )
            );

            if (!string.IsNullOrWhiteSpace(cover.Title))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(cover.Title), true, true, CoverTitleFontSize),
                    new W.Run(new W.Break())
                );
            }

            foreach (var item in cover.Items.OrderBy(x => x.Order))
            {
                paragraph.Append(
                    CreateRunForCover(PrepareRTLText(item.Label + ":"), true, false, "32"),
                    new W.Run(new W.Break())
                );

                var runs = CreateRunsForText(PrepareRTLText(item.Value));
                paragraph.Append(runs);
                paragraph.Append(new W.Run(new W.Break()));
            }

            return paragraph;
        }

        #endregion

        #region Paragraph Creation

        private W.Paragraph CreateParagraph(string text)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.Indentation() { FirstLine = "397" },
                    new W.SpacingBetweenLines()
                    {
                        Line = "360",
                        LineRule = W.LineSpacingRuleValues.Auto
                    }
                )
            );

            var runs = new List<W.Run>();
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

        private W.Run CreateRunForParagraph(string text, bool isPersian)
        {
            return new W.Run(
                new W.RunProperties(
                    new W.RunFonts()
                    {
                        Ascii = isPersian ? PersianFont : EnglishFont,
                        HighAnsi = isPersian ? PersianFont : EnglishFont,
                        ComplexScript = isPersian ? PersianFont : EnglishFont
                    },
                    new W.FontSize() { Val = isPersian ? PersianFontSize : EnglishFontSize }
                ),
                new W.Text(text)
            );
        }

        #endregion

        #region Table Cell Creation

        private W.TableCell CreateHeaderCell(string text, bool isHeader, int widthPercentage)
        {
            var cell = new W.TableCell();

            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification { Val = W.JustificationValues.Center },
                    new W.BiDi(),
                    new W.SpacingBetweenLines { After = "0" }
                )
            );

            paragraph.Append(CreateTableCellRun(PrepareRTLText(text), true, true));
            cell.Append(paragraph);

            var cellProps = new W.TableCellProperties(
                new W.TableCellWidth { Type = W.TableWidthUnitValues.Dxa, Width = (widthPercentage * 50).ToString() },
                new W.TableCellVerticalAlignment { Val = W.TableVerticalAlignmentValues.Center },
                new W.TableCellMargin(
                    new W.TopMargin { Width = "100", Type = W.TableWidthUnitValues.Dxa },
                    new W.BottomMargin { Width = "100", Type = W.TableWidthUnitValues.Dxa },
                    new W.LeftMargin { Width = "100", Type = W.TableWidthUnitValues.Dxa },
                    new W.RightMargin { Width = "100", Type = W.TableWidthUnitValues.Dxa }
                )
            );

            var shading = new W.Shading
            {
                Val = W.ShadingPatternValues.Clear,
                Color = "auto",
                Fill = "FFFFFF"
            };
            cellProps.Append(shading);

            cell.Append(cellProps);
            return cell;
        }

        private W.TableCell CreateDataCell(string text, string backgroundColor = null)
        {
            var cell = new W.TableCell();

            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification { Val = W.JustificationValues.Right },
                    new W.BiDi(),
                    new W.SpacingBetweenLines { After = "0" }
                )
            );

            var runs = CreateRunsForTableCell(text);
            paragraph.Append(runs);
            cell.Append(paragraph);

            var cellProps = new W.TableCellProperties(
                new W.TableCellVerticalAlignment { Val = W.TableVerticalAlignmentValues.Center },
                new W.TableCellMargin(
                    new W.TopMargin { Width = "80", Type = W.TableWidthUnitValues.Dxa },
                    new W.BottomMargin { Width = "80", Type = W.TableWidthUnitValues.Dxa },
                    new W.LeftMargin { Width = "100", Type = W.TableWidthUnitValues.Dxa },
                    new W.RightMargin { Width = "100", Type = W.TableWidthUnitValues.Dxa }
                )
            );

            if (!string.IsNullOrEmpty(backgroundColor))
            {
                var shading = new W.Shading
                {
                    Val = W.ShadingPatternValues.Clear,
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

        private List<W.Run> CreateRunsForTableCell(string text)
        {
            var runs = new List<W.Run>();
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

        private W.Run CreateTableCellRun(string text, bool isPersian, bool isHeader)
        {
            var runProperties = new W.RunProperties(
                new W.RunFonts
                {
                    Ascii = isPersian ? PersianFont : EnglishFont,
                    HighAnsi = isPersian ? PersianFont : EnglishFont,
                    ComplexScript = isPersian ? PersianFont : EnglishFont
                },
                new W.FontSize { Val = isHeader ? TableHeaderFontSize : (isPersian ? PersianFontSize : EnglishFontSize) }
            );

            if (isHeader)
            {
                runProperties.Append(new W.Bold());
            }

            var run = new W.Run(runProperties);

            if (!string.IsNullOrWhiteSpace(text))
            {
                var textElement = new W.Text(text);
                textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
                run.Append(textElement);
            }
            else
            {
                run.Append(new W.Text(" "));
            }

            return run;
        }

        private List<W.Run> CreateRunsForText(string text)
        {
            var runs = new List<W.Run>();
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

        private W.Run CreateRunForCover(string text, bool isPersian, bool isTitle, string fontSize)
        {
            var run = new W.Run(
                new W.RunProperties(
                    new W.RunFonts
                    {
                        Ascii = isPersian ? PersianFont : EnglishFont,
                        HighAnsi = isPersian ? PersianFont : EnglishFont,
                        ComplexScript = isPersian ? PersianFont : EnglishFont
                    },
                    new W.FontSize { Val = fontSize },
                    new W.Bold()
                )
            );

            var textElement = new W.Text(text);
            textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
            run.Append(textElement);

            return run;
        }

        #endregion

        #region Table of Contents

        private W.Paragraph CreateTableOfContents()
        {
            var paragraph = new W.Paragraph();

            var paraProps = new W.ParagraphProperties(
                new W.ParagraphStyleId { Val = "Normal" },
                new W.Justification { Val = W.JustificationValues.Left },
                new W.BiDi(),
                new W.SpacingBetweenLines { After = "120" }
            );
            paragraph.Append(paraProps);

            var run = new W.Run();
            var fieldCode = new W.FieldCode { Text = "TOC \\o \"1-2\" \\h \\* MERGEFORMAT" };
            var fieldChar1 = new W.FieldChar { FieldCharType = W.FieldCharValues.Begin };
            var fieldChar2 = new W.FieldChar { FieldCharType = W.FieldCharValues.Separate };
            var fieldChar3 = new W.FieldChar { FieldCharType = W.FieldCharValues.End };
            var placeholderText = new W.Text("【اینجا کلیک کرده و F9 بزنید】");

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
            var styles = new W.Styles();

            var normalStyle = new W.Style
            {
                Type = W.StyleValues.Paragraph,
                StyleId = "Normal",
                Default = true
            };
            var normalName = new W.StyleName { Val = "Normal" };
            normalStyle.Append(normalName);
            var normalRunProps = new W.StyleRunProperties(
                new W.RunFonts
                {
                    Ascii = PersianFont,
                    HighAnsi = PersianFont,
                    ComplexScript = PersianFont
                },
                new W.FontSize { Val = "24" }
            );
            normalStyle.Append(normalRunProps);
            styles.Append(normalStyle);

            var heading1Style = new W.Style
            {
                Type = W.StyleValues.Paragraph,
                StyleId = "Heading1",
                Default = false,
                CustomStyle = false
            };

            var heading1Name = new W.StyleName { Val = "heading 1" };
            heading1Style.Append(heading1Name);

            var heading1ParaProps = new W.StyleParagraphProperties(
                new W.ParagraphStyleId { Val = "Heading1" },
                new W.Justification { Val = W.JustificationValues.Center },
                new W.SpacingBetweenLines { After = "240", Line = "240" },
                new W.OutlineLevel { Val = 0 }
            );

            var heading1RunProps = new W.StyleRunProperties(
                new W.RunFonts
                {
                    Ascii = PersianFont,
                    HighAnsi = PersianFont,
                    ComplexScript = PersianFont
                },
                new W.FontSize { Val = MasterHeaderFontSize },
                new W.Bold()
            );

            heading1Style.Append(heading1ParaProps, heading1RunProps);
            styles.Append(heading1Style);

            var heading2Style = new W.Style
            {
                Type = W.StyleValues.Paragraph,
                StyleId = "Heading2",
                Default = false,
                CustomStyle = false
            };

            var heading2Name = new W.StyleName { Val = "heading 2" };
            heading2Style.Append(heading2Name);

            var heading2ParaProps = new W.StyleParagraphProperties(
                new W.ParagraphStyleId { Val = "Heading2" },
                new W.Justification { Val = W.JustificationValues.Right },
                new W.SpacingBetweenLines { After = "120", Line = "240" },
                new W.OutlineLevel { Val = 1 }
            );

            var heading2RunProps = new W.StyleRunProperties(
                new W.RunFonts
                {
                    Ascii = PersianFont,
                    HighAnsi = PersianFont,
                    ComplexScript = PersianFont
                },
                new W.FontSize { Val = SubHeaderFontSize },
                new W.Bold()
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