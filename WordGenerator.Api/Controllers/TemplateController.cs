using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordGenerator.Api.Application.DTOs;
using WordGenerator.Api.Domain.Entities;
using WordGenerator.Api.Domain.Enums;
using WordGenerator.Api.Infra.Context;

namespace WordGenerator.Api.Controllers
{
    [ApiController]
    [Route("api/templates")]
    public class TemplateController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TemplateController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // CREATE TEMPLATE
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create(CreateDocumentTemplateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var template = new DocumentTemplate
                {
                    Name = dto.Name,
                    PageHeader = dto.PageHeader == null ? null : new PageHeader
                    {
                        HeaderText = dto.PageHeader.HeaderText,
                        IsActive = dto.PageHeader.IsActive,
                        Logos = dto.PageHeader.Logos?.Select(l => new HeaderLogo
                        {
                            FileName = l.FileName,
                            Order = l.Order,
                            Width = l.Width,
                            Height = l.Height,
                            ImageData = string.IsNullOrEmpty(l.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(l.ImageBase64)
                        }).ToList() ?? new List<HeaderLogo>()
                    },
                    CoverPage = dto.CoverPage == null ? null : new CoverPageTemplate
                    {
                        Title = dto.CoverPage.Title,
                        Items = dto.CoverPage.Items.OrderBy(x => x.Order).Select(x => new CoverPageItem
                        {
                            Label = x.Label,
                            Value = x.Value,
                            Order = x.Order
                        }).ToList(),
                        Elements = new List<ContentElement>()
                    },
                    MasterSections = new List<MasterSection>(),
                    Sections = new List<TemplateSection>()
                };

                _context.DocumentTemplates.Add(template);
                await _context.SaveChangesAsync();

                // ========== Cover Page Elements ==========
                if (dto.CoverPage != null && template.CoverPage != null)
                {
                    template.CoverPage.Elements = await MapContentElements(
                        dto.CoverPage.Elements,
                        null,
                        null,
                        null,
                        template.CoverPage.Id
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== Master Sections ==========
                foreach (var masterDto in dto.MasterSections.OrderBy(x => x.Order))
                {
                    var master = new MasterSection
                    {
                        Title = masterDto.Title,
                        Order = masterDto.Order,
                        ShowInToc = masterDto.ShowInToc,
                        TemplateId = template.Id,
                        SubSections = new List<SubSection>(),
                        Elements = new List<ContentElement>()
                    };

                    _context.MasterSections.Add(master);
                    await _context.SaveChangesAsync();

                    master.Elements = await MapContentElements(
                        masterDto.Elements,
                        master.Id,
                        null,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();

                    foreach (var subDto in masterDto.SubSections.OrderBy(x => x.Order))
                    {
                        var sub = new SubSection
                        {
                            Title = subDto.Title,
                            Order = subDto.Order,
                            ShowInToc = subDto.ShowInToc,
                            MasterSectionId = master.Id,
                            Elements = new List<ContentElement>()
                        };

                        _context.SubSections.Add(sub);
                        await _context.SaveChangesAsync();

                        sub.Elements = await MapContentElements(
                            subDto.Elements,
                            null,
                            sub.Id,
                            null,
                            null
                        );
                        await _context.SaveChangesAsync();
                    }
                }

                // ========== Legacy Sections ==========
                foreach (var sectionDto in dto.Sections.OrderBy(x => x.Order))
                {
                    var section = new TemplateSection
                    {
                        Title = sectionDto.Title,
                        Order = sectionDto.Order,
                        TemplateId = template.Id,
                        Elements = new List<ContentElement>()
                    };

                    _context.TemplateSections.Add(section);
                    await _context.SaveChangesAsync();

                    section.Elements = await MapContentElements(
                        sectionDto.Elements,
                        null,
                        null,
                        section.Id,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(new { Id = template.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================
        // GET ALL TEMPLATES
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.DocumentTemplates
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    HasCoverPage = x.CoverPage != null,
                    HasPageHeader = x.PageHeader != null,
                    MasterSectionsCount = x.MasterSections.Count,
                    SectionsCount = x.Sections.Count
                })
                .ToListAsync();

            return Ok(result);
        }

        // =========================
        // GET TEMPLATE BY ID
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var template = await _context.DocumentTemplates
                .AsNoTracking()
                .Include(x => x.PageHeader)
                    .ThenInclude(x => x.Logos)
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
                .FirstOrDefaultAsync(x => x.Id == id);

            if (template == null)
                return NotFound();

            return Ok(new
            {
                template.Id,
                template.Name,

                PageHeader = template.PageHeader == null ? null : new
                {
                    template.PageHeader.Id,
                    template.PageHeader.HeaderText,
                    template.PageHeader.IsActive,
                    Logos = template.PageHeader.Logos.OrderBy(l => l.Order).Select(l => new
                    {
                        l.Id,
                        l.FileName,
                        l.Order,
                        l.Width,
                        l.Height,
                        ImageBase64 = Convert.ToBase64String(l.ImageData)
                    })
                },

                CoverPage = template.CoverPage == null ? null : new
                {
                    template.CoverPage.Id,
                    template.CoverPage.Title,
                    Items = template.CoverPage.Items.OrderBy(x => x.Order).Select(x => new
                    {
                        x.Id,
                        x.Label,
                        x.Value,
                        x.Order
                    }),
                    Elements = template.CoverPage.Elements.OrderBy(e => e.Order).Select(e => MapContentElementToDto(e))
                },

                MasterSections = template.MasterSections.OrderBy(x => x.Order).Select((x, index) => new
                {
                    x.Id,
                    x.Title,
                    x.Order,
                    x.ShowInToc,
                    SectionNumber = (index + 1).ToString(),
                    Elements = x.Elements.OrderBy(e => e.Order).Select(e => MapContentElementToDto(e)),
                    SubSections = x.SubSections.OrderBy(s => s.Order).Select((s, subIndex) => new
                    {
                        s.Id,
                        s.Title,
                        s.Order,
                        s.ShowInToc,
                        SectionNumber = $"{index + 1}-{subIndex + 1}",
                        Elements = s.Elements.OrderBy(e => e.Order).Select(e => MapContentElementToDto(e))
                    })
                }),

                Sections = template.Sections.OrderBy(x => x.Order).Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Order,
                    Elements = x.Elements.OrderBy(e => e.Order).Select(e => MapContentElementToDto(e))
                })
            });
        }

        // =========================
        // DELETE TEMPLATE
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var template = await _context.DocumentTemplates
                .FirstOrDefaultAsync(x => x.Id == id);

            if (template == null)
                return NotFound();

            _context.DocumentTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // =========================
        // UPDATE TEMPLATE
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, CreateDocumentTemplateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingTemplate = await _context.DocumentTemplates
                    .Include(x => x.PageHeader)
                        .ThenInclude(x => x.Logos)
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
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existingTemplate == null)
                    return NotFound();

                // ========== 1. حذف سلول‌های جداول ==========
                await DeleteAllCells(existingTemplate);
                await _context.SaveChangesAsync();

                // ========== 2. حذف ردیف‌های جداول ==========
                await DeleteAllRows(existingTemplate);
                await _context.SaveChangesAsync();

                // ========== 3. حذف ستون‌های جداول ==========
                await DeleteAllColumns(existingTemplate);
                await _context.SaveChangesAsync();

                // ========== 4. حذف جداول ==========
                await DeleteAllTables(existingTemplate);
                await _context.SaveChangesAsync();

                // ========== 5. حذف تمام Elementها ==========
                await DeleteAllElements(existingTemplate);
                await _context.SaveChangesAsync();

                // ========== 6. حذف تصاویر (Images) ==========
                await DeleteAllImages(existingTemplate);
                await _context.SaveChangesAsync();

                // ========== 7. حذف زیربخش‌ها ==========
                foreach (var master in existingTemplate.MasterSections)
                {
                    if (master.SubSections.Any())
                        _context.SubSections.RemoveRange(master.SubSections);
                }
                await _context.SaveChangesAsync();

                // ========== 8. حذف بخش‌های اصلی ==========
                if (existingTemplate.MasterSections.Any())
                    _context.MasterSections.RemoveRange(existingTemplate.MasterSections);
                await _context.SaveChangesAsync();

                // ========== 9. حذف بخش‌های قدیمی ==========
                if (existingTemplate.Sections.Any())
                    _context.TemplateSections.RemoveRange(existingTemplate.Sections);
                await _context.SaveChangesAsync();

                // ========== 10. حذف آیتم‌های کاورپیج ==========
                if (existingTemplate.CoverPage != null && existingTemplate.CoverPage.Items.Any())
                    _context.CoverPageItems.RemoveRange(existingTemplate.CoverPage.Items);
                await _context.SaveChangesAsync();

                // ========== 11. حذف کاورپیج ==========
                if (existingTemplate.CoverPage != null)
                    _context.CoverPageTemplates.Remove(existingTemplate.CoverPage);
                await _context.SaveChangesAsync();

                // ========== 12. حذف PageHeader و لوگوها ==========
                if (existingTemplate.PageHeader != null)
                {
                    if (existingTemplate.PageHeader.Logos.Any())
                        _context.HeaderLogos.RemoveRange(existingTemplate.PageHeader.Logos);
                    _context.PageHeaders.Remove(existingTemplate.PageHeader);
                }
                await _context.SaveChangesAsync();

                // ========== 13. حذف خود تمپلیت ==========
                _context.DocumentTemplates.Remove(existingTemplate);
                await _context.SaveChangesAsync();

                // ========== 14. ایجاد تمپلیت جدید ==========
                var newTemplate = new DocumentTemplate
                {
                    Name = dto.Name,
                    PageHeader = dto.PageHeader == null ? null : new PageHeader
                    {
                        HeaderText = dto.PageHeader.HeaderText,
                        IsActive = dto.PageHeader.IsActive,
                        Logos = dto.PageHeader.Logos?.Select(l => new HeaderLogo
                        {
                            FileName = l.FileName,
                            Order = l.Order,
                            Width = l.Width,
                            Height = l.Height,
                            ImageData = string.IsNullOrEmpty(l.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(l.ImageBase64)
                        }).ToList() ?? new List<HeaderLogo>()
                    },
                    CoverPage = dto.CoverPage == null ? null : new CoverPageTemplate
                    {
                        Title = dto.CoverPage.Title,
                        Items = dto.CoverPage.Items.OrderBy(x => x.Order).Select(x => new CoverPageItem
                        {
                            Label = x.Label,
                            Value = x.Value,
                            Order = x.Order
                        }).ToList(),
                        Elements = new List<ContentElement>()
                    },
                    MasterSections = new List<MasterSection>(),
                    Sections = new List<TemplateSection>()
                };

                _context.DocumentTemplates.Add(newTemplate);
                await _context.SaveChangesAsync();

                // ========== 15. اضافه کردن Cover Page Elements ==========
                if (dto.CoverPage != null && newTemplate.CoverPage != null)
                {
                    newTemplate.CoverPage.Elements = await MapContentElements(
                        dto.CoverPage.Elements,
                        null,
                        null,
                        null,
                        newTemplate.CoverPage.Id
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== 16. اضافه کردن Master Sections ==========
                foreach (var masterDto in dto.MasterSections.OrderBy(x => x.Order))
                {
                    var master = new MasterSection
                    {
                        Title = masterDto.Title,
                        Order = masterDto.Order,
                        ShowInToc = masterDto.ShowInToc,
                        TemplateId = newTemplate.Id,
                        SubSections = new List<SubSection>(),
                        Elements = new List<ContentElement>()
                    };

                    _context.MasterSections.Add(master);
                    await _context.SaveChangesAsync();

                    master.Elements = await MapContentElements(
                        masterDto.Elements,
                        master.Id,
                        null,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();

                    foreach (var subDto in masterDto.SubSections.OrderBy(x => x.Order))
                    {
                        var sub = new SubSection
                        {
                            Title = subDto.Title,
                            Order = subDto.Order,
                            ShowInToc = subDto.ShowInToc,
                            MasterSectionId = master.Id,
                            Elements = new List<ContentElement>()
                        };

                        _context.SubSections.Add(sub);
                        await _context.SaveChangesAsync();

                        sub.Elements = await MapContentElements(
                            subDto.Elements,
                            null,
                            sub.Id,
                            null,
                            null
                        );
                        await _context.SaveChangesAsync();
                    }
                }

                // ========== 17. اضافه کردن Legacy Sections ==========
                foreach (var sectionDto in dto.Sections.OrderBy(x => x.Order))
                {
                    var section = new TemplateSection
                    {
                        Title = sectionDto.Title,
                        Order = sectionDto.Order,
                        TemplateId = newTemplate.Id,
                        Elements = new List<ContentElement>()
                    };

                    _context.TemplateSections.Add(section);
                    await _context.SaveChangesAsync();

                    section.Elements = await MapContentElements(
                        sectionDto.Elements,
                        null,
                        null,
                        section.Id,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(new { Id = newTemplate.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================
        // Helper Methods
        // =========================

        private async Task<List<ContentElement>> MapContentElements(
            List<ContentElementDto> elements,
            long? masterSectionId,
            long? subSectionId,
            long? templateSectionId,
            long? coverPageId)
        {
            var result = new List<ContentElement>();

            if (elements == null || !elements.Any())
                return result;

            foreach (var elementDto in elements.OrderBy(x => x.Order))
            {
                var element = new ContentElement
                {
                    Type = elementDto.Type switch
                    {
                        "paragraph" => ContentElementType.Paragraph,
                        "image" => ContentElementType.Image,
                        "table" => ContentElementType.Table,
                        _ => ContentElementType.Paragraph
                    },
                    Order = elementDto.Order,
                    MasterSectionId = masterSectionId,
                    SubSectionId = subSectionId,
                    TemplateSectionId = templateSectionId,
                    CoverPageId = coverPageId
                };

                switch (element.Type)
                {
                    case ContentElementType.Paragraph:
                        element.ParagraphText = elementDto.Text;
                        break;

                    case ContentElementType.Image when elementDto.Image != null:
                        element.Image = new ImageItem
                        {
                            FileName = elementDto.Image.FileName,
                            Caption = elementDto.Image.Caption,
                            Order = elementDto.Order,
                            Width = elementDto.Image.Width,
                            Height = elementDto.Image.Height,
                            ImageData = string.IsNullOrEmpty(elementDto.Image.ImageBase64)
                                ? Array.Empty<byte>()
                                : Convert.FromBase64String(elementDto.Image.ImageBase64)
                        };
                        break;

                    case ContentElementType.Table when elementDto.Table != null:
                        var table = new DynamicTable
                        {
                            Title = elementDto.Table.Title,
                            Order = elementDto.Table.Order,
                            ShowRowNumbers = elementDto.Table.ShowRowNumbers,
                            RowNumberHeader = elementDto.Table.RowNumberHeader,
                            Columns = elementDto.Table.Columns.OrderBy(c => c.Order).Select(c => new TableColumnDefinition
                            {
                                Header = c.Header,
                                Width = c.Width,
                                Order = c.Order
                            }).ToList()
                        };

                        _context.DynamicTables.Add(table);
                        await _context.SaveChangesAsync();

                        var columns = table.Columns.OrderBy(c => c.Order).ToList();
                        foreach (var rowDto in elementDto.Table.Rows.OrderBy(x => x.RowNumber))
                        {
                            var row = new TableDataRow
                            {
                                RowNumber = rowDto.RowNumber,
                                DynamicTableId = table.Id,
                                Cells = new List<TableDataCell>()
                            };

                            for (int i = 0; i < columns.Count && i < (rowDto.Values?.Count ?? 0); i++)
                            {
                                row.Cells.Add(new TableDataCell
                                {
                                    TableColumnDefinitionId = columns[i].Id,
                                    Value = rowDto.Values[i] ?? ""
                                });
                            }

                            _context.TableRows.Add(row);
                        }

                        await _context.SaveChangesAsync();

                        element.Table = table;
                        break;
                }

                result.Add(element);
            }

            return result;
        }

        private object MapContentElementToDto(ContentElement element)
        {
            var baseObj = new
            {
                element.Id,
                Type = element.Type.ToString().ToLower(),
                element.Order
            };

            return element.Type switch
            {
                ContentElementType.Paragraph => new
                {
                    baseObj.Id,
                    baseObj.Type,
                    baseObj.Order,
                    Text = element.ParagraphText
                },
                ContentElementType.Image when element.Image != null => new
                {
                    baseObj.Id,
                    baseObj.Type,
                    baseObj.Order,
                    Image = new
                    {
                        element.Image.Id,
                        element.Image.FileName,
                        element.Image.Caption,
                        element.Image.Order,
                        element.Image.Width,
                        element.Image.Height,
                        ImageBase64 = Convert.ToBase64String(element.Image.ImageData)
                    }
                },
                ContentElementType.Table when element.Table != null => new
                {
                    baseObj.Id,
                    baseObj.Type,
                    baseObj.Order,
                    table = new
                    {
                        element.Table.Id,
                        element.Table.Title,
                        element.Table.Order,
                        element.Table.ShowRowNumbers,
                        element.Table.RowNumberHeader,
                        columns = element.Table.Columns.OrderBy(c => c.Order).Select(c => new
                        {
                            c.Id,
                            c.Header,
                            c.Width,
                            c.Order
                        }),
                        rows = element.Table.Rows.OrderBy(r => r.RowNumber).Select(r => new
                        {
                            r.Id,
                            r.RowNumber,
                            cells = r.Cells.OrderBy(c => c.Column.Order).Select(c => new
                            {
                                c.Id,
                                ColumnId = c.TableColumnDefinitionId,
                                c.Value
                            }).ToList()
                        })
                    }
                },
                _ => baseObj
            };
        }

        // ========== متدهای کمکی حذف ==========

        private async Task DeleteAllCells(DocumentTemplate template)
        {
            var allCells = new List<TableDataCell>();

            if (template.CoverPage != null)
            {
                foreach (var element in template.CoverPage.Elements.Where(e => e.Table != null))
                {
                    foreach (var row in element.Table.Rows)
                        allCells.AddRange(row.Cells);
                }
            }

            foreach (var master in template.MasterSections)
            {
                foreach (var element in master.Elements.Where(e => e.Table != null))
                {
                    foreach (var row in element.Table.Rows)
                        allCells.AddRange(row.Cells);
                }
                foreach (var sub in master.SubSections)
                {
                    foreach (var element in sub.Elements.Where(e => e.Table != null))
                    {
                        foreach (var row in element.Table.Rows)
                            allCells.AddRange(row.Cells);
                    }
                }
            }

            foreach (var section in template.Sections)
            {
                foreach (var element in section.Elements.Where(e => e.Table != null))
                {
                    foreach (var row in element.Table.Rows)
                        allCells.AddRange(row.Cells);
                }
            }

            if (allCells.Any())
                _context.TableCells.RemoveRange(allCells);

            await _context.SaveChangesAsync();
        }

        private async Task DeleteAllRows(DocumentTemplate template)
        {
            var allRows = new List<TableDataRow>();

            if (template.CoverPage != null)
            {
                foreach (var element in template.CoverPage.Elements.Where(e => e.Table != null))
                    allRows.AddRange(element.Table.Rows);
            }

            foreach (var master in template.MasterSections)
            {
                foreach (var element in master.Elements.Where(e => e.Table != null))
                    allRows.AddRange(element.Table.Rows);
                foreach (var sub in master.SubSections)
                {
                    foreach (var element in sub.Elements.Where(e => e.Table != null))
                        allRows.AddRange(element.Table.Rows);
                }
            }

            foreach (var section in template.Sections)
            {
                foreach (var element in section.Elements.Where(e => e.Table != null))
                    allRows.AddRange(element.Table.Rows);
            }

            if (allRows.Any())
                _context.TableRows.RemoveRange(allRows);

            await _context.SaveChangesAsync();
        }

        private async Task DeleteAllColumns(DocumentTemplate template)
        {
            var allColumns = new List<TableColumnDefinition>();

            if (template.CoverPage != null)
            {
                foreach (var element in template.CoverPage.Elements.Where(e => e.Table != null))
                    allColumns.AddRange(element.Table.Columns);
            }

            foreach (var master in template.MasterSections)
            {
                foreach (var element in master.Elements.Where(e => e.Table != null))
                    allColumns.AddRange(element.Table.Columns);
                foreach (var sub in master.SubSections)
                {
                    foreach (var element in sub.Elements.Where(e => e.Table != null))
                        allColumns.AddRange(element.Table.Columns);
                }
            }

            foreach (var section in template.Sections)
            {
                foreach (var element in section.Elements.Where(e => e.Table != null))
                    allColumns.AddRange(element.Table.Columns);
            }

            if (allColumns.Any())
                _context.TableColumns.RemoveRange(allColumns);

            await _context.SaveChangesAsync();
        }

        private async Task DeleteAllTables(DocumentTemplate template)
        {
            var allTables = new List<DynamicTable>();

            if (template.CoverPage != null)
            {
                foreach (var element in template.CoverPage.Elements.Where(e => e.Table != null))
                    allTables.Add(element.Table);
            }

            foreach (var master in template.MasterSections)
            {
                foreach (var element in master.Elements.Where(e => e.Table != null))
                    allTables.Add(element.Table);
                foreach (var sub in master.SubSections)
                {
                    foreach (var element in sub.Elements.Where(e => e.Table != null))
                        allTables.Add(element.Table);
                }
            }

            foreach (var section in template.Sections)
            {
                foreach (var element in section.Elements.Where(e => e.Table != null))
                    allTables.Add(element.Table);
            }

            if (allTables.Any())
                _context.DynamicTables.RemoveRange(allTables);

            await _context.SaveChangesAsync();
        }

        private async Task DeleteAllElements(DocumentTemplate template)
        {
            var allElements = new List<ContentElement>();

            if (template.CoverPage != null)
                allElements.AddRange(template.CoverPage.Elements);

            foreach (var master in template.MasterSections)
            {
                allElements.AddRange(master.Elements);
                foreach (var sub in master.SubSections)
                    allElements.AddRange(sub.Elements);
            }

            foreach (var section in template.Sections)
                allElements.AddRange(section.Elements);

            if (allElements.Any())
                _context.ContentElements.RemoveRange(allElements);

            await _context.SaveChangesAsync();
        }

        private async Task DeleteAllImages(DocumentTemplate template)
        {
            var allImages = new List<ImageItem>();

            if (template.CoverPage != null)
            {
                foreach (var element in template.CoverPage.Elements.Where(e => e.Image != null))
                    allImages.Add(element.Image);
            }

            foreach (var master in template.MasterSections)
            {
                foreach (var element in master.Elements.Where(e => e.Image != null))
                    allImages.Add(element.Image);
                foreach (var sub in master.SubSections)
                {
                    foreach (var element in sub.Elements.Where(e => e.Image != null))
                        allImages.Add(element.Image);
                }
            }

            foreach (var section in template.Sections)
            {
                foreach (var element in section.Elements.Where(e => e.Image != null))
                    allImages.Add(element.Image);
            }

            if (allImages.Any())
                _context.Images.RemoveRange(allImages);

            await _context.SaveChangesAsync();
        }
    }
}