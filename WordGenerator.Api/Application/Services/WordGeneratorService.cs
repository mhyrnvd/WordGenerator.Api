using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WordGenerator.Api.Application.DTOs;
using WordGenerator.Api.Application.Requests;
using WordGenerator.Api.Domain.Entities;
using WordGenerator.Api.Domain.Enums;
using WordGenerator.Api.Infra.Context;

// ===== Using aliases =====
using W = DocumentFormat.OpenXml.Wordprocessing;
using D = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

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

                // ایجاد Numbering Definitions برای Bullet List
                InitializeNumbering(mainPart);

                var body = new W.Body();

                // Cover Page
                if (document.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromDto(document.CoverPage));

                    foreach (var element in document.CoverPage.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart);
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

                // Master Sections
                int masterCounter = 0;
                foreach (var masterSection in document.MasterSections.OrderBy(x => x.Order))
                {
                    masterCounter++;
                    body.Append(CreateMasterHeading(masterSection.Title, masterCounter.ToString()));

                    foreach (var element in masterSection.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart);
                    }

                    int subCounter = 0;
                    foreach (var subSection in masterSection.SubSections.OrderBy(x => x.Order))
                    {
                        subCounter++;
                        body.Append(CreateSubHeading(subSection.Title, $"{subCounter}-{masterCounter}"));

                        foreach (var element in subSection.Elements.OrderBy(x => x.Order))
                        {
                            RenderElement(body, element, mainPart);
                        }
                    }
                }

                // Legacy Sections
                foreach (var section in document.Sections.OrderBy(x => x.Order))
                {
                    body.Append(CreateHeading(section.Title, "32"));

                    foreach (var element in section.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart);
                    }
                }

                // ===== بخش‌های انتهای سند =====

                // الف) پیوست‌ها
                if (document.Attachments != null && document.Attachments.IsActive)
                {
                    RenderAttachmentsSection(body, document.Attachments, mainPart);
                }

                // ب) منابع (انگلیسی - چپ‌چین)
                if (document.References != null && document.References.IsActive)
                {
                    RenderReferencesSection(body, document.References, mainPart);
                }

                // پ) مدارک
                if (document.Documents != null && document.Documents.IsActive)
                {
                    RenderDocumentsSection(body, document.Documents, mainPart);
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
                    .ThenInclude(x => x.Elements)
                        .ThenInclude(e => e.Image)
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                                    .ThenInclude(c => c.Column)
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Elements)
                        .ThenInclude(e => e.BulletList)
                            .ThenInclude(b => b.Items)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Elements)
                        .ThenInclude(e => e.Image)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                                    .ThenInclude(c => c.Column)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Elements)
                        .ThenInclude(e => e.BulletList)
                            .ThenInclude(b => b.Items)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Elements)
                            .ThenInclude(e => e.Image)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Columns)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Rows)
                                    .ThenInclude(r => r.Cells)
                                        .ThenInclude(c => c.Column)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Elements)
                            .ThenInclude(e => e.BulletList)
                                .ThenInclude(b => b.Items)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Elements)
                        .ThenInclude(e => e.Image)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                                    .ThenInclude(c => c.Column)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Elements)
                        .ThenInclude(e => e.BulletList)
                            .ThenInclude(b => b.Items)
                // ===== Include بخش‌های جدید =====
                .Include(x => x.AttachmentSection)
                    .ThenInclude(a => a.Elements)
                        .ThenInclude(e => e.Image)
                .Include(x => x.AttachmentSection)
                    .ThenInclude(a => a.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.AttachmentSection)
                    .ThenInclude(a => a.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                                    .ThenInclude(c => c.Column)
                .Include(x => x.AttachmentSection)
                    .ThenInclude(a => a.Elements)
                        .ThenInclude(e => e.BulletList)
                            .ThenInclude(b => b.Items)
                .Include(x => x.ReferenceSection)
                    .ThenInclude(r => r.Elements)
                        .ThenInclude(e => e.Image)
                .Include(x => x.ReferenceSection)
                    .ThenInclude(r => r.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.ReferenceSection)
                    .ThenInclude(r => r.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                                    .ThenInclude(c => c.Column)
                .Include(x => x.ReferenceSection)
                    .ThenInclude(r => r.Elements)
                        .ThenInclude(e => e.BulletList)
                            .ThenInclude(b => b.Items)
                .Include(x => x.DocumentSection)
                    .ThenInclude(d => d.Elements)
                        .ThenInclude(e => e.Image)
                .Include(x => x.DocumentSection)
                    .ThenInclude(d => d.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.DocumentSection)
                    .ThenInclude(d => d.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                                    .ThenInclude(c => c.Column)
                .Include(x => x.DocumentSection)
                    .ThenInclude(d => d.Elements)
                        .ThenInclude(e => e.BulletList)
                            .ThenInclude(b => b.Items)
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

                // ایجاد Numbering Definitions برای Bullet List
                InitializeNumbering(mainPart);

                var body = new W.Body();

                // Cover Page
                if (template.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromEntity(template.CoverPage));

                    foreach (var element in template.CoverPage.Elements.OrderBy(x => x.Order))
                    {
                        RenderElementFromEntity(body, element, mainPart);
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

                    foreach (var element in masterSection.Elements.OrderBy(x => x.Order))
                    {
                        RenderElementFromEntity(body, element, mainPart);
                    }

                    int subCounter = 0;
                    foreach (var subSection in masterSection.SubSections.OrderBy(x => x.Order))
                    {
                        subCounter++;
                        body.Append(CreateSubHeading(subSection.Title, $"{subCounter}-{masterCounter}"));

                        foreach (var element in subSection.Elements.OrderBy(x => x.Order))
                        {
                            RenderElementFromEntity(body, element, mainPart);
                        }
                    }
                }

                // Legacy Sections
                foreach (var section in selectedSections)
                {
                    body.Append(CreateHeading(section.Title, "32"));

                    foreach (var element in section.Elements.OrderBy(x => x.Order))
                    {
                        RenderElementFromEntity(body, element, mainPart);
                    }
                }

                // ===== بخش‌های انتهای سند =====

                // الف) پیوست‌ها
                if (template.AttachmentSection != null && template.AttachmentSection.IsActive)
                {
                    RenderAttachmentsSectionFromEntity(body, template.AttachmentSection, mainPart);
                }

                // ب) منابع (انگلیسی - چپ‌چین)
                if (template.ReferenceSection != null && template.ReferenceSection.IsActive)
                {
                    RenderReferencesSectionFromEntity(body, template.ReferenceSection, mainPart);
                }

                // پ) مدارک
                if (template.DocumentSection != null && template.DocumentSection.IsActive)
                {
                    RenderDocumentsSectionFromEntity(body, template.DocumentSection, mainPart);
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

                // ایجاد Numbering Definitions برای Bullet List
                InitializeNumbering(mainPart);

                var body = new W.Body();

                // ========== Cover Page ==========
                if (request.CoverPage != null)
                {
                    body.Append(CreateCoverPageFromDto(request.CoverPage));

                    foreach (var element in request.CoverPage.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart);
                    }

                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
                }

                // ========== Table of Contents ==========
                if (request.IncludeTableOfContents)
                {
                    body.Append(CreateHeading("فهرست مطالب", "32"));
                    body.Append(CreateTableOfContents());
                    body.Append(new W.Paragraph(new W.Run(new W.Text(""))));
                    body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));
                }

                // ========== Master Sections ==========
                int masterCounter = 0;
                foreach (var masterSection in request.MasterSections.OrderBy(x => x.Order))
                {
                    masterCounter++;
                    body.Append(CreateMasterHeading(masterSection.Title, masterCounter.ToString()));

                    foreach (var element in masterSection.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart);
                    }

                    int subCounter = 0;
                    foreach (var subSection in masterSection.SubSections.OrderBy(x => x.Order))
                    {
                        subCounter++;
                        body.Append(CreateSubHeading(subSection.Title, $"{subCounter}-{masterCounter}"));

                        foreach (var element in subSection.Elements.OrderBy(x => x.Order))
                        {
                            RenderElement(body, element, mainPart);
                        }
                    }
                }

                // ========== Legacy Sections ==========
                foreach (var section in request.Sections.OrderBy(x => x.Order))
                {
                    body.Append(CreateHeading(section.Title, "32"));

                    foreach (var element in section.Elements.OrderBy(x => x.Order))
                    {
                        RenderElement(body, element, mainPart);
                    }
                }

                // ========== بخش‌های انتهای سند ==========

                // الف) پیوست‌ها
                if (request.Attachments != null && request.Attachments.IsActive)
                {
                    RenderAttachmentsSection(body, request.Attachments, mainPart);
                }

                // ب) منابع (انگلیسی - چپ‌چین)
                if (request.References != null && request.References.IsActive)
                {
                    RenderReferencesSection(body, request.References, mainPart);
                }

                // پ) مدارک
                if (request.Documents != null && request.Documents.IsActive)
                {
                    RenderDocumentsSection(body, request.Documents, mainPart);
                }

                mainPart.Document.Append(body);

                // ========== اضافه کردن Header ==========
                if (request.PageHeader != null && request.PageHeader.IsActive)
                {
                    AddHeaderToDocument(mainPart, request.PageHeader);
                }

                mainPart.Document.Save();
            }

            return ms.ToArray();
        }
        #endregion

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

            // بررسی وجود Abstract Numbering با AbstractNumberId = 1
            var existingAbstractNum = numberingPart.Numbering
                .Elements<W.AbstractNum>()
                .FirstOrDefault(a => a.AbstractNumberId != null && a.AbstractNumberId.Value == 1);

            if (existingAbstractNum != null)
                return;

            // ایجاد Abstract Numbering برای Bullet
            var abstractNum = new W.AbstractNum()
            {
                AbstractNumberId = 1
            };

            // تنظیم MultiLevelType
            var multiLevelType = new W.MultiLevelType();
            multiLevelType.SetAttribute(new OpenXmlAttribute("val", null, "bullet"));
            abstractNum.Append(multiLevelType);

            // ===== Level 0 - ❖ =====
            var level0 = new W.Level() { LevelIndex = 0 };
            level0.Append(new W.StartNumberingValue() { Val = 1 });

            var numFormat0 = new W.NumberingFormat();
            numFormat0.SetAttribute(new OpenXmlAttribute("val", null, "bullet"));
            level0.Append(numFormat0);

            level0.Append(new W.LevelText() { Val = "❖" });
            level0.Append(new W.LevelJustification() { Val = W.LevelJustificationValues.Left });
            level0.Append(new W.ParagraphProperties(
                new W.Indentation() { Left = "720", Hanging = "360" }
            ));
            level0.Append(new W.RunProperties(
                new W.RunFonts()
                {
                    Ascii = "Segoe UI Symbol",
                    HighAnsi = "Segoe UI Symbol",
                    ComplexScript = "Segoe UI Symbol"
                }
            ));
            abstractNum.Append(level0);

            // ===== Level 1 - o (دایره توخالی) =====
            var level1 = new W.Level() { LevelIndex = 1 };
            level1.Append(new W.StartNumberingValue() { Val = 1 });

            var numFormat1 = new W.NumberingFormat();
            numFormat1.SetAttribute(new OpenXmlAttribute("val", null, "bullet"));
            level1.Append(numFormat1);

            level1.Append(new W.LevelText() { Val = "o" });
            level1.Append(new W.LevelJustification() { Val = W.LevelJustificationValues.Left });
            level1.Append(new W.ParagraphProperties(
                new W.Indentation() { Left = "1440", Hanging = "360" }
            ));
            level1.Append(new W.RunProperties(
                new W.RunFonts()
                {
                    Ascii = "Symbol",
                    HighAnsi = "Symbol",
                    ComplexScript = "Symbol"
                }
            ));
            abstractNum.Append(level1);

            // ===== Level 2 - ▪ (مربع) =====
            var level2 = new W.Level() { LevelIndex = 2 };
            level2.Append(new W.StartNumberingValue() { Val = 1 });

            var numFormat2 = new W.NumberingFormat();
            numFormat2.SetAttribute(new OpenXmlAttribute("val", null, "bullet"));
            level2.Append(numFormat2);

            level2.Append(new W.LevelText() { Val = "▪" });
            level2.Append(new W.LevelJustification() { Val = W.LevelJustificationValues.Left });
            level2.Append(new W.ParagraphProperties(
                new W.Indentation() { Left = "2160", Hanging = "360" }
            ));
            level2.Append(new W.RunProperties(
                new W.RunFonts()
                {
                    Ascii = "Symbol",
                    HighAnsi = "Symbol",
                    ComplexScript = "Symbol"
                }
            ));
            abstractNum.Append(level2);

            numberingPart.Numbering.Append(abstractNum);

            // ===== ایجاد Numbering Instance =====
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

            // ستون چپ (لوگوها)
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

            if (logos.Any())
            {
                var firstLogo = logos[0];
                var firstImageDto = new ImageItemDto
                {
                    FileName = firstLogo.FileName,
                    Width = firstLogo.Width > 0 ? firstLogo.Width : 60,
                    Height = firstLogo.Height > 0 ? firstLogo.Height : 40,
                    ImageBase64 = firstLogo.ImageBase64
                };
                InsertImageToHeader(leftParagraph, firstImageDto, mainPart, headerPart);
            }

            for (int i = 1; i < logos.Count; i++)
            {
                var spaceRun = new W.Run();
                var spaceRunProps = new W.RunProperties();
                spaceRun.AppendChild(spaceRunProps);

                var spaceText = new W.Text("     ");
                spaceText.SetAttribute(new OpenXmlAttribute("xml:space", null, "preserve"));
                spaceRun.AppendChild(spaceText);
                leftParagraph.AppendChild(spaceRun);

                var logo = logos[i];
                var imageDto = new ImageItemDto
                {
                    FileName = logo.FileName,
                    Width = logo.Width > 0 ? logo.Width : 60,
                    Height = logo.Height > 0 ? logo.Height : 40,
                    ImageBase64 = logo.ImageBase64
                };
                InsertImageToHeader(leftParagraph, imageDto, mainPart, headerPart);
            }

            leftCell.Append(leftParagraph);
            row.Append(leftCell);

            // ستون راست (متن هدر)
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
                rightParagraph.Append(new W.Run(
                    new W.RunProperties(
                        new W.RunFonts { Ascii = PersianFont, HighAnsi = PersianFont, ComplexScript = PersianFont },
                        new W.FontSize { Val = "22" },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText(pageHeader.HeaderText))
                ));
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

        private void InsertImageToHeader(W.Paragraph paragraph, ImageItemDto imageDto, MainDocumentPart mainPart, HeaderPart headerPart)
        {
            if (string.IsNullOrEmpty(imageDto.ImageBase64))
                return;

            try
            {
                var imageBytes = Convert.FromBase64String(imageDto.ImageBase64);
                if (imageBytes == null || imageBytes.Length == 0)
                    return;

                // تشخیص نوع تصویر
                var imagePartType = ImagePartType.Jpeg;
                if (imageBytes.Length > 4)
                {
                    if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 &&
                        imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                        imagePartType = ImagePartType.Png;
                    else if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                        imagePartType = ImagePartType.Jpeg;
                }

                var imagePart = headerPart.AddImagePart(imagePartType);
                using (var stream = new MemoryStream(imageBytes))
                {
                    imagePart.FeedData(stream);
                }
                var imagePartId = headerPart.GetIdOfPart(imagePart);
                if (string.IsNullOrEmpty(imagePartId))
                    return;

                long cx = (long)(imageDto.Width * 9525);
                long cy = (long)(imageDto.Height * 9525);

                if (cx < 1) cx = 9525;
                if (cy < 1) cy = 9525;

                // محدودیت اندازه
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

                var run = new W.Run();

                var drawing = new W.Drawing(
                    new DW.Inline(
                        new DW.Extent() { Cx = cx, Cy = cy },
                        new DW.EffectExtent()
                        {
                            LeftEdge = 0L,
                            TopEdge = 0L,
                            RightEdge = 100000L,
                            BottomEdge = 0L
                        },
                        new DW.DocProperties()
                        {
                            Id = (UInt32Value)(imageDto.Id ?? DateTime.Now.Ticks),
                            Name = imageDto.FileName ?? "Logo"
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
                                            Name = imageDto.FileName ?? "logo.jpg"
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

                run.Append(drawing);
                paragraph.Append(run);
            }
            catch (Exception ex)
            {
                var run = new W.Run(
                    new W.RunProperties(
                        new W.FontSize { Val = "16" },
                        new W.Color { Val = "FF6600" }
                    ),
                    new W.Text($"[{imageDto.FileName ?? "لوگو"}]")
                );
                paragraph.Append(run);
            }
        }
        #endregion

        #region Element Rendering Methods

        private void RenderElement(W.Body body, ContentElementDto element, MainDocumentPart mainPart)
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
                        body.Append(CreateTableFromDto(element.Table));
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

        private void RenderElementFromEntity(W.Body body, ContentElement element, MainDocumentPart mainPart)
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
                    }
                    break;

                case ContentElementType.Table:
                    if (element.Table != null)
                    {
                        var tableDto = MapTableToDto(element.Table);
                        body.Append(CreateTableFromDto(tableDto));
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

        #endregion

        #region Final Sections (Attachments, References, Documents)

        /// <summary>
        /// ایجاد بخش پیوست‌ها - راست‌چین
        /// </summary>
        private void RenderAttachmentsSection(W.Body body, AttachmentSectionDto attachments, MainDocumentPart mainPart)
        {
            if (attachments == null || !attachments.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                ),
                new W.Run(
                    new W.RunProperties(
                        new W.RunFonts
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize { Val = MasterHeaderFontSize },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText(attachments.Title))
                )
            );
            body.Append(titleParagraph);

            foreach (var element in attachments.Elements.OrderBy(x => x.Order))
            {
                RenderElement(body, element, mainPart);
            }
        }

        private void RenderAttachmentsSectionFromEntity(W.Body body, AttachmentSection attachments, MainDocumentPart mainPart)
        {
            if (attachments == null || !attachments.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                ),
                new W.Run(
                    new W.RunProperties(
                        new W.RunFonts
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize { Val = MasterHeaderFontSize },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText(attachments.Title))
                )
            );
            body.Append(titleParagraph);

            foreach (var element in attachments.Elements.OrderBy(x => x.Order))
            {
                RenderElementFromEntity(body, element, mainPart);
            }
        }

        /// <summary>
        /// ایجاد بخش منابع - چپ‌چین و انگلیسی برای محتوا، راست‌چین برای عنوان
        /// </summary>
        private void RenderReferencesSection(W.Body body, ReferenceSectionDto references, MainDocumentPart mainPart)
        {
            if (references == null || !references.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                ),
                new W.Run(
                    new W.RunProperties(
                        new W.RunFonts
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize { Val = MasterHeaderFontSize },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText(references.Title))
                )
            );
            body.Append(titleParagraph);

            foreach (var element in references.Elements.OrderBy(x => x.Order))
            {
                RenderElementWithLeftAlignment(body, element, mainPart);
            }
        }

        private void RenderReferencesSectionFromEntity(W.Body body, ReferenceSection references, MainDocumentPart mainPart)
        {
            if (references == null || !references.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                ),
                new W.Run(
                    new W.RunProperties(
                        new W.RunFonts
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize { Val = MasterHeaderFontSize },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText(references.Title))
                )
            );
            body.Append(titleParagraph);

            foreach (var element in references.Elements.OrderBy(x => x.Order))
            {
                var elementDto = MapContentElementToDto(element);
                RenderElementWithLeftAlignment(body, elementDto, mainPart);
            }
        }

        /// <summary>
        /// رندر المان با چپ‌چین (برای بخش منابع)
        /// </summary>
        private void RenderElementWithLeftAlignment(W.Body body, ContentElementDto element, MainDocumentPart mainPart)
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
                        body.Append(CreateLeftAlignedTableFromDto(element.Table));
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
                    }
                    break;
            }
        }

        /// <summary>
        /// ایجاد پاراگراف چپ‌چین برای منابع انگلیسی
        /// </summary>
        private W.Paragraph CreateLeftAlignedParagraph(string text)
        {
            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.Indentation() { FirstLine = "397" },
                    new W.SpacingBetweenLines()
                    {
                        Line = "360",
                        LineRule = W.LineSpacingRuleValues.Auto
                    }
                )
            );

            var run = new W.Run(
                new W.RunProperties(
                    new W.RunFonts()
                    {
                        Ascii = EnglishFont,
                        HighAnsi = EnglishFont,
                        ComplexScript = EnglishFont
                    },
                    new W.FontSize() { Val = EnglishFontSize }
                ),
                new W.Text(text)
            );
            paragraph.Append(run);

            return paragraph;
        }

        /// <summary>
        /// ایجاد Bullet List چپ‌چین برای منابع انگلیسی
        /// </summary>
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
                    ),
                    new W.Run(
                        new W.RunProperties(
                            new W.RunFonts
                            {
                                Ascii = EnglishFont,
                                HighAnsi = EnglishFont,
                                ComplexScript = EnglishFont
                            },
                            new W.FontSize { Val = "28" },
                            new W.Bold()
                        ),
                        new W.Text(bulletList.Title)
                    )
                );
                paragraphs.Add(titleParagraph);
            }

            foreach (var item in bulletList.Items.OrderBy(x => x.Order))
            {
                var paragraph = CreateLeftAlignedBulletListParagraph(item.Text, item.Level);
                paragraphs.Add(paragraph);
            }

            var spacingParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.SpacingBetweenLines { After = "200" }
                )
            );
            paragraphs.Add(spacingParagraph);

            return paragraphs.ToArray();
        }

        /// <summary>
        /// ایجاد Bullet List Paragraph چپ‌چین
        /// </summary>
        private W.Paragraph CreateLeftAlignedBulletListParagraph(string text, int level = 0)
        {
            int safeLevel = Math.Clamp(level, 0, 2);

            var numberingProps = new W.NumberingProperties();
            var numberingLevelRef = new W.NumberingLevelReference() { Val = safeLevel };
            var numberingId = new W.NumberingId() { Val = 1 };
            numberingProps.Append(numberingLevelRef);
            numberingProps.Append(numberingId);

            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.Justification() { Val = W.JustificationValues.Left },
                    numberingProps,
                    new W.Indentation()
                    {
                        FirstLine = "397",
                        Left = (safeLevel * 720).ToString()
                    },
                    new W.SpacingBetweenLines()
                    {
                        Line = "360",
                        LineRule = W.LineSpacingRuleValues.Auto,
                        After = "0"
                    }
                )
            );

            var run = new W.Run(
                new W.RunProperties(
                    new W.RunFonts()
                    {
                        Ascii = EnglishFont,
                        HighAnsi = EnglishFont,
                        ComplexScript = EnglishFont
                    },
                    new W.FontSize() { Val = EnglishFontSize }
                ),
                new W.Text(text)
            );
            paragraph.Append(run);

            return paragraph;
        }

        /// <summary>
        /// ایجاد جدول چپ‌چین برای منابع انگلیسی
        /// </summary>
        private W.Table CreateLeftAlignedTableFromDto(TableDataDto tableData)
        {
            var table = CreateTableFromDto(tableData);

            var tableProps = table.GetFirstChild<W.TableProperties>();
            if (tableProps != null)
            {
                var justification = tableProps.GetFirstChild<W.Justification>();
                if (justification != null)
                {
                    justification.Val = W.JustificationValues.Left;
                }
                else
                {
                    tableProps.Append(new W.Justification { Val = W.JustificationValues.Left });
                }
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
                            {
                                justification.Val = W.JustificationValues.Left;
                            }
                            else
                            {
                                paraProps.Append(new W.Justification { Val = W.JustificationValues.Left });
                            }

                            var bidi = paraProps.GetFirstChild<W.BiDi>();
                            if (bidi != null)
                                bidi.Remove();
                        }
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// ایجاد بخش مدارک - راست‌چین
        /// </summary>
        private void RenderDocumentsSection(W.Body body, DocumentSectionDto documents, MainDocumentPart mainPart)
        {
            if (documents == null || !documents.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                ),
                new W.Run(
                    new W.RunProperties(
                        new W.RunFonts
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize { Val = MasterHeaderFontSize },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText(documents.Title))
                )
            );
            body.Append(titleParagraph);

            foreach (var element in documents.Elements.OrderBy(x => x.Order))
            {
                RenderElement(body, element, mainPart);
            }
        }

        private void RenderDocumentsSectionFromEntity(W.Body body, DocumentSection documents, MainDocumentPart mainPart)
        {
            if (documents == null || !documents.IsActive)
                return;

            body.Append(new W.Paragraph(new W.Run(new W.Break() { Type = BreakValues.Page })));

            var titleParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    new W.SpacingBetweenLines { After = "240", Before = "120" }
                ),
                new W.Run(
                    new W.RunProperties(
                        new W.RunFonts
                        {
                            Ascii = PersianFont,
                            HighAnsi = PersianFont,
                            ComplexScript = PersianFont
                        },
                        new W.FontSize { Val = MasterHeaderFontSize },
                        new W.Bold()
                    ),
                    new W.Text(PrepareRTLText(documents.Title))
                )
            );
            body.Append(titleParagraph);

            foreach (var element in documents.Elements.OrderBy(x => x.Order))
            {
                RenderElementFromEntity(body, element, mainPart);
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
                    ),
                    new W.Run(
                        new W.RunProperties(
                            new W.RunFonts
                            {
                                Ascii = PersianFont,
                                HighAnsi = PersianFont,
                                ComplexScript = PersianFont
                            },
                            new W.FontSize { Val = "28" },
                            new W.Bold()
                        ),
                        new W.Text(PrepareRTLText(bulletList.Title))
                    )
                );
                paragraphs.Add(titleParagraph);
            }

            foreach (var item in bulletList.Items.OrderBy(x => x.Order))
            {
                var paragraph = CreateBulletListParagraph(item.Text, item.Level);
                paragraphs.Add(paragraph);
            }

            var spacingParagraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.SpacingBetweenLines { After = "200" }
                )
            );
            paragraphs.Add(spacingParagraph);

            return paragraphs.ToArray();
        }

        private W.Paragraph CreateBulletListParagraph(string text, int level = 0)
        {
            int safeLevel = Math.Clamp(level, 0, 2);

            var numberingProps = new W.NumberingProperties();
            var numberingLevelRef = new W.NumberingLevelReference() { Val = safeLevel };
            var numberingId = new W.NumberingId() { Val = 1 };

            numberingProps.Append(numberingLevelRef);
            numberingProps.Append(numberingId);

            var paragraph = new W.Paragraph(
                new W.ParagraphProperties(
                    new W.BiDi(),
                    new W.Justification() { Val = W.JustificationValues.Left },
                    numberingProps,
                    new W.Indentation()
                    {
                        FirstLine = "397",
                        Left = (safeLevel * 720).ToString()
                    },
                    new W.SpacingBetweenLines()
                    {
                        Line = "360",
                        LineRule = W.LineSpacingRuleValues.Auto,
                        After = "0"
                    }
                )
            );

            var textRuns = CreateRunsForBulletText(text);
            foreach (var run in textRuns)
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
                {
                    currentIsPersian = isPersian;
                }

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
            {
                runs.Add(CreateBulletTextRun(new string(current.ToArray()), currentIsPersian ?? false));
            }

            if (runs.Count == 0)
            {
                runs.Add(CreateBulletTextRun(" ", false));
            }

            return runs;
        }

        private W.Run CreateBulletTextRun(string text, bool isPersian)
        {
            var runProperties = new W.RunProperties(
                new W.RunFonts()
                {
                    Ascii = isPersian ? PersianFont : EnglishFont,
                    HighAnsi = isPersian ? PersianFont : EnglishFont,
                    ComplexScript = isPersian ? PersianFont : EnglishFont
                },
                new W.FontSize() { Val = isPersian ? PersianFontSize : EnglishFontSize }
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

        private void InsertImageToParagraph(W.Paragraph paragraph, ImageItemDto imageDto, MainDocumentPart mainPart)
        {
            if (string.IsNullOrEmpty(imageDto.ImageBase64))
                return;

            try
            {
                var imageBytes = Convert.FromBase64String(imageDto.ImageBase64);
                if (imageBytes == null || imageBytes.Length == 0)
                    return;

                var imagePartType = ImagePartType.Jpeg;
                if (imageBytes.Length > 4)
                {
                    if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 &&
                        imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                    {
                        imagePartType = ImagePartType.Png;
                    }
                    else if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                    {
                        imagePartType = ImagePartType.Jpeg;
                    }
                }

                var imagePart = mainPart.AddImagePart(imagePartType);
                using (var stream = new MemoryStream(imageBytes))
                {
                    imagePart.FeedData(stream);
                }

                var imagePartId = mainPart.GetIdOfPart(imagePart);
                if (string.IsNullOrEmpty(imagePartId))
                    return;

                long cx = (long)(imageDto.Width * 9525);
                long cy = (long)(imageDto.Height * 9525);

                if (cy == 0 && cx > 0)
                {
                    cy = cx;
                }

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

                var run = new W.Run();

                var drawing = new W.Drawing();

                var inline = new DW.Inline(
                    new DW.Extent() { Cx = cx, Cy = cy },
                    new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties()
                    {
                        Id = (UInt32Value)(imageDto.Id ?? DateTime.Now.Ticks),
                        Name = imageDto.FileName ?? "Image"
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
                                        Name = imageDto.FileName ?? "Image"
                                    },
                                    new PIC.NonVisualPictureDrawingProperties()
                                ),
                                new PIC.BlipFill(
                                    new D.Blip() { Embed = imagePartId },
                                    new D.Stretch(new D.FillRectangle())
                                ),
                                new PIC.ShapeProperties(
                                    new D.Transform2D(
                                        new D.Offset() { X = 0L, Y = 0L },
                                        new D.Extents() { Cx = cx, Cy = cy }
                                    ),
                                    new D.PresetGeometry(new D.AdjustValueList())
                                    { Preset = D.ShapeTypeValues.Rectangle }
                                )
                            )
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                );

                drawing.Append(inline);
                run.Append(drawing);
                paragraph.Append(run);
            }
            catch (Exception ex)
            {
                var run = new W.Run(
                    new W.RunProperties(
                        new W.FontSize { Val = "24" },
                        new W.Color { Val = "FF0000" }
                    ),
                    new W.Text($"[خطا: {imageDto.FileName}]")
                );
                paragraph.Append(run);
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
                new W.Justification { Val = W.JustificationValues.Left },
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
                return false;

            return c == '[' || c == ']' || c == '{' || c == '}' ||
                   c == '.' || c == ',' || c == ';' || c == ':' || c == '!' || c == '?' ||
                   c == '@' || c == '#' || c == '$' || c == '%' || c == '^' || c == '&' ||
                   c == '*' || c == '+' || c == '=' || c == '<' || c == '>' || c == '/' ||
                   c == '\\' || c == '|' || c == '~' || c == '`' || c == '_' || c == '-';
        }

        #endregion
    }
}