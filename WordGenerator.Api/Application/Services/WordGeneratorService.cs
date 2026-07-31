using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using WordGenerator.Api.Application.DTOs;
using WordGenerator.Api.Application.Requests;
using WordGenerator.Api.Domain.Entities;
using WordGenerator.Api.Domain.Enums;
using WordGenerator.Api.Infra.Context;
using D = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
// ===== Using aliases =====
using W = DocumentFormat.OpenXml.Wordprocessing;

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
        private const string BulletFontSize = "24";

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
                InitializeNumbering(mainPart);

                var body = new W.Body();

                // ===== Cover Page =====
                if (document.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromDto(document.CoverPage));
                    int dummy1 = 0, dummy2 = 0;
                    foreach (var element in document.CoverPage.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart, ref dummy1, ref dummy2, "0");
                    }
                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
                }

                // ===== پیش‌گفتار =====
                if (document.Preface != null && document.Preface.IsActive)
                {
                    RenderPrefaceSection(body, document.Preface, mainPart);
                }

                // ===== مفاهیم =====
                if (document.Concepts != null && document.Concepts.IsActive)
                {
                    RenderConceptsSection(body, document.Concepts, mainPart);
                }

                // ===== Table of Contents =====
                if (document.IncludeTableOfContents)
                {
                    body.Append(CreateHeading("فهرست مطالب", "32"));
                    body.Append(CreateTableOfContents());
                    body.Append(new W.Paragraph(new W.Run(new W.Text(""))));

                    body.Append(CreateHeading("فهرست تصاویر", "32"));
                    body.Append(CreateTableOfFigures());
                    body.Append(new W.Paragraph(new W.Run(new W.Text(""))));

                    body.Append(CreateHeading("فهرست جداول", "32"));
                    body.Append(CreateTableOfTables());
                    body.Append(new W.Paragraph(new W.Run(new W.Text(""))));

                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
                }

                // ===== Master Sections =====
                int masterCounter = 0;
                foreach (var masterSection in document.MasterSections.OrderBy(x => x.Order))
                {
                    masterCounter++;
                    string sectionNumber = masterCounter.ToString();

                    int tableCounterInSection = 0;
                    int imageCounterInSection = 0;

                    body.Append(CreateMasterHeading(masterSection.Title, sectionNumber));

                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

                    foreach (var element in masterSection.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart, ref tableCounterInSection, ref imageCounterInSection, sectionNumber);
                    }

                    foreach (var subSection in masterSection.SubSections.OrderBy(x => x.Order))
                    {
                        body.Append(CreateSubHeading(subSection.Title, $"{sectionNumber}-{masterCounter}"));

                        foreach (var element in subSection.Elements.OrderBy(x => x.Order))
                        {
                            RenderElement(body, element, mainPart, ref tableCounterInSection, ref imageCounterInSection, sectionNumber);
                        }
                    }
                }

                // ===== Legacy Sections =====
                foreach (var section in document.Sections.OrderBy(x => x.Order))
                {
                    int legacyTableCounter = 0;
                    int legacyImageCounter = 0;

                    body.Append(CreateHeading(section.Title, "32"));
                    foreach (var element in section.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart, ref legacyTableCounter, ref legacyImageCounter, "0");
                    }
                }

                // ===== بخش‌های انتهای سند =====
                if (document.Attachments != null && document.Attachments.IsActive)
                {
                    int attachTableCounter = 0;
                    int attachImageCounter = 0;
                    RenderAttachmentsSection(body, document.Attachments, mainPart, ref attachTableCounter, ref attachImageCounter);
                }

                if (document.References != null && document.References.IsActive)
                {
                    int refTableCounter = 0;
                    int refImageCounter = 0;
                    RenderReferencesSection(body, document.References, mainPart, ref refTableCounter, ref refImageCounter);
                }

                if (document.Documents != null && document.Documents.IsActive)
                {
                    int docTableCounter = 0;
                    int docImageCounter = 0;
                    RenderDocumentsSection(body, document.Documents, mainPart, ref docTableCounter, ref docImageCounter);
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
                InitializeNumbering(mainPart);

                var body = new W.Body();

                // ========== Cover Page ==========
                if (request.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromDto(request.CoverPage));
                    int dummy1 = 0, dummy2 = 0;
                    foreach (var element in request.CoverPage.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart, ref dummy1, ref dummy2, "0");
                    }
                }

                // ===== پیش‌گفتار =====
                if (request.Preface != null && request.Preface.IsActive)
                {
                    RenderPrefaceSection(body, request.Preface, mainPart);
                }

                // ===== مفاهیم =====
                if (request.Concepts != null && request.Concepts.IsActive)
                {
                    RenderConceptsSection(body, request.Concepts, mainPart);
                }

                // ========== Table of Contents ==========
                if (request.IncludeTableOfContents)
                {
                    body.Append(CreateHeading("فهرست مطالب", "32"));
                    body.Append(CreateTableOfContents());
                    body.Append(new W.Paragraph(new W.Run(new W.Text(""))));

                    body.Append(CreateHeading("فهرست تصاویر", "32"));
                    body.Append(CreateTableOfFigures());
                    body.Append(new W.Paragraph(new W.Run(new W.Text(""))));

                    body.Append(CreateHeading("فهرست جداول", "32"));
                    body.Append(CreateTableOfTables());
                    body.Append(new W.Paragraph(new W.Run(new W.Text(""))));

                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
                }

                // ========== Master Sections ==========
                int masterCounter = 0;
                foreach (var masterSection in request.MasterSections.OrderBy(x => x.Order))
                {
                    masterCounter++;
                    string sectionNumber = masterCounter.ToString();

                    int tableCounterInSection = 0;
                    int imageCounterInSection = 0;

                    body.Append(CreateMasterHeading(masterSection.Title, sectionNumber));

                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

                    foreach (var element in masterSection.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart, ref tableCounterInSection, ref imageCounterInSection, sectionNumber);
                    }

                    foreach (var subSection in masterSection.SubSections.OrderBy(x => x.Order))
                    {
                        body.Append(CreateSubHeading(subSection.Title, $"{sectionNumber}-{masterCounter}"));

                        foreach (var element in subSection.Elements.OrderBy(x => x.Order))
                        {
                            RenderElement(body, element, mainPart, ref tableCounterInSection, ref imageCounterInSection, sectionNumber);
                        }
                    }
                }

                // ========== Legacy Sections ==========
                foreach (var section in request.Sections.OrderBy(x => x.Order))
                {
                    int legacyTableCounter = 0;
                    int legacyImageCounter = 0;

                    body.Append(CreateHeading(section.Title, "32"));
                    foreach (var element in section.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart, ref legacyTableCounter, ref legacyImageCounter, "0");
                    }
                }

                // ========== بخش‌های انتهای سند ==========
                if (request.Attachments != null && request.Attachments.IsActive)
                {
                    int attachTableCounter = 0;
                    int attachImageCounter = 0;
                    RenderAttachmentsSection(body, request.Attachments, mainPart, ref attachTableCounter, ref attachImageCounter);
                }

                if (request.References != null && request.References.IsActive)
                {
                    int refTableCounter = 0;
                    int refImageCounter = 0;
                    RenderReferencesSection(body, request.References, mainPart, ref refTableCounter, ref refImageCounter);
                }

                if (request.Documents != null && request.Documents.IsActive)
                {
                    int docTableCounter = 0;
                    int docImageCounter = 0;
                    RenderDocumentsSection(body, request.Documents, mainPart, ref docTableCounter, ref docImageCounter);
                }

                mainPart.Document.Append(body);

                if (request.PageHeader != null && request.PageHeader.IsActive)
                {
                    AddHeaderToDocument(mainPart, request.PageHeader);
                }

                mainPart.Document.Save();
            }

            return ms.ToArray();
        }
        #endregion

        private void AddEmptyLines(W.Body body, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                body.Append(new W.Paragraph());
            }
        }

        #region Initialize Numbering for Bullet Lists

        private void InitializeNumbering(MainDocumentPart mainPart)
        {
            var numberingPart = mainPart.NumberingDefinitionsPart;
            if (numberingPart == null)
            {
                numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
                numberingPart.Numbering = new W.Numbering();
                numberingPart.Numbering.Save();
            }

            var existingAbstractNum = numberingPart.Numbering
                .Elements<W.AbstractNum>()
                .FirstOrDefault(a => a.AbstractNumberId != null && a.AbstractNumberId.Value == 1);

            if (existingAbstractNum != null)
                return;

            var abstractNum = new W.AbstractNum() { AbstractNumberId = 1 };

            var multiLevelType = new W.MultiLevelType();
            multiLevelType.SetAttribute(new OpenXmlAttribute("val", null, "bullet"));
            abstractNum.Append(multiLevelType);

            var level0 = new W.Level() { LevelIndex = 0 };
            level0.Append(new W.StartNumberingValue() { Val = 1 });
            var numFormat0 = new W.NumberingFormat();
            numFormat0.SetAttribute(new OpenXmlAttribute("val", null, "bullet"));
            level0.Append(numFormat0);
            level0.Append(new W.LevelText() { Val = "❖" });
            level0.Append(new W.LevelJustification() { Val = W.LevelJustificationValues.Left });
            level0.Append(new W.ParagraphProperties(new W.Indentation() { Left = "720", Hanging = "360" }));
            level0.Append(new W.RunProperties(new W.RunFonts() { Ascii = "Segoe UI Symbol", HighAnsi = "Segoe UI Symbol", ComplexScript = "Segoe UI Symbol" }));
            abstractNum.Append(level0);

            var level1 = new W.Level() { LevelIndex = 1 };
            level1.Append(new W.StartNumberingValue() { Val = 1 });
            var numFormat1 = new W.NumberingFormat();
            numFormat1.SetAttribute(new OpenXmlAttribute("val", null, "bullet"));
            level1.Append(numFormat1);
            level1.Append(new W.LevelText() { Val = "o" });
            level1.Append(new W.LevelJustification() { Val = W.LevelJustificationValues.Left });
            level1.Append(new W.ParagraphProperties(new W.Indentation() { Left = "1440", Hanging = "360" }));
            level1.Append(new W.RunProperties(new W.RunFonts() { Ascii = "Symbol", HighAnsi = "Symbol", ComplexScript = "Symbol" }));
            abstractNum.Append(level1);

            var level2 = new W.Level() { LevelIndex = 2 };
            level2.Append(new W.StartNumberingValue() { Val = 1 });
            var numFormat2 = new W.NumberingFormat();
            numFormat2.SetAttribute(new OpenXmlAttribute("val", null, "bullet"));
            level2.Append(numFormat2);
            level2.Append(new W.LevelText() { Val = "▪" });
            level2.Append(new W.LevelJustification() { Val = W.LevelJustificationValues.Left });
            level2.Append(new W.ParagraphProperties(new W.Indentation() { Left = "2160", Hanging = "360" }));
            level2.Append(new W.RunProperties(new W.RunFonts() { Ascii = "Symbol", HighAnsi = "Symbol", ComplexScript = "Symbol" }));
            abstractNum.Append(level2);

            numberingPart.Numbering.Append(abstractNum);

            var num = new W.NumberingInstance() { NumberID = 1 };
            num.Append(new W.AbstractNumId() { Val = 1 });
            numberingPart.Numbering.Append(num);
            numberingPart.Numbering.Save();
        }

        #endregion

        #region Page Header

        private void AddHeaderToDocument(MainDocumentPart mainPart, PageHeaderDto pageHeader)
        {
            var headerPart = mainPart.AddNewPart<HeaderPart>();
            var header = new W.Header();

            var table = new W.Table();

            var tableProps = new W.TableProperties(
                new W.TableBorders(
                    new W.TopBorder { Val = W.BorderValues.Nil },
                    new W.BottomBorder { Val = W.BorderValues.Nil },
                    new W.LeftBorder { Val = W.BorderValues.Nil },
                    new W.RightBorder { Val = W.BorderValues.Nil },
                    new W.InsideHorizontalBorder { Val = W.BorderValues.Nil },
                    new W.InsideVerticalBorder { Val = W.BorderValues.Nil }
                ),
                new W.TableWidth { Width = "100%", Type = W.TableWidthUnitValues.Pct },
                new W.TableLayout { Type = W.TableLayoutValues.Autofit },
                new W.Justification { Val = W.JustificationValues.Center }
            );
            table.Append(tableProps);

            var row = new W.TableRow();
            row.Append(new W.TableRowProperties(
                new W.TableRowHeight { Val = 500, HeightType = W.HeightRuleValues.AtLeast }
            ));

            // ===== ستون چپ (لوگوها) =====
            var leftCell = new W.TableCell();
            var leftCellProps = new W.TableCellProperties(
                new W.TableCellWidth { Type = W.TableWidthUnitValues.Pct, Width = "50" },
                new W.TableCellVerticalAlignment { Val = W.TableVerticalAlignmentValues.Center },
                new W.Justification { Val = W.JustificationValues.Left }
            );
            leftCell.Append(leftCellProps);

            var leftParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "0", Before = "0" }
                )
            );

            var logos = pageHeader.Logos?.OrderBy(x => x.Order).ToList() ?? new List<HeaderLogoDto>();

            for (int i = 0; i < logos.Count; i++)
            {
                var logo = logos[i];

                var imageDto = new ImageItemDto
                {
                    FileName = logo.FileName,
                    Width = logo.Width > 0 ? logo.Width : 60,
                    Height = logo.Height > 0 ? logo.Height : 40,
                    ImageBase64 = logo.ImageBase64
                };
                InsertImageToHeader(leftParagraph, imageDto, mainPart, headerPart);

                if (i < logos.Count - 1)
                {
                    var spaceRun = new W.Run();
                    var spaceRunProps = new W.RunProperties();
                    spaceRun.AppendChild(spaceRunProps);

                    var spaceText = new W.Text("     ");
                    spaceText.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
                    spaceRun.AppendChild(spaceText);
                    leftParagraph.AppendChild(spaceRun);
                }
            }

            leftCell.Append(leftParagraph);
            row.Append(leftCell);

            // ===== ستون راست (متن هدر) =====
            var rightCell = new W.TableCell();
            var rightCellProps = new W.TableCellProperties(
                new W.TableCellWidth { Type = W.TableWidthUnitValues.Pct, Width = "50" },
                new W.TableCellVerticalAlignment { Val = W.TableVerticalAlignmentValues.Center },
                new W.Justification { Val = W.JustificationValues.Right }
            );
            rightCell.Append(rightCellProps);

            var rightParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification { Val = W.JustificationValues.Right },
                    new W.SpacingBetweenLines { After = "0", Before = "0" }
                )
            );

            if (!string.IsNullOrEmpty(pageHeader.HeaderText))
            {
                // ===== تشخیص فارسی/انگلیسی برای متن هدر =====
                var runs = CreateRunsForHeaderText(pageHeader.HeaderText, "22");
                foreach (var run in runs)
                {
                    rightParagraph.Append(run);
                }
            }

            rightCell.Append(rightParagraph);
            row.Append(rightCell);

            table.Append(row);
            header.Append(table);
            headerPart.Header = header;

            var headerReference = new W.HeaderReference
            {
                Id = mainPart.GetIdOfPart(headerPart),
                Type = W.HeaderFooterValues.Default
            };

            var sectionProperties = mainPart.Document.Descendants<W.SectionProperties>().FirstOrDefault();
            if (sectionProperties == null)
            {
                sectionProperties = new W.SectionProperties();
                var pageMargin = new W.PageMargin
                {
                    Top = 1440,
                    Bottom = 1440,
                    Left = 1440,
                    Right = 1440,
                    Header = 720,
                    Footer = 720
                };
                sectionProperties.Append(pageMargin);
                mainPart.Document.Append(sectionProperties);
            }

            sectionProperties.PrependChild(headerReference);
        }

        private List<W.Run> CreateRunsForHeaderText(string text, string fontSize)
        {
            var runs = new List<W.Run>();
            if (string.IsNullOrEmpty(text))
            {
                runs.Add(new W.Run(new W.Text(" ")));
                return runs;
            }

            var current = new List<char>();
            bool? currentIsPersian = null;
            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                    currentIsPersian = isPersian;

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                        runs.Add(CreateRunForHeaderText(new string(current.ToArray()), currentIsPersian.Value, fontSize));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
                runs.Add(CreateRunForHeaderText(new string(current.ToArray()), currentIsPersian ?? false, fontSize));

            if (runs.Count == 0)
                runs.Add(new W.Run(new W.Text(" ")));

            return runs;
        }

        private W.Run CreateRunForHeaderText(string text, bool isPersian, string fontSize)
        {
            var fontName = isPersian ? PersianFont : EnglishFont;

            var runProperties = new W.RunProperties(
                new W.RunFonts
                {
                    Ascii = fontName,
                    HighAnsi = fontName,
                    ComplexScript = fontName
                },
                new W.FontSize { Val = fontSize }
            );

            if (isPersian)
                runProperties.Append(new W.Bold());

            var run = new W.Run(runProperties);
            var textElement = new W.Text(text);
            textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
            run.Append(textElement);

            return run;
        }

        private void InsertImageToHeader(W.Paragraph paragraph, ImageItemDto imageDto, MainDocumentPart mainPart, HeaderPart headerPart)
        {
            if (string.IsNullOrEmpty(imageDto.ImageBase64))
                return;

            try
            {
                Console.WriteLine($"=== Inserting Header Image: {imageDto.FileName} ===");
                Console.WriteLine($"Base64 Length: {imageDto.ImageBase64?.Length ?? 0}");

                var imageBytes = Convert.FromBase64String(imageDto.ImageBase64);
                Console.WriteLine($"Image Bytes Length: {imageBytes.Length}");

                if (imageBytes == null || imageBytes.Length == 0)
                {
                    Console.WriteLine("ERROR: Image bytes is null or empty");
                    return;
                }

                var imagePartType = DetectImagePartType(imageBytes);
                Console.WriteLine($"Image Type: {imagePartType}");

                var imagePart = headerPart.AddImagePart(imagePartType);
                using (var stream = new MemoryStream(imageBytes))
                {
                    imagePart.FeedData(stream);
                }
                var imagePartId = headerPart.GetIdOfPart(imagePart);
                Console.WriteLine($"Image Part ID: {imagePartId}");

                if (string.IsNullOrEmpty(imagePartId))
                {
                    Console.WriteLine("ERROR: ImagePartId is null or empty");
                    return;
                }

                long cx, cy;

                if (imageDto.Width <= 0)
                {
                    imageDto.Width = 60;
                    imageDto.Height = (int)(imageDto.Width * 0.75);
                }

                cx = (long)(imageDto.Width * 9525);
                cy = (long)(imageDto.Height * 9525);

                long maxSize = 200 * 9525;
                if (cx > maxSize)
                {
                    cy = (long)(cy * ((double)maxSize / cx));
                    cx = maxSize;
                }
                if (cy > maxSize)
                {
                    cx = (long)(cx * ((double)maxSize / cy));
                    cy = maxSize;
                }

                if (cx < 1) cx = 9525;
                if (cy < 1) cy = 9525;

                uint uniqueId = _imageId++;
                Console.WriteLine($"Unique Image ID: {uniqueId}");

                var imageRun = new W.Run();

                var runProps = new W.RunProperties();
                runProps.AppendChild(new W.Spacing() { Val = 40 });
                imageRun.AppendChild(runProps);

                var drawing = new W.Drawing();

                var inline = new DW.Inline();
                inline.AppendChild(new DW.Extent() { Cx = cx, Cy = cy });
                inline.AppendChild(new DW.EffectExtent()
                {
                    LeftEdge = 0L,
                    TopEdge = 0L,
                    RightEdge = 100000L,
                    BottomEdge = 0L
                });
                inline.AppendChild(new DW.DocProperties()
                {
                    Id = (UInt32Value)uniqueId,
                    Name = imageDto.FileName ?? $"Logo_{uniqueId}",
                    Description = ""
                });

                var nvGraphicFramePr = new DW.NonVisualGraphicFrameDrawingProperties();
                nvGraphicFramePr.AppendChild(new D.GraphicFrameLocks() { NoChangeAspect = true });
                inline.AppendChild(nvGraphicFramePr);

                var graphic = new D.Graphic();
                var graphicData = new D.GraphicData()
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"
                };

                var picture = new PIC.Picture();

                var nvPicPr = new PIC.NonVisualPictureProperties();
                nvPicPr.AppendChild(new PIC.NonVisualDrawingProperties()
                {
                    Id = (UInt32Value)0U,
                    Name = imageDto.FileName ?? $"Logo_{uniqueId}",
                    Description = ""
                });
                nvPicPr.AppendChild(new PIC.NonVisualPictureDrawingProperties());
                picture.AppendChild(nvPicPr);

                var blipFill = new PIC.BlipFill();
                blipFill.AppendChild(new D.Blip() { Embed = imagePartId });
                blipFill.AppendChild(new D.Stretch(new D.FillRectangle()));
                picture.AppendChild(blipFill);

                var spPr = new PIC.ShapeProperties();
                var xfrm = new D.Transform2D();
                xfrm.AppendChild(new D.Offset() { X = 0L, Y = 0L });
                xfrm.AppendChild(new D.Extents() { Cx = cx, Cy = cy });
                spPr.AppendChild(xfrm);
                spPr.AppendChild(new D.PresetGeometry(new D.AdjustValueList())
                {
                    Preset = D.ShapeTypeValues.Rectangle
                });
                picture.AppendChild(spPr);

                graphicData.AppendChild(picture);
                graphic.AppendChild(graphicData);
                inline.AppendChild(graphic);

                drawing.AppendChild(inline);
                imageRun.AppendChild(drawing);
                paragraph.AppendChild(imageRun);

                Console.WriteLine($"✅ Header Image inserted successfully: {imageDto.FileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in InsertImageToHeader: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                var errorRun = new W.Run(
                    new W.RunProperties(new W.FontSize { Val = "16" }, new W.Color { Val = "FF6600" }),
                    new W.Text($"[{imageDto.FileName ?? "لوگو"}]")
                );
                paragraph.AppendChild(errorRun);
            }
        }
        #endregion

        #region Element Rendering Methods

        private void RenderElement(W.Body body, ContentElementDto element, MainDocumentPart mainPart,
            ref int tableCounter, ref int imageCounter, string sectionNumber = "1")
        {
            switch (element.Type?.ToLower())
            {
                case "paragraph":
                    if (!string.IsNullOrEmpty(element.Text))
                    {
                        var paragraphs = CreateParagraphsFromText(element.Text);
                        foreach (var paragraph in paragraphs)
                        {
                            body.Append(paragraph);
                        }
                    }
                    break;

                case "image":
                    if (element.Image != null)
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, element.Image, mainPart);
                        body.Append(imageParagraph);

                        imageCounter++;
                        if (!string.IsNullOrEmpty(element.Image.Caption))
                        {
                            var caption = CreateImageCaption(element.Image.Caption, sectionNumber, imageCounter);
                            if (caption != null)
                                body.Append(caption);
                        }
                    }
                    break;

                case "table":
                    if (element.Table != null)
                    {
                        tableCounter++;
                        if (!string.IsNullOrEmpty(element.Table.Title))
                        {
                            var caption = CreateTableCaption(element.Table.Title, sectionNumber, tableCounter);
                            body.Append(caption);
                        }

                        var table = CreateTableFromDto(element.Table);
                        body.Append(table);
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
                    }
                    break;

                case "bulletlist":
                    if (element.BulletList != null)
                    {
                        var bulletParagraphs = CreateBulletList(element.BulletList);
                        foreach (var paragraph in bulletParagraphs)
                        {
                            body.Append(paragraph);
                        }
                    }
                    break;
            }
        }

        private void RenderElementFromEntity(W.Body body, ContentElement element, MainDocumentPart mainPart,
            ref int tableCounter, ref int imageCounter, string sectionNumber = "1")
        {
            switch (element.Type)
            {
                case ContentElementType.Paragraph:
                    if (!string.IsNullOrEmpty(element.ParagraphText))
                        body.Append(CreateParagraph(element.ParagraphText));
                    break;

                case ContentElementType.Image:
                    if (element.Image != null)
                    {
                        var imageDto = MapImageToDto(element.Image);
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, imageDto, mainPart);
                        body.Append(imageParagraph);

                        imageCounter++;
                        if (!string.IsNullOrEmpty(imageDto.Caption))
                        {
                            var caption = CreateImageCaption(imageDto.Caption, sectionNumber, imageCounter);
                            if (caption != null)
                                body.Append(caption);
                        }
                    }
                    break;

                case ContentElementType.Table:
                    if (element.Table != null)
                    {
                        var tableDto = MapTableToDto(element.Table);

                        tableCounter++;
                        if (!string.IsNullOrEmpty(tableDto.Title))
                        {
                            var caption = CreateTableCaption(tableDto.Title, sectionNumber, tableCounter);
                            body.Append(caption);
                        }

                        var table = CreateTableFromDto(tableDto);
                        body.Append(table);
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
                    }
                    break;

                case ContentElementType.BulletList:
                    if (element.BulletList != null)
                    {
                        var bulletListDto = MapBulletListToDto(element.BulletList);
                        var bulletParagraphs = CreateBulletList(bulletListDto);
                        foreach (var paragraph in bulletParagraphs)
                        {
                            body.Append(paragraph);
                        }
                    }
                    break;
            }
        }

        private void RenderElementWithoutCaption(W.Body body, ContentElementDto element, MainDocumentPart mainPart)
        {
            switch (element.Type?.ToLower())
            {
                case "paragraph":
                    if (!string.IsNullOrEmpty(element.Text))
                        body.Append(CreateParagraph(element.Text));
                    break;

                case "image":
                    if (element.Image != null)
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Center },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, element.Image, mainPart);
                        body.Append(imageParagraph);
                    }
                    break;

                case "table":
                    if (element.Table != null)
                    {
                        var table = CreateTableFromDto(element.Table);
                        body.Append(table);
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
                    }
                    break;

                case "bulletlist":
                    if (element.BulletList != null)
                    {
                        var bulletParagraphs = CreateBulletList(element.BulletList);
                        foreach (var paragraph in bulletParagraphs)
                        {
                            body.Append(paragraph);
                        }
                    }
                    break;
            }
        }
        #endregion

        #region Caption Methods

        private W.Paragraph CreateTableCaption(string title, string sectionNumber, int tableNumberInSection)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Center },
                    new W.SpacingBetweenLines { After = "120", Before = "120" }
                )
            );

            var fixedTitle = title;

            // ===== فیلد TC برای ثبت در فهرست جداول =====
            var tcRun = new W.Run();
            var tcFieldChar1 = new W.FieldChar { FieldCharType = W.FieldCharValues.Begin };
            tcRun.Append(tcFieldChar1);
            var tcFieldCode = new W.FieldCode
            {
                Text = $"TC \"{fixedTitle}\" \\f Table \\l 1"
            };
            tcRun.Append(tcFieldCode);
            var tcFieldChar2 = new W.FieldChar { FieldCharType = W.FieldCharValues.Separate };
            tcRun.Append(tcFieldChar2);
            tcRun.Append(new W.Text(""));
            var tcFieldChar3 = new W.FieldChar { FieldCharType = W.FieldCharValues.End };
            tcRun.Append(tcFieldChar3);
            paragraph.Append(tcRun);

            // ===== متن عنوان با تشخیص فارسی/انگلیسی =====
            var runs = CreateRunsForCaption(fixedTitle, "24");
            foreach (var run in runs)
            {
                paragraph.Append(run);
            }

            return paragraph;
        }

        private W.Paragraph CreateImageCaption(string caption, string sectionNumber, int imageNumberInSection)
        {
            if (string.IsNullOrEmpty(caption))
                return null;

            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Center },
                    new W.SpacingBetweenLines { After = "120", Before = "120" }
                )
            );

            var fixedCaption = caption;

            // ===== فیلد TC برای ثبت در فهرست تصاویر =====
            var tcRun = new W.Run();
            var tcFieldChar1 = new W.FieldChar { FieldCharType = W.FieldCharValues.Begin };
            tcRun.Append(tcFieldChar1);
            var tcFieldCode = new W.FieldCode
            {
                Text = $"TC \"{fixedCaption}\" \\f Figure \\l 1"
            };
            tcRun.Append(tcFieldCode);
            var tcFieldChar2 = new W.FieldChar { FieldCharType = W.FieldCharValues.Separate };
            tcRun.Append(tcFieldChar2);
            tcRun.Append(new W.Text(""));
            var tcFieldChar3 = new W.FieldChar { FieldCharType = W.FieldCharValues.End };
            tcRun.Append(tcFieldChar3);
            paragraph.Append(tcRun);

            // ===== متن کپشن با تشخیص فارسی/انگلیسی =====
            var runs = CreateRunsForCaption(fixedCaption, "24");
            foreach (var run in runs)
            {
                paragraph.Append(run);
            }

            return paragraph;
        }

        private List<W.Run> CreateRunsForCaption(string text, string fontSize)
        {
            var runs = new List<W.Run>();
            if (string.IsNullOrEmpty(text))
            {
                runs.Add(new W.Run(new W.Text(" ")));
                return runs;
            }

            var current = new List<char>();
            bool? currentIsPersian = null;
            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                    currentIsPersian = isPersian;

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                        runs.Add(CreateCaptionRun(new string(current.ToArray()), currentIsPersian.Value, fontSize));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
                runs.Add(CreateCaptionRun(new string(current.ToArray()), currentIsPersian ?? false, fontSize));

            if (runs.Count == 0)
                runs.Add(new W.Run(new W.Text(" ")));

            return runs;
        }

        private W.Run CreateCaptionRun(string text, bool isPersian, string fontSize)
        {
            var fontName = isPersian ? PersianFont : EnglishFont;

            var runProperties = new W.RunProperties(
                new W.RunFonts
                {
                    Ascii = fontName,
                    HighAnsi = fontName,
                    ComplexScript = fontName
                },
                new W.FontSize { Val = fontSize }
            );

            var run = new W.Run(runProperties);
            var textElement = new W.Text(text);
            textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
            run.Append(textElement);

            return run;
        }

        private string ReverseNumbersInText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var pattern = @"(\d+-\d+(?:-\d+)*\-?)";
            var matches = Regex.Matches(text, pattern);

            if (matches.Count == 0)
                return text;

            var result = text;
            foreach (Match match in matches)
            {
                var numberPart = match.Groups[1].Value;

                var parts = numberPart.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    Array.Reverse(parts);
                    var reversedNumber = string.Join("-", parts);
                    if (numberPart.EndsWith("-"))
                        reversedNumber += "-";

                    result = result.Replace(numberPart, reversedNumber);
                }
            }

            return result;
        }
        #endregion

        #region Table of Figures and Tables

        private W.Paragraph CreateTableOfFigures()
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
            var fieldChar1 = new W.FieldChar { FieldCharType = W.FieldCharValues.Begin };
            var fieldCode = new W.FieldCode { Text = "TOC \\f Figure \\h \\* MERGEFORMAT" };
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

        private W.Paragraph CreateTableOfTables()
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
            var fieldChar1 = new W.FieldChar { FieldCharType = W.FieldCharValues.Begin };
            var fieldCode = new W.FieldCode { Text = "TOC \\f Table \\h \\* MERGEFORMAT" };
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

        #region Preface and Concepts Sections

        private void RenderPrefaceSection(W.Body body, PrefaceSectionDto preface, MainDocumentPart mainPart)
        {
            if (preface == null || !preface.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.ParagraphStyleId() { Val = "Heading1" },
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                )
            );

            var titleRuns = CreateRunsForHeading(preface.Title, MasterHeaderFontSize, true);
            foreach (var run in titleRuns)
            {
                titleParagraph.Append(run);
            }
            body.Append(titleParagraph);

            foreach (var element in preface.Elements.OrderBy(x => x.Order))
            {
                RenderElementWithoutCaption(body, element, mainPart);
            }
        }

        private void RenderConceptsSection(W.Body body, ConceptsSectionDto concepts, MainDocumentPart mainPart)
        {
            if (concepts == null || !concepts.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.ParagraphStyleId() { Val = "Heading1" },
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                )
            );

            var titleRuns = CreateRunsForHeading(concepts.Title, MasterHeaderFontSize, true);
            foreach (var run in titleRuns)
            {
                titleParagraph.Append(run);
            }
            body.Append(titleParagraph);

            foreach (var element in concepts.Elements.OrderBy(x => x.Order))
            {
                RenderElementWithoutCaption(body, element, mainPart);
            }

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
        }

        #endregion

        #region Final Sections

        private void RenderAttachmentsSection(W.Body body, AttachmentSectionDto attachments, MainDocumentPart mainPart, ref int tableCounter, ref int imageCounter)
        {
            if (attachments == null || !attachments.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                )
            );

            var titleRuns = CreateRunsForHeading(attachments.Title, MasterHeaderFontSize, true);
            foreach (var run in titleRuns)
            {
                titleParagraph.Append(run);
            }
            body.Append(titleParagraph);

            string sectionNumber = "الف";
            foreach (var element in attachments.Elements.OrderBy(x => x.Order))
            {
                RenderElement(body, element, mainPart, ref tableCounter, ref imageCounter, sectionNumber);
            }
        }

        private void RenderAttachmentsSectionFromEntity(W.Body body, AttachmentSection attachments, MainDocumentPart mainPart, ref int tableCounter, ref int imageCounter)
        {
            if (attachments == null || !attachments.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                )
            );

            var titleRuns = CreateRunsForHeading(attachments.Title, MasterHeaderFontSize, true);
            foreach (var run in titleRuns)
            {
                titleParagraph.Append(run);
            }
            body.Append(titleParagraph);

            string sectionNumber = "الف";
            foreach (var element in attachments.Elements.OrderBy(x => x.Order))
            {
                RenderElementFromEntity(body, element, mainPart, ref tableCounter, ref imageCounter, sectionNumber);
            }
        }

        private void RenderReferencesSection(W.Body body, ReferenceSectionDto references, MainDocumentPart mainPart, ref int tableCounter, ref int imageCounter)
        {
            if (references == null || !references.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                )
            );

            var titleRuns = CreateRunsForHeading(references.Title, MasterHeaderFontSize, true);
            foreach (var run in titleRuns)
            {
                titleParagraph.Append(run);
            }
            body.Append(titleParagraph);

            string sectionNumber = "ب";
            foreach (var element in references.Elements.OrderBy(x => x.Order))
            {
                RenderElementWithLeftAlignment(body, element, mainPart, ref tableCounter, ref imageCounter, sectionNumber);
            }
        }

        private void RenderReferencesSectionFromEntity(W.Body body, ReferenceSection references, MainDocumentPart mainPart, ref int tableCounter, ref int imageCounter)
        {
            if (references == null || !references.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                )
            );

            var titleRuns = CreateRunsForHeading(references.Title, MasterHeaderFontSize, true);
            foreach (var run in titleRuns)
            {
                titleParagraph.Append(run);
            }
            body.Append(titleParagraph);

            string sectionNumber = "ب";
            foreach (var element in references.Elements.OrderBy(x => x.Order))
            {
                var elementDto = MapContentElementToDto(element);
                RenderElementWithLeftAlignment(body, elementDto, mainPart, ref tableCounter, ref imageCounter, sectionNumber);
            }
        }

        private void RenderElementWithLeftAlignment(W.Body body, ContentElementDto element, MainDocumentPart mainPart,
            ref int tableCounter, ref int imageCounter, string sectionNumber = "1")
        {
            switch (element.Type?.ToLower())
            {
                case "paragraph":
                    if (!string.IsNullOrEmpty(element.Text))
                        body.Append(CreateLeftAlignedParagraph(element.Text));
                    break;

                case "bulletlist":
                    if (element.BulletList != null)
                    {
                        var bulletParagraphs = CreateLeftAlignedBulletList(element.BulletList);
                        foreach (var paragraph in bulletParagraphs)
                        {
                            body.Append(paragraph);
                        }
                    }
                    break;

                case "table":
                    if (element.Table != null)
                    {
                        tableCounter++;
                        if (!string.IsNullOrEmpty(element.Table.Title))
                        {
                            var caption = CreateTableCaption(element.Table.Title, sectionNumber, tableCounter);
                            body.Append(caption);
                        }

                        var table = CreateLeftAlignedTableFromDto(element.Table);
                        body.Append(table);
                        body.Append(new W.Paragraph(new W.Run(new W.Break())));
                    }
                    break;

                case "image":
                    if (element.Image != null)
                    {
                        var imageParagraph = new W.Paragraph(
                            new W.ParagraphProperties(
                                new W.Justification { Val = W.JustificationValues.Left },
                                new W.SpacingBetweenLines { After = "200" }
                            )
                        );
                        InsertImageToParagraph(imageParagraph, element.Image, mainPart);
                        body.Append(imageParagraph);

                        imageCounter++;
                        if (!string.IsNullOrEmpty(element.Image.Caption))
                        {
                            var caption = CreateImageCaption(element.Image.Caption, sectionNumber, imageCounter);
                            if (caption != null)
                                body.Append(caption);
                        }
                    }
                    break;
            }
        }

        private W.Paragraph CreateLeftAlignedParagraph(string text)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.Indentation() { FirstLine = "397" },
                    new W.SpacingBetweenLines() { Line = "360", LineRule = W.LineSpacingRuleValues.Auto }
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
                    currentIsPersian = isPersian;

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                    {
                        var runText = new string(current.ToArray());
                        bool isPersianRun = currentIsPersian.Value;
                        var run = new W.Run(
                            new W.RunProperties(
                                new W.RunFonts()
                                {
                                    Ascii = EnglishFont,
                                    HighAnsi = EnglishFont,
                                    ComplexScript = isPersianRun ? PersianFont : EnglishFont
                                },
                                new W.FontSize() { Val = EnglishFontSize }
                            ),
                            new W.Text(runText)
                        );
                        runs.Add(run);
                    }
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
            {
                var runText = new string(current.ToArray());
                bool isPersianRun = currentIsPersian ?? false;
                var run = new W.Run(
                    new W.RunProperties(
                        new W.RunFonts()
                        {
                            Ascii = EnglishFont,
                            HighAnsi = EnglishFont,
                            ComplexScript = isPersianRun ? PersianFont : EnglishFont
                        },
                        new W.FontSize() { Val = EnglishFontSize }
                    ),
                    new W.Text(runText)
                );
                runs.Add(run);
            }

            paragraph.Append(runs);
            return paragraph;
        }

        private W.Paragraph[] CreateLeftAlignedBulletList(BulletListDto bulletList)
        {
            if (bulletList == null || bulletList.Items == null || !bulletList.Items.Any())
                return new W.Paragraph[0];

            var paragraphs = new List<W.Paragraph>();

            if (!string.IsNullOrEmpty(bulletList.Title))
            {
                var titleParagraph = new W.Paragraph(
                    new W.ParagraphProperties(
                        new W.Justification() { Val = W.JustificationValues.Left },
                        new W.SpacingBetweenLines { After = "120" }
                    )
                );

                var titleRuns = CreateRunsForLeftAlignedBulletText(bulletList.Title);
                foreach (var run in titleRuns)
                {
                    titleParagraph.Append(run);
                }
                paragraphs.Add(titleParagraph);
            }

            foreach (var item in bulletList.Items.OrderBy(x => x.Order))
            {
                var paragraph = CreateLeftAlignedBulletListParagraph(item.Text, item.Level);
                paragraphs.Add(paragraph);
            }

            var spacingParagraph = new W.Paragraph(
                new W.ParagraphProperties(new W.SpacingBetweenLines { After = "200" })
            );
            paragraphs.Add(spacingParagraph);

            return paragraphs.ToArray();
        }

        private W.Paragraph CreateLeftAlignedBulletListParagraph(string text, int level = 0)
        {
            int safeLevel = Math.Clamp(level, 0, 2);

            var numberingProps = new W.NumberingProperties();
            numberingProps.Append(new W.NumberingLevelReference() { Val = safeLevel });
            numberingProps.Append(new W.NumberingId() { Val = 1 });

            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification() { Val = W.JustificationValues.Left },
                    numberingProps,
                    new W.Indentation() { FirstLine = "397", Left = (safeLevel * 720).ToString() },
                    new W.SpacingBetweenLines() { Line = "360", LineRule = W.LineSpacingRuleValues.Auto, After = "0" }
                )
            );

            var runs = CreateRunsForLeftAlignedBulletText(text);
            foreach (var run in runs)
            {
                paragraph.Append(run);
            }

            return paragraph;
        }

        private List<W.Run> CreateRunsForLeftAlignedBulletText(string text)
        {
            var runs = new List<W.Run>();
            if (string.IsNullOrEmpty(text))
            {
                var run = new W.Run(
                    new W.RunProperties(
                        new W.RunFonts() { Ascii = EnglishFont, HighAnsi = EnglishFont, ComplexScript = EnglishFont },
                        new W.FontSize() { Val = EnglishFontSize }
                    ),
                    new W.Text(" ")
                );
                runs.Add(run);
                return runs;
            }

            var current = new List<char>();
            bool? currentIsPersian = null;
            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                    currentIsPersian = isPersian;

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                    {
                        var runText = new string(current.ToArray());
                        bool isPersianRun = currentIsPersian.Value;
                        var fontName = isPersianRun ? PersianFont : EnglishFont;
                        var run = new W.Run(
                            new W.RunProperties(
                                new W.RunFonts()
                                {
                                    Ascii = EnglishFont,
                                    HighAnsi = EnglishFont,
                                    ComplexScript = fontName
                                },
                                new W.FontSize() { Val = EnglishFontSize }
                            ),
                            new W.Text(runText)
                        );
                        runs.Add(run);
                    }
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
            {
                var runText = new string(current.ToArray());
                bool isPersianRun = currentIsPersian ?? false;
                var fontName = isPersianRun ? PersianFont : EnglishFont;
                var run = new W.Run(
                    new W.RunProperties(
                        new W.RunFonts()
                        {
                            Ascii = EnglishFont,
                            HighAnsi = EnglishFont,
                            ComplexScript = fontName
                        },
                        new W.FontSize() { Val = EnglishFontSize }
                    ),
                    new W.Text(runText)
                );
                runs.Add(run);
            }

            return runs;
        }

        private W.Table CreateLeftAlignedTableFromDto(TableDataDto tableData)
        {
            var table = CreateTableFromDto(tableData);

            var tableProps = table.GetFirstChild<W.TableProperties>();
            if (tableProps != null)
            {
                var justification = tableProps.GetFirstChild<W.Justification>();
                if (justification != null)
                    justification.Val = W.JustificationValues.Left;
                else
                    tableProps.Append(new W.Justification { Val = W.JustificationValues.Left });
            }

            foreach (var row in table.Descendants<W.TableRow>())
            {
                foreach (var cell in row.Descendants<W.TableCell>())
                {
                    var para = cell.GetFirstChild<W.Paragraph>();
                    if (para != null)
                    {
                        var paraProps = para.GetFirstChild<W.ParagraphProperties>();
                        if (paraProps != null)
                        {
                            var justification = paraProps.GetFirstChild<W.Justification>();
                            if (justification != null)
                                justification.Val = W.JustificationValues.Left;
                            else
                                paraProps.Append(new W.Justification { Val = W.JustificationValues.Left });

                            var bidi = paraProps.GetFirstChild<W.BiDi>();
                            if (bidi != null)
                                bidi.Remove();
                        }
                    }
                }
            }

            return table;
        }

        private void RenderDocumentsSection(W.Body body, DocumentSectionDto documents, MainDocumentPart mainPart, ref int tableCounter, ref int imageCounter)
        {
            if (documents == null || !documents.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                )
            );

            var titleRuns = CreateRunsForHeading(documents.Title, MasterHeaderFontSize, true);
            foreach (var run in titleRuns)
            {
                titleParagraph.Append(run);
            }
            body.Append(titleParagraph);

            string sectionNumber = "پ";
            foreach (var element in documents.Elements.OrderBy(x => x.Order))
            {
                RenderElement(body, element, mainPart, ref tableCounter, ref imageCounter, sectionNumber);
            }
        }

        private void RenderDocumentsSectionFromEntity(W.Body body, DocumentSection documents, MainDocumentPart mainPart, ref int tableCounter, ref int imageCounter)
        {
            if (documents == null || !documents.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                )
            );

            var titleRuns = CreateRunsForHeading(documents.Title, MasterHeaderFontSize, true);
            foreach (var run in titleRuns)
            {
                titleParagraph.Append(run);
            }
            body.Append(titleParagraph);

            string sectionNumber = "پ";
            foreach (var element in documents.Elements.OrderBy(x => x.Order))
            {
                RenderElementFromEntity(body, element, mainPart, ref tableCounter, ref imageCounter, sectionNumber);
            }
        }

        private ContentElementDto MapContentElementToDto(ContentElement element)
        {
            return new ContentElementDto
            {
                Id = element.Id,
                Type = element.Type.ToString().ToLower(),
                Order = element.Order,
                Text = element.ParagraphText,
                Image = element.Image != null ? MapImageToDto(element.Image) : null,
                Table = element.Table != null ? MapTableToDto(element.Table) : null,
                BulletList = element.BulletList != null ? MapBulletListToDto(element.BulletList) : null
            };
        }

        #endregion

        #region Bullet List Methods

        private BulletListDto MapBulletListToDto(BulletList bulletList)
        {
            if (bulletList == null) return null;

            return new BulletListDto
            {
                Id = bulletList.Id,
                Title = bulletList.Title,
                Order = bulletList.Order,
                Items = bulletList.Items?.OrderBy(x => x.Order).Select(x => new BulletListItemDto
                {
                    Id = x.Id,
                    Text = x.Text,
                    Order = x.Order,
                    Level = x.Level
                }).ToList() ?? new List<BulletListItemDto>()
            };
        }

        private W.Paragraph[] CreateBulletList(BulletListDto bulletList)
        {
            if (bulletList == null || bulletList.Items == null || !bulletList.Items.Any())
                return new W.Paragraph[0];

            var paragraphs = new List<W.Paragraph>();

            if (!string.IsNullOrEmpty(bulletList.Title))
            {
                var titleParagraph = new W.Paragraph(
                    new W.ParagraphProperties(
                        new W.BiDi(),
                        new W.Justification() { Val = W.JustificationValues.Left },
                        new W.SpacingBetweenLines { After = "120" }
                    )
                );

                var titleRuns = CreateRunsForBulletTitle(bulletList.Title);
                foreach (var run in titleRuns)
                {
                    titleParagraph.Append(run);
                }
                paragraphs.Add(titleParagraph);
            }

            foreach (var item in bulletList.Items.OrderBy(x => x.Order))
            {
                var paragraph = CreateBulletListParagraph(item.Text, item.Level);
                paragraphs.Add(paragraph);
            }

            var spacingParagraph = new W.Paragraph(
                new W.ParagraphProperties(new W.SpacingBetweenLines { After = "200" })
            );
            paragraphs.Add(spacingParagraph);

            return paragraphs.ToArray();
        }

        private List<W.Run> CreateRunsForBulletTitle(string text)
        {
            var runs = new List<W.Run>();
            if (string.IsNullOrEmpty(text))
            {
                runs.Add(new W.Run(new W.Text(" ")));
                return runs;
            }

            var current = new List<char>();
            bool? currentIsPersian = null;
            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                    currentIsPersian = isPersian;

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                        runs.Add(CreateBulletTitleRun(new string(current.ToArray()), currentIsPersian.Value));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
                runs.Add(CreateBulletTitleRun(new string(current.ToArray()), currentIsPersian ?? false));

            if (runs.Count == 0)
                runs.Add(new W.Run(new W.Text(" ")));

            return runs;
        }

        private W.Run CreateBulletTitleRun(string text, bool isPersian)
        {
            var fontName = isPersian ? PersianFont : EnglishFont;
            var fontSize = isPersian ? "28" : "28";

            var runProperties = new W.RunProperties(
                new W.RunFonts
                {
                    Ascii = fontName,
                    HighAnsi = fontName,
                    ComplexScript = fontName
                },
                new W.FontSize { Val = fontSize },
                new W.Bold()
            );

            var run = new W.Run(runProperties);
            var textElement = new W.Text(text);
            textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
            run.Append(textElement);

            return run;
        }

        private W.Paragraph CreateBulletListParagraph(string text, int level = 0)
        {
            int safeLevel = Math.Clamp(level, 0, 2);

            var numberingProps = new W.NumberingProperties();
            numberingProps.Append(new W.NumberingLevelReference() { Val = safeLevel });
            numberingProps.Append(new W.NumberingId() { Val = 1 });

            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    numberingProps,
                    new W.Indentation() { FirstLine = "397", Left = (safeLevel * 720).ToString() },
                    new W.SpacingBetweenLines() { Line = "360", LineRule = W.LineSpacingRuleValues.Auto, After = "0" }
                )
            );

            var runs = CreateRunsForBulletText(text);
            foreach (var run in runs)
            {
                paragraph.Append(run);
            }

            return paragraph;
        }

        private List<W.Run> CreateRunsForBulletText(string text)
        {
            var runs = new List<W.Run>();
            if (string.IsNullOrEmpty(text))
            {
                runs.Add(CreateBulletTextRun(" ", false));
                return runs;
            }

            var current = new List<char>();
            bool? currentIsPersian = null;
            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                    currentIsPersian = isPersian;

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                        runs.Add(CreateBulletTextRun(new string(current.ToArray()), currentIsPersian.Value));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
                runs.Add(CreateBulletTextRun(new string(current.ToArray()), currentIsPersian ?? false));

            if (runs.Count == 0)
                runs.Add(CreateBulletTextRun(" ", false));

            return runs;
        }

        private W.Run CreateBulletTextRun(string text, bool isPersian)
        {
            var fontName = isPersian ? PersianFont : EnglishFont;
            var fontSize = isPersian ? PersianFontSize : EnglishFontSize;

            var runProperties = new W.RunProperties(
                new W.RunFonts()
                {
                    Ascii = fontName,
                    HighAnsi = fontName,
                    ComplexScript = fontName
                },
                new W.FontSize() { Val = fontSize }
            );

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

        #endregion

        #region Image Methods

        private uint _imageId = 1;

        private PartTypeInfo DetectImagePartType(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length < 4)
                return ImagePartType.Jpeg;

            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 &&
                imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
            {
                return ImagePartType.Png;
            }
            else if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
            {
                return ImagePartType.Jpeg;
            }
            else if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 &&
                     imageBytes[2] == 0x46 && imageBytes[3] == 0x38)
            {
                return ImagePartType.Gif;
            }
            else if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
            {
                return ImagePartType.Bmp;
            }

            return ImagePartType.Jpeg;
        }

        private void InsertImageToParagraph(W.Paragraph paragraph, ImageItemDto imageDto, MainDocumentPart mainPart)
        {
            if (string.IsNullOrEmpty(imageDto.ImageBase64))
                return;

            try
            {
                var imageBytes = Convert.FromBase64String(imageDto.ImageBase64);

                if (imageBytes == null || imageBytes.Length == 0)
                {
                    return;
                }

                var imagePartType = DetectImagePartType(imageBytes);

                var imagePart = mainPart.AddImagePart(imagePartType);
                using (var stream = new MemoryStream(imageBytes))
                {
                    imagePart.FeedData(stream);
                }

                var imagePartId = mainPart.GetIdOfPart(imagePart);

                if (string.IsNullOrEmpty(imagePartId))
                {
                    return;
                }

                long cx, cy;

                if (imageDto.Width <= 0)
                {
                    imageDto.Width = 400;
                    imageDto.Height = (int)(imageDto.Width * 0.75);
                }

                cx = (long)(imageDto.Width * 9525);
                cy = (long)(imageDto.Height * 9525);

                long maxSize = 800 * 9525;
                if (cx > maxSize)
                {
                    cy = (long)(cy * ((double)maxSize / cx));
                    cx = maxSize;
                }
                if (cy > maxSize)
                {
                    cx = (long)(cx * ((double)maxSize / cy));
                    cy = maxSize;
                }

                if (cx < 1) cx = 9525;
                if (cy < 1) cy = 9525;

                var imageRun = new W.Run();

                uint uniqueId = _imageId++;

                var drawing = new W.Drawing();

                var inline = new DW.Inline();

                inline.AppendChild(new DW.Extent() { Cx = cx, Cy = cy });
                inline.AppendChild(new DW.EffectExtent()
                {
                    LeftEdge = 0L,
                    TopEdge = 0L,
                    RightEdge = 0L,
                    BottomEdge = 0L
                });

                inline.AppendChild(new DW.DocProperties()
                {
                    Id = (UInt32Value)uniqueId,
                    Name = imageDto.FileName ?? $"Image_{uniqueId}",
                    Description = imageDto.Caption ?? ""
                });

                var nvGraphicFramePr = new DW.NonVisualGraphicFrameDrawingProperties();
                nvGraphicFramePr.AppendChild(new D.GraphicFrameLocks() { NoChangeAspect = true });
                inline.AppendChild(nvGraphicFramePr);

                var graphic = new D.Graphic();
                var graphicData = new D.GraphicData()
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"
                };

                var picture = new PIC.Picture();

                var nvPicPr = new PIC.NonVisualPictureProperties();
                nvPicPr.AppendChild(new PIC.NonVisualDrawingProperties()
                {
                    Id = (UInt32Value)0U,
                    Name = imageDto.FileName ?? $"Image_{uniqueId}",
                    Description = imageDto.Caption ?? ""
                });
                nvPicPr.AppendChild(new PIC.NonVisualPictureDrawingProperties());
                picture.AppendChild(nvPicPr);

                var blipFill = new PIC.BlipFill();
                var blip = new D.Blip() { Embed = imagePartId };
                blipFill.AppendChild(blip);
                blipFill.AppendChild(new D.Stretch(new D.FillRectangle()));
                picture.AppendChild(blipFill);

                var spPr = new PIC.ShapeProperties();
                var xfrm = new D.Transform2D();
                xfrm.AppendChild(new D.Offset() { X = 0L, Y = 0L });
                xfrm.AppendChild(new D.Extents() { Cx = cx, Cy = cy });
                spPr.AppendChild(xfrm);
                spPr.AppendChild(new D.PresetGeometry(new D.AdjustValueList())
                {
                    Preset = D.ShapeTypeValues.Rectangle
                });
                picture.AppendChild(spPr);

                graphicData.AppendChild(picture);
                graphic.AppendChild(graphicData);
                inline.AppendChild(graphic);

                drawing.AppendChild(inline);
                imageRun.AppendChild(drawing);
                paragraph.AppendChild(imageRun);
            }
            catch (Exception ex)
            {
                var errorRun = new W.Run(
                    new W.RunProperties(
                        new W.FontSize { Val = "24" },
                        new W.Color { Val = "FF0000" }
                    ),
                    new W.Text($"[خطا: {imageDto.FileName}]")
                );
                paragraph.AppendChild(errorRun);
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

        private TableDataDto MapTableToDto(DynamicTable table)
        {
            var columns = table.Columns.OrderBy(c => c.Order).ToList();

            return new TableDataDto
            {
                Id = table.Id,
                Title = table.Title,
                Order = table.Order,
                ShowRowNumbers = table.ShowRowNumbers,
                RowNumberHeader = table.RowNumberHeader,
                Columns = columns.Select(c => new ColumnDataDto
                {
                    Id = c.Id,
                    Header = c.Header,
                    Width = c.Width,
                    Order = c.Order
                }).ToList(),
                Rows = table.Rows.OrderBy(r => r.RowNumber).Select(r => new RowDataDto
                {
                    Id = r.Id,
                    RowNumber = r.RowNumber,
                    Cells = r.Cells.OrderBy(c => c.Column.Order).Select(c => new CellDataDto
                    {
                        Id = c.Id,
                        ColumnId = c.TableColumnDefinitionId,
                        Value = c.Value
                    }).ToList()
                }).ToList()
            };
        }

        #endregion

        #region Heading Creation Methods

        private W.Paragraph CreateMasterHeading(string text, string sectionNumber)
        {
            int spacingBefore = 4320;

            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.ParagraphStyleId() { Val = "Heading1" },
                    new W.Justification() { Val = W.JustificationValues.Center },
                    new W.BiDi(),
                    new W.SpacingBetweenLines
                    {
                        After = "0",
                        Before = spacingBefore.ToString()
                    },
                    new W.PageBreakBefore()
                )
            );

            var runs = CreateRunsForHeading(text, MasterHeaderFontSize, true);
            foreach (var run in runs)
            {
                paragraph.Append(run);
            }

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
                )
            );

            var match = Regex.Match(text, @"^([\d-]+?)\s+(.+)$");

            string finalText = text;
            if (match.Success)
            {
                var numberPart = match.Groups[1].Value;
                var titlePart = match.Groups[2].Value.Trim();

                var parts = numberPart.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    Array.Reverse(parts);
                    var reversedNumber = string.Join("-", parts);
                    if (numberPart.EndsWith("-"))
                        reversedNumber += "-";
                    numberPart = reversedNumber;
                }

                finalText = numberPart + " " + titlePart;
            }

            var runs = CreateRunsForHeading(finalText, SubHeaderFontSize, true);
            foreach (var run in runs)
            {
                paragraph.Append(run);
            }

            return paragraph;
        }

        private W.Paragraph CreateHeading(string text, string fontSize)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.ParagraphStyleId() { Val = "Heading1" },
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.BiDi(),
                    new W.SpacingBetweenLines { After = "240" }
                )
            );

            var runs = CreateRunsForHeading(text, fontSize, true);
            foreach (var run in runs)
            {
                paragraph.Append(run);
            }

            return paragraph;
        }

        private List<W.Run> CreateRunsForHeading(string text, string fontSize, bool isBold)
        {
            var runs = new List<W.Run>();
            if (string.IsNullOrEmpty(text))
            {
                runs.Add(new W.Run(new W.Text(" ")));
                return runs;
            }

            var current = new List<char>();
            bool? currentIsPersian = null;
            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                    currentIsPersian = isPersian;

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                        runs.Add(CreateRunForHeading(new string(current.ToArray()), currentIsPersian.Value, fontSize, isBold));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
                runs.Add(CreateRunForHeading(new string(current.ToArray()), currentIsPersian ?? false, fontSize, isBold));

            return runs;
        }

        private W.Run CreateRunForHeading(string text, bool isPersian, string fontSize, bool isBold)
        {
            var fontName = isPersian ? PersianFont : EnglishFont;

            var runProperties = new W.RunProperties(
                new W.RunFonts
                {
                    Ascii = fontName,
                    HighAnsi = fontName,
                    ComplexScript = fontName
                },
                new W.FontSize { Val = fontSize }
            );

            if (isBold)
                runProperties.Append(new W.Bold());

            var run = new W.Run(runProperties);
            var textElement = new W.Text(text);
            textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
            run.Append(textElement);

            return run;
        }

        #endregion

        #region Table Creation

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
                var titleRuns = CreateRunsForCover(cover.Title, true, CoverTitleFontSize);
                foreach (var run in titleRuns)
                {
                    paragraph.Append(run);
                }
                paragraph.Append(new W.Run(new W.Break()));
                paragraph.Append(new W.Run(new W.Break()));
            }

            foreach (var item in cover.Items.OrderBy(x => x.Order))
            {
                var labelText = PrepareRTLText(item.Label + ":");
                var labelRuns = CreateRunsForCover(labelText, true, "32");
                foreach (var run in labelRuns)
                {
                    paragraph.Append(run);
                }
                paragraph.Append(new W.Run(new W.Break()));

                var valueRuns = CreateRunsForCover(item.Value, true, "32");
                foreach (var run in valueRuns)
                {
                    paragraph.Append(run);
                }
                paragraph.Append(new W.Run(new W.Break()));

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
                var titleRuns = CreateRunsForCover(cover.Title, true, CoverTitleFontSize);
                foreach (var run in titleRuns)
                {
                    paragraph.Append(run);
                }
                paragraph.Append(new W.Run(new W.Break()));
            }

            foreach (var item in cover.Items.OrderBy(x => x.Order))
            {
                var labelText = PrepareRTLText(item.Label + ":");
                var labelRuns = CreateRunsForCover(labelText, false, "32");
                foreach (var run in labelRuns)
                {
                    paragraph.Append(run);
                }
                paragraph.Append(new W.Run(new W.Break()));

                var valueRuns = CreateRunsForCover(item.Value, false, "32");
                foreach (var run in valueRuns)
                {
                    paragraph.Append(run);
                }
                paragraph.Append(new W.Run(new W.Break()));

                paragraph.Append(new W.Run(new W.Break()));
            }

            return paragraph;
        }

        private List<W.Run> CreateRunsForCover(string text, bool isTitle, string fontSize)
        {
            var runs = new List<W.Run>();
            if (string.IsNullOrEmpty(text))
            {
                runs.Add(new W.Run(new W.Text(" ")));
                return runs;
            }

            var current = new List<char>();
            bool? currentIsPersian = null;
            var preparedText = PrepareRTLText(text);

            foreach (var c in preparedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                    currentIsPersian = isPersian;

                if (currentIsPersian != isPersian)
                {
                    if (current.Count > 0)
                        runs.Add(CreateRunForCoverText(new string(current.ToArray()), currentIsPersian.Value, isTitle, fontSize));
                    current.Clear();
                    currentIsPersian = isPersian;
                }

                current.Add(c);
            }

            if (current.Count > 0)
                runs.Add(CreateRunForCoverText(new string(current.ToArray()), currentIsPersian ?? false, isTitle, fontSize));

            return runs;
        }

        private W.Run CreateRunForCoverText(string text, bool isPersian, bool isTitle, string fontSize)
        {
            var fontName = isPersian ? PersianFont : EnglishFont;

            var runProperties = new W.RunProperties(
                new W.RunFonts
                {
                    Ascii = fontName,
                    HighAnsi = fontName,
                    ComplexScript = fontName
                },
                new W.FontSize { Val = fontSize }
            );

            if (isTitle)
                runProperties.Append(new W.Bold());

            var run = new W.Run(runProperties);
            var textElement = new W.Text(text);
            textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
            run.Append(textElement);

            return run;
        }

        #endregion

        #region Paragraph Creation

        private W.Paragraph CreateParagraph(string text)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification()
                    {
                        Val = W.JustificationValues.Left
                    },
                    new W.Indentation()
                    {
                        FirstLine = "397"
                    },
                    new W.SpacingBetweenLines()
                    {
                        Line = "360",
                        LineRule = W.LineSpacingRuleValues.Auto
                    }
                )
            );

            text = text?
                .Replace("\r", "")
                .TrimEnd('\n');

            var preparedText = PrepareRTLText(text ?? "");

            var current = new StringBuilder();
            bool? currentIsPersian = null;

            foreach (var c in preparedText)
            {
                // فاصله باید متعلق به Run قبلی باشد
                // نه اینکه خودش باعث تغییر نوع Run شود
                if (char.IsWhiteSpace(c))
                {
                    current.Append(c);
                    continue;
                }

                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                {
                    currentIsPersian = isPersian;
                }
                else if (currentIsPersian != isPersian)
                {
                    if (current.Length > 0)
                    {
                        paragraph.Append(
                            CreateRunForParagraph(
                                current.ToString(),
                                currentIsPersian.Value
                            )
                        );

                        current.Clear();
                    }

                    currentIsPersian = isPersian;
                }

                current.Append(c);
            }

            if (current.Length > 0 && currentIsPersian.HasValue)
            {
                paragraph.Append(
                    CreateRunForParagraph(
                        current.ToString(),
                        currentIsPersian.Value
                    )
                );
            }

            return paragraph;
        }

        private W.Run CreateRunForParagraph(string text, bool isPersian)
        {
            if (isPersian)
            {
                return new W.Run(
                    new W.RunProperties(
                        new W.RunFonts()
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize() { Val = PersianFontSize }
                    ),
                    new W.Text(text)
                );
            }
            else
            {
                // ===== متن انگلیسی را با فاصله‌های اصلی حفظ کن =====
                // مهم: از کلمات جدا شده با فاصله استفاده کن
                var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var preservedText = string.Join(" ", words);

                var run = new W.Run(
                    new W.RunProperties(
                        new W.RunFonts()
                        {
                            Ascii = EnglishFont,
                            HighAnsi = EnglishFont,
                            ComplexScript = EnglishFont
                        },
                        new W.FontSize() { Val = EnglishFontSize },
                        // اضافه کردن Spacing برای بهبود نمایش
                        new W.Spacing() { Val = 5 }
                    )
                );

                var textElement = new W.Text(preservedText);
                textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
                run.Append(textElement);

                return run;
            }
        }

        private List<W.Paragraph> CreateParagraphsFromText(string text)
        {
            var paragraphs = new List<W.Paragraph>();

            if (string.IsNullOrEmpty(text))
            {
                paragraphs.Add(new W.Paragraph());
                return paragraphs;
            }

            // ===== حفظ فاصله‌ها =====
            // جایگزینی فاصله‌های غیر استاندارد با فاصله معمولی
            text = text.Replace('\u200B', ' '); // Zero Width Space
            text = text.Replace('\u200C', ' '); // Zero Width Non-Joiner
            text = text.Replace('\u200D', ' '); // Zero Width Joiner

            // اطمینان از وجود فاصله بین کلمات انگلیسی
            // "Corrosivityofsoilsonsteel" -> "Corrosivity of soils on steel"
            text = Regex.Replace(text, @"([a-z])([A-Z])", "$1 $2");
            text = Regex.Replace(text, @"([A-Z])([A-Z][a-z])", "$1 $2");

            // ===== حالا متن را به خطوط تقسیم کن =====
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    paragraphs.Add(CreateParagraph(line.Trim()));
                }
            }

            if (paragraphs.Count == 0)
            {
                paragraphs.Add(new W.Paragraph());
            }

            return paragraphs;
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

            // ===== استفاده از متد جدید برای هدر =====
            var runs = CreateRunsForHeaderCell(text);
            foreach (var run in runs)
            {
                paragraph.Append(run);
            }

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

            var shading = new W.Shading { Val = W.ShadingPatternValues.Clear, Color = "auto", Fill = "FFFFFF" };
            cellProps.Append(shading);

            cell.Append(cellProps);
            return cell;
        }
        private List<W.Run> CreateRunsForHeaderCell(string text)
        {
            var runs = new List<W.Run>();
            if (string.IsNullOrEmpty(text))
            {
                runs.Add(CreateHeaderCellRun(" ", false));
                return runs;
            }

            // اگر متن فقط انگلیسی است
            if (text.All(c => IsEnglish(c) || char.IsWhiteSpace(c) || c == '.' || c == ',' || c == '-' || c == '_'))
            {
                runs.Add(CreateHeaderCellRun(text, false));
                return runs;
            }

            // اگر متن فقط فارسی است
            if (text.All(c => !IsEnglish(c) || char.IsWhiteSpace(c)))
            {
                runs.Add(CreateHeaderCellRun(text, true));
                return runs;
            }

            var current = new StringBuilder();
            bool? currentIsPersian = null;
            var processedText = text;

            processedText = processedText.Replace('\u200B', ' ');
            processedText = processedText.Replace('\u200C', ' ');
            processedText = processedText.Replace('\u200D', ' ');

            // اطمینان از وجود فاصله بین کلمات انگلیسی
            processedText = Regex.Replace(processedText, @"([a-z])([A-Z])", "$1 $2");
            processedText = Regex.Replace(processedText, @"([A-Z])([A-Z][a-z])", "$1 $2");
            processedText = Regex.Replace(processedText, @"(\d+)([^\d\s])", "$1 $2");
            processedText = Regex.Replace(processedText, @"([^\d\s])(\d+)", "$1 $2");

            foreach (char c in processedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                {
                    currentIsPersian = isPersian;
                }
                else if (currentIsPersian != isPersian)
                {
                    if (current.Length > 0)
                    {
                        var segment = current.ToString().Trim();
                        if (!string.IsNullOrEmpty(segment))
                        {
                            runs.Add(CreateHeaderCellRun(segment, currentIsPersian.Value));
                        }
                        current.Clear();
                    }
                    currentIsPersian = isPersian;
                }

                current.Append(c);
            }

            if (current.Length > 0 && currentIsPersian.HasValue)
            {
                var segment = current.ToString().Trim();
                if (!string.IsNullOrEmpty(segment))
                {
                    runs.Add(CreateHeaderCellRun(segment, currentIsPersian.Value));
                }
            }

            if (runs.Count == 0)
            {
                runs.Add(CreateHeaderCellRun(" ", false));
            }

            return runs;
        }

        private W.Run CreateHeaderCellRun(string text, bool isPersian)
        {
            var fontName = isPersian ? PersianFont : EnglishFont;

            var runProperties = new W.RunProperties(
                new W.RunFonts
                {
                    Ascii = fontName,
                    HighAnsi = fontName,
                    ComplexScript = fontName
                },
                new W.FontSize { Val = TableHeaderFontSize },
                new W.Bold()
            );

            var run = new W.Run(runProperties);

            if (!string.IsNullOrWhiteSpace(text))
            {
                var textElement = new W.Text(text);
                textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
                run.Append(textElement);
            }
            else
            {
                var textElement = new W.Text(" ");
                textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
                run.Append(textElement);
            }

            return run;
        }

        private W.TableCell CreateDataCell(string text, string backgroundColor = null)
        {
            var cell = new W.TableCell();

            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification { Val = W.JustificationValues.Left },
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
                var shading = new W.Shading { Val = W.ShadingPatternValues.Clear, Color = "auto", Fill = backgroundColor };
                cellProps.Append(shading);
            }

            cell.Append(cellProps);
            return cell;
        }

        private List<W.Run> CreateRunsForTableCell(string text)
        {
            var runs = new List<W.Run>();
            if (string.IsNullOrEmpty(text))
            {
                runs.Add(CreateTableCellRun(" ", false, false));
                return runs;
            }

            // ===== اگر متن فقط انگلیسی است =====
            if (text.All(c => IsEnglish(c) || char.IsWhiteSpace(c) || c == '.' || c == ',' || c == '-' || c == '_'))
            {
                runs.Add(CreateTableCellRun(text, false, false));
                return runs;
            }

            // ===== اگر متن فقط فارسی است (همه کاراکترها غیر انگلیسی) =====
            if (text.All(c => !IsEnglish(c) || char.IsWhiteSpace(c)))
            {
                runs.Add(CreateTableCellRun(text, true, false));
                return runs;
            }

            // ===== متن مخلوط =====
            var current = new StringBuilder();
            bool? currentIsPersian = null;
            var processedText = text;

            // جایگزین کردن فاصله‌های غیراستاندارد
            processedText = processedText.Replace('\u200B', ' ');
            processedText = processedText.Replace('\u200C', ' ');
            processedText = processedText.Replace('\u200D', ' ');

            // ===== مهم: اطمینان از وجود فاصله بین کلمات انگلیسی =====
            // "Corrosivityofsoilsonsteel" -> "Corrosivity of soils on steel"
            processedText = Regex.Replace(processedText, @"([a-z])([A-Z])", "$1 $2");
            processedText = Regex.Replace(processedText, @"([A-Z])([A-Z][a-z])", "$1 $2");
            // اضافه کردن فاصله بعد از اعداد قبل از حرف فارسی
            processedText = Regex.Replace(processedText, @"(\d+)([^\d\s])", "$1 $2");
            // اضافه کردن فاصله قبل از اعداد بعد از حرف فارسی
            processedText = Regex.Replace(processedText, @"([^\d\s])(\d+)", "$1 $2");

            foreach (char c in processedText)
            {
                bool isPersian = !IsEnglish(c);

                if (currentIsPersian == null)
                {
                    currentIsPersian = isPersian;
                }
                else if (currentIsPersian != isPersian)
                {
                    // ذخیره بخش قبلی
                    if (current.Length > 0)
                    {
                        var segment = current.ToString().Trim();
                        if (!string.IsNullOrEmpty(segment))
                        {
                            runs.Add(CreateTableCellRun(segment, currentIsPersian.Value, false));
                        }
                        current.Clear();
                    }
                    currentIsPersian = isPersian;
                }

                current.Append(c);
            }

            // ذخیره بخش آخر
            if (current.Length > 0 && currentIsPersian.HasValue)
            {
                var segment = current.ToString().Trim();
                if (!string.IsNullOrEmpty(segment))
                {
                    runs.Add(CreateTableCellRun(segment, currentIsPersian.Value, false));
                }
            }

            // اگر هیچ Run ساخته نشد
            if (runs.Count == 0)
            {
                runs.Add(CreateTableCellRun(" ", false, false));
            }

            return runs;
        }

        private W.Run CreateTableCellRun(string text, bool isPersian, bool isHeader)
        {
            var fontName = isPersian ? PersianFont : EnglishFont;
            var fontSize = isHeader ? TableHeaderFontSize : "20";

            var runProperties = new W.RunProperties(
                new W.RunFonts
                {
                    Ascii = fontName,
                    HighAnsi = fontName,
                    ComplexScript = fontName
                },
                new W.FontSize { Val = fontSize }
            );

            if (isHeader)
                runProperties.Append(new W.Bold());

            var run = new W.Run(runProperties);

            // ===== مهم: حفظ فاصله‌ها =====
            if (!string.IsNullOrWhiteSpace(text))
            {
                var textElement = new W.Text(text);
                textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
                run.Append(textElement);
            }
            else
            {
                var textElement = new W.Text(" ");
                textElement.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
                run.Append(textElement);
            }

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
            normalStyle.Append(new W.StyleName { Val = "Normal" });
            normalStyle.Append(new W.StyleRunProperties(
                new W.RunFonts { Ascii = PersianFont, HighAnsi = PersianFont, ComplexScript = PersianFont },
                new W.FontSize { Val = "24" }
            ));
            styles.Append(normalStyle);

            var heading1Style = new W.Style
            {
                Type = W.StyleValues.Paragraph,
                StyleId = "Heading1",
                Default = false,
                CustomStyle = false
            };
            heading1Style.Append(new W.StyleName { Val = "heading 1" });
            heading1Style.Append(new W.StyleParagraphProperties(
                new W.ParagraphStyleId { Val = "Heading1" },
                new W.Justification { Val = W.JustificationValues.Center },
                new W.SpacingBetweenLines { After = "240", Line = "240" },
                new W.OutlineLevel { Val = 0 }
            ));
            heading1Style.Append(new W.StyleRunProperties(
                new W.RunFonts { Ascii = PersianFont, HighAnsi = PersianFont, ComplexScript = PersianFont },
                new W.FontSize { Val = MasterHeaderFontSize },
                new W.Bold()
            ));
            styles.Append(heading1Style);

            var heading2Style = new W.Style
            {
                Type = W.StyleValues.Paragraph,
                StyleId = "Heading2",
                Default = false,
                CustomStyle = false
            };
            heading2Style.Append(new W.StyleName { Val = "heading 2" });
            heading2Style.Append(new W.StyleParagraphProperties(
                new W.ParagraphStyleId { Val = "Heading2" },
                new W.Justification { Val = W.JustificationValues.Left },
                new W.SpacingBetweenLines { After = "120", Line = "240" },
                new W.OutlineLevel { Val = 1 }
            ));
            heading2Style.Append(new W.StyleRunProperties(
                new W.RunFonts { Ascii = PersianFont, HighAnsi = PersianFont, ComplexScript = PersianFont },
                new W.FontSize { Val = SubHeaderFontSize },
                new W.Bold()
            ));
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
            text = Regex.Replace(text, @"«(.*?)»", m => "\u200F»" + m.Groups[1].Value + "«\u200F");

            return "\u202B" + text + "\u202C";
        }

        private bool IsEnglish(char c)
        {
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                return true;

            if (c >= '0' && c <= '9')
                return false;

            return c == '[' || c == ']' || c == '{' || c == '}' || c == '(' || c == ')' || c == '»'|| c == '«' ||
                   /*c == '.' ||*/ c == ',' || c == ';' || c == '!' || c == '?' ||
                   c == '@' || c == '#' || c == '$' || c == '%' || c == '^' || c == '&' ||
                   c == '*' || c == '+' || c == '=' || c == '<' || c == '>' || c == '/' ||
                   c == '\\' || c == '|' || c == '~' || c == '`' || c == '_' || c == '-';
        }

        #endregion
    }
}