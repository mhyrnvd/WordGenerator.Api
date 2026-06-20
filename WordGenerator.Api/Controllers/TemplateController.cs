using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordGenerator.Api.Application.DTOs;
using WordGenerator.Api.Domain.Entities;
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
                    CoverPage = dto.CoverPage == null ? null : new CoverPageTemplate
                    {
                        Title = dto.CoverPage.Title,
                        Items = dto.CoverPage.Items.OrderBy(x => x.Order).Select(x => new CoverPageItem
                        {
                            Label = x.Label,
                            Value = x.Value,
                            Order = x.Order
                        }).ToList(),
                        ImageGroups = dto.CoverPage.ImageGroups?.OrderBy(g => g.Order).Select(g => new ImageGroup
                        {
                            Title = g.Title,
                            Order = g.Order,
                            ImagesPerRow = g.ImagesPerRow > 0 ? g.ImagesPerRow : 2,
                            Images = g.Images.Select(img => new ImageItem
                            {
                                FileName = img.FileName,
                                Caption = img.Caption,
                                Order = img.Order,
                                Width = img.Width,
                                Height = img.Height,
                                ImageData = string.IsNullOrEmpty(img.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(img.ImageBase64)
                            }).ToList()
                        }).ToList() ?? new List<ImageGroup>(),
                        Tables = dto.CoverPage.Tables.Select(x => new DynamicTable
                        {
                            Title = x.Title,
                            Order = x.Order,
                            ShowRowNumbers = x.ShowRowNumbers,
                            RowNumberHeader = x.RowNumberHeader,
                            Columns = x.Columns.OrderBy(c => c.Order).Select(c => new TableColumnDefinition
                            {
                                Header = c.Header,
                                Width = c.Width,
                                Order = c.Order
                            }).ToList()
                        }).ToList()
                    },
                    MasterSections = dto.MasterSections.OrderBy(x => x.Order).Select(x => new MasterSection
                    {
                        Title = x.Title,
                        Order = x.Order,
                        ShowInToc = x.ShowInToc,
                        Paragraphs = x.Paragraphs.OrderBy(p => p.Order).Select(p => new MasterSectionParagraph
                        {
                            Text = p.Text,
                            Order = p.Order
                        }).ToList(),
                        ImageGroups = x.ImageGroups?.OrderBy(g => g.Order).Select(g => new ImageGroup
                        {
                            Title = g.Title,
                            Order = g.Order,
                            ImagesPerRow = g.ImagesPerRow > 0 ? g.ImagesPerRow : 2,
                            Images = g.Images.Select(img => new ImageItem
                            {
                                FileName = img.FileName,
                                Caption = img.Caption,
                                Order = img.Order,
                                Width = img.Width,
                                Height = img.Height,
                                ImageData = string.IsNullOrEmpty(img.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(img.ImageBase64)
                            }).ToList()
                        }).ToList() ?? new List<ImageGroup>(),
                        SubSections = x.SubSections.OrderBy(s => s.Order).Select(s => new SubSection
                        {
                            Title = s.Title,
                            Order = s.Order,
                            ShowInToc = s.ShowInToc,
                            Paragraphs = s.Paragraphs.OrderBy(p => p.Order).Select(p => new SubSectionParagraph
                            {
                                Text = p.Text,
                                Order = p.Order
                            }).ToList(),
                            ImageGroups = s.ImageGroups?.OrderBy(g => g.Order).Select(g => new ImageGroup
                            {
                                Title = g.Title,
                                Order = g.Order,
                                ImagesPerRow = g.ImagesPerRow > 0 ? g.ImagesPerRow : 2,
                                Images = g.Images.Select(img => new ImageItem
                                {
                                    FileName = img.FileName,
                                    Caption = img.Caption,
                                    Order = img.Order,
                                    Width = img.Width,
                                    Height = img.Height,
                                    ImageData = string.IsNullOrEmpty(img.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(img.ImageBase64)
                                }).ToList()
                            }).ToList() ?? new List<ImageGroup>(),
                            Tables = s.Tables.Select(t => new DynamicTable
                            {
                                Title = t.Title,
                                Order = t.Order,
                                ShowRowNumbers = t.ShowRowNumbers,
                                RowNumberHeader = t.RowNumberHeader,
                                Columns = t.Columns.OrderBy(c => c.Order).Select(c => new TableColumnDefinition
                                {
                                    Header = c.Header,
                                    Width = c.Width,
                                    Order = c.Order
                                }).ToList()
                            }).ToList()
                        }).ToList(),
                        Tables = x.Tables.Select(t => new DynamicTable
                        {
                            Title = t.Title,
                            Order = t.Order,
                            ShowRowNumbers = t.ShowRowNumbers,
                            RowNumberHeader = t.RowNumberHeader,
                            Columns = t.Columns.OrderBy(c => c.Order).Select(c => new TableColumnDefinition
                            {
                                Header = c.Header,
                                Width = c.Width,
                                Order = c.Order
                            }).ToList()
                        }).ToList()
                    }).ToList(),
                    Sections = dto.Sections.OrderBy(x => x.Order).Select(x => new TemplateSection
                    {
                        Title = x.Title,
                        Order = x.Order,
                        Paragraphs = x.Paragraphs.OrderBy(p => p.Order).Select(p => new SectionParagraph
                        {
                            Text = p.Text,
                            Order = p.Order
                        }).ToList(),
                        ImageGroups = x.ImageGroups?.OrderBy(g => g.Order).Select(g => new ImageGroup
                        {
                            Title = g.Title,
                            Order = g.Order,
                            ImagesPerRow = g.ImagesPerRow > 0 ? g.ImagesPerRow : 2,
                            Images = g.Images.Select(img => new ImageItem
                            {
                                FileName = img.FileName,
                                Caption = img.Caption,
                                Order = img.Order,
                                Width = img.Width,
                                Height = img.Height,
                                ImageData = string.IsNullOrEmpty(img.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(img.ImageBase64)
                            }).ToList()
                        }).ToList() ?? new List<ImageGroup>(),
                        Tables = x.Tables.Select(t => new DynamicTable
                        {
                            Title = t.Title,
                            Order = t.Order,
                            ShowRowNumbers = t.ShowRowNumbers,
                            RowNumberHeader = t.RowNumberHeader,
                            Columns = t.Columns.OrderBy(c => c.Order).Select(c => new TableColumnDefinition
                            {
                                Header = c.Header,
                                Width = c.Width,
                                Order = c.Order
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };

                _context.DocumentTemplates.Add(template);
                await _context.SaveChangesAsync();

                await AddAllTableRowsAndCells(dto, template);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Id = template.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task AddAllTableRowsAndCells(CreateDocumentTemplateDto dto, DocumentTemplate template)
        {
            // Cover Page Tables
            if (dto.CoverPage != null && template.CoverPage != null)
            {
                var coverPageTables = template.CoverPage.Tables.ToList();
                for (int i = 0; i < dto.CoverPage.Tables.Count && i < coverPageTables.Count; i++)
                {
                    var tableDto = dto.CoverPage.Tables[i];
                    var table = coverPageTables[i];
                    await AddRowsToTable(tableDto, table);
                }
            }

            // MasterSections Tables
            var masterSectionsList = template.MasterSections.ToList();
            for (int m = 0; m < dto.MasterSections.Count && m < masterSectionsList.Count; m++)
            {
                var masterDto = dto.MasterSections[m];
                var master = masterSectionsList[m];

                var masterTables = master.Tables.ToList();
                for (int t = 0; t < masterDto.Tables.Count && t < masterTables.Count; t++)
                {
                    var tableDto = masterDto.Tables[t];
                    var table = masterTables[t];
                    await AddRowsToTable(tableDto, table);
                }

                var subSectionsList = master.SubSections.ToList();
                for (int s = 0; s < masterDto.SubSections.Count && s < subSectionsList.Count; s++)
                {
                    var subDto = masterDto.SubSections[s];
                    var sub = subSectionsList[s];

                    var subTables = sub.Tables.ToList();
                    for (int t = 0; t < subDto.Tables.Count && t < subTables.Count; t++)
                    {
                        var tableDto = subDto.Tables[t];
                        var table = subTables[t];
                        await AddRowsToTable(tableDto, table);
                    }
                }
            }

            // Sections Tables
            var sectionsList = template.Sections.ToList();
            for (int s = 0; s < dto.Sections.Count && s < sectionsList.Count; s++)
            {
                var sectionDto = dto.Sections[s];
                var section = sectionsList[s];

                var sectionTables = section.Tables.ToList();
                for (int t = 0; t < sectionDto.Tables.Count && t < sectionTables.Count; t++)
                {
                    var tableDto = sectionDto.Tables[t];
                    var table = sectionTables[t];
                    await AddRowsToTable(tableDto, table);
                }
            }
        }

        private async Task AddRowsToTable(CreateDynamicTableDto tableDto, DynamicTable table)
        {
            if (tableDto == null || table == null) return;

            var columns = table.Columns?.OrderBy(c => c.Order).ToList() ?? new List<TableColumnDefinition>();

            if (columns.Count == 0) return;

            foreach (var rowDto in tableDto.Rows?.OrderBy(x => x.RowNumber) ?? Enumerable.Empty<CreateTableRowDto>())
            {
                var row = new TableDataRow
                {
                    RowNumber = rowDto.RowNumber,
                    DynamicTableId = table.Id,
                    Cells = new List<TableDataCell>()
                };

                for (int i = 0; i < columns.Count && i < (rowDto.Values?.Count ?? 0); i++)
                {
                    if (columns[i] != null)
                    {
                        row.Cells.Add(new TableDataCell
                        {
                            TableColumnDefinitionId = columns[i].Id,
                            Value = rowDto.Values[i] ?? ""
                        });
                    }
                }

                _context.TableRows.Add(row);
            }

            await _context.SaveChangesAsync();
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
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Items)
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.ImageGroups)
                        .ThenInclude(g => g.Images)
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Tables)
                        .ThenInclude(t => t.Columns)
                .Include(x => x.CoverPage)
                    .ThenInclude(x => x.Tables)
                        .ThenInclude(t => t.Rows)
                            .ThenInclude(r => r.Cells)
                                .ThenInclude(c => c.Column)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Paragraphs)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.ImageGroups)
                        .ThenInclude(g => g.Images)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Paragraphs)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.ImageGroups)
                            .ThenInclude(g => g.Images)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Tables)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.SubSections)
                        .ThenInclude(s => s.Tables)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                                    .ThenInclude(c => c.Column)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Tables)
                        .ThenInclude(t => t.Columns)
                .Include(x => x.MasterSections)
                    .ThenInclude(m => m.Tables)
                        .ThenInclude(t => t.Rows)
                            .ThenInclude(r => r.Cells)
                                .ThenInclude(c => c.Column)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Paragraphs)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.ImageGroups)
                        .ThenInclude(g => g.Images)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Tables)
                        .ThenInclude(t => t.Columns)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Tables)
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
                    ImageGroups = template.CoverPage.ImageGroups.OrderBy(g => g.Order).Select(g => new
                    {
                        g.Id,
                        g.Title,
                        g.Order,
                        g.ImagesPerRow,
                        Images = g.Images.OrderBy(i => i.Order).Select(i => new
                        {
                            i.Id,
                            i.FileName,
                            i.Caption,
                            i.Order,
                            i.Width,
                            i.Height,
                            ImageBase64 = Convert.ToBase64String(i.ImageData)
                        })
                    }),
                    Tables = template.CoverPage.Tables.OrderBy(x => x.Order).Select(x => new
                    {
                        x.Id,
                        x.Title,
                        x.Order,
                        x.ShowRowNumbers,
                        x.RowNumberHeader,
                        Columns = x.Columns.OrderBy(c => c.Order).Select(c => new
                        {
                            c.Id,
                            c.Header,
                            c.Width,
                            c.Order,
                            c.IsRowNumberColumn
                        }),
                        Rows = x.Rows.OrderBy(r => r.RowNumber).Select(r => new
                        {
                            r.Id,
                            r.RowNumber,
                            Cells = r.Cells
                                .Where(c => c.Column != null)
                                .OrderBy(c => c.Column.Order)
                                .Select(c => new
                                {
                                    c.Id,
                                    ColumnId = c.TableColumnDefinitionId,
                                    c.Value
                                })
                        })
                    })
                },

                MasterSections = template.MasterSections.OrderBy(x => x.Order).Select((x, index) => new
                {
                    x.Id,
                    x.Title,
                    x.Order,
                    x.ShowInToc,
                    SectionNumber = (index + 1).ToString(),
                    Paragraphs = x.Paragraphs.OrderBy(p => p.Order).Select(p => new
                    {
                        p.Id,
                        p.Text,
                        p.Order
                    }),
                    ImageGroups = x.ImageGroups.OrderBy(g => g.Order).Select(g => new
                    {
                        g.Id,
                        g.Title,
                        g.Order,
                        g.ImagesPerRow,
                        Images = g.Images.OrderBy(i => i.Order).Select(i => new
                        {
                            i.Id,
                            i.FileName,
                            i.Caption,
                            i.Order,
                            i.Width,
                            i.Height,
                            ImageBase64 = Convert.ToBase64String(i.ImageData)
                        })
                    }),
                    SubSections = x.SubSections.OrderBy(s => s.Order).Select((s, subIndex) => new
                    {
                        s.Id,
                        s.Title,
                        s.Order,
                        s.ShowInToc,
                        SectionNumber = $"{index + 1}-{subIndex + 1}",
                        Paragraphs = s.Paragraphs.OrderBy(p => p.Order).Select(p => new
                        {
                            p.Id,
                            p.Text,
                            p.Order
                        }),
                        ImageGroups = s.ImageGroups.OrderBy(g => g.Order).Select(g => new
                        {
                            g.Id,
                            g.Title,
                            g.Order,
                            g.ImagesPerRow,
                            Images = g.Images.OrderBy(i => i.Order).Select(i => new
                            {
                                i.Id,
                                i.FileName,
                                i.Caption,
                                i.Order,
                                i.Width,
                                i.Height,
                                ImageBase64 = Convert.ToBase64String(i.ImageData)
                            })
                        }),
                        Tables = s.Tables.OrderBy(t => t.Order).Select(t => new
                        {
                            t.Id,
                            t.Title,
                            t.Order,
                            t.ShowRowNumbers,
                            t.RowNumberHeader,
                            Columns = t.Columns.OrderBy(c => c.Order).Select(c => new
                            {
                                c.Id,
                                c.Header,
                                c.Width,
                                c.Order,
                                c.IsRowNumberColumn
                            }),
                            Rows = t.Rows.OrderBy(r => r.RowNumber).Select(r => new
                            {
                                r.Id,
                                r.RowNumber,
                                Cells = r.Cells
                                    .Where(c => c.Column != null)
                                    .OrderBy(c => c.Column.Order)
                                    .Select(c => new
                                    {
                                        c.Id,
                                        ColumnId = c.TableColumnDefinitionId,
                                        c.Value
                                    })
                            })
                        })
                    }),
                    Tables = x.Tables.OrderBy(t => t.Order).Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Order,
                        t.ShowRowNumbers,
                        t.RowNumberHeader,
                        Columns = t.Columns.OrderBy(c => c.Order).Select(c => new
                        {
                            c.Id,
                            c.Header,
                            c.Width,
                            c.Order,
                            c.IsRowNumberColumn
                        }),
                        Rows = t.Rows.OrderBy(r => r.RowNumber).Select(r => new
                        {
                            r.Id,
                            r.RowNumber,
                            Cells = r.Cells
                                .Where(c => c.Column != null)
                                .OrderBy(c => c.Column.Order)
                                .Select(c => new
                                {
                                    c.Id,
                                    ColumnId = c.TableColumnDefinitionId,
                                    c.Value
                                })
                        })
                    })
                }),

                Sections = template.Sections.OrderBy(x => x.Order).Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Order,
                    Paragraphs = x.Paragraphs.OrderBy(p => p.Order).Select(p => new
                    {
                        p.Id,
                        p.Text,
                        p.Order
                    }),
                    ImageGroups = x.ImageGroups.OrderBy(g => g.Order).Select(g => new
                    {
                        g.Id,
                        g.Title,
                        g.Order,
                        g.ImagesPerRow,
                        Images = g.Images.OrderBy(i => i.Order).Select(i => new
                        {
                            i.Id,
                            i.FileName,
                            i.Caption,
                            i.Order,
                            i.Width,
                            i.Height,
                            ImageBase64 = Convert.ToBase64String(i.ImageData)
                        })
                    }),
                    Tables = x.Tables.OrderBy(t => t.Order).Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Order,
                        t.ShowRowNumbers,
                        t.RowNumberHeader,
                        Columns = t.Columns.OrderBy(c => c.Order).Select(c => new
                        {
                            c.Id,
                            c.Header,
                            c.Width,
                            c.Order,
                            c.IsRowNumberColumn
                        }),
                        Rows = t.Rows.OrderBy(r => r.RowNumber).Select(r => new
                        {
                            r.Id,
                            r.RowNumber,
                            Cells = r.Cells
                                .Where(c => c.Column != null)
                                .OrderBy(c => c.Column.Order)
                                .Select(c => new
                                {
                                    c.Id,
                                    ColumnId = c.TableColumnDefinitionId,
                                    c.Value
                                })
                        })
                    })
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
                    .Include(x => x.CoverPage)
                        .ThenInclude(x => x.Items)
                    .Include(x => x.CoverPage)
                        .ThenInclude(x => x.ImageGroups)
                            .ThenInclude(g => g.Images)
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
                        .ThenInclude(m => m.ImageGroups)
                            .ThenInclude(g => g.Images)
                    .Include(x => x.MasterSections)
                        .ThenInclude(m => m.SubSections)
                            .ThenInclude(s => s.Paragraphs)
                    .Include(x => x.MasterSections)
                        .ThenInclude(m => m.SubSections)
                            .ThenInclude(s => s.ImageGroups)
                                .ThenInclude(g => g.Images)
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
                        .ThenInclude(s => s.ImageGroups)
                            .ThenInclude(g => g.Images)
                    .Include(x => x.Sections)
                        .ThenInclude(s => s.Tables)
                            .ThenInclude(t => t.Columns)
                    .Include(x => x.Sections)
                        .ThenInclude(s => s.Tables)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existingTemplate == null)
                    return NotFound();

                // ========== 1. Delete Cells ==========
                if (existingTemplate.CoverPage != null)
                {
                    foreach (var table in existingTemplate.CoverPage.Tables)
                        foreach (var row in table.Rows)
                            if (row.Cells.Any()) _context.TableCells.RemoveRange(row.Cells);
                }

                foreach (var master in existingTemplate.MasterSections)
                {
                    foreach (var table in master.Tables)
                        foreach (var row in table.Rows)
                            if (row.Cells.Any()) _context.TableCells.RemoveRange(row.Cells);

                    foreach (var sub in master.SubSections)
                        foreach (var table in sub.Tables)
                            foreach (var row in table.Rows)
                                if (row.Cells.Any()) _context.TableCells.RemoveRange(row.Cells);
                }

                foreach (var section in existingTemplate.Sections)
                {
                    foreach (var table in section.Tables)
                        foreach (var row in table.Rows)
                            if (row.Cells.Any()) _context.TableCells.RemoveRange(row.Cells);
                }
                await _context.SaveChangesAsync();

                // ========== 2. Delete Rows ==========
                if (existingTemplate.CoverPage != null)
                    foreach (var table in existingTemplate.CoverPage.Tables)
                        if (table.Rows.Any()) _context.TableRows.RemoveRange(table.Rows);

                foreach (var master in existingTemplate.MasterSections)
                {
                    foreach (var table in master.Tables)
                        if (table.Rows.Any()) _context.TableRows.RemoveRange(table.Rows);
                    foreach (var sub in master.SubSections)
                        foreach (var table in sub.Tables)
                            if (table.Rows.Any()) _context.TableRows.RemoveRange(table.Rows);
                }

                foreach (var section in existingTemplate.Sections)
                    foreach (var table in section.Tables)
                        if (table.Rows.Any()) _context.TableRows.RemoveRange(table.Rows);
                await _context.SaveChangesAsync();

                // ========== 3. Delete Columns ==========
                if (existingTemplate.CoverPage != null)
                    foreach (var table in existingTemplate.CoverPage.Tables)
                        if (table.Columns.Any()) _context.TableColumns.RemoveRange(table.Columns);

                foreach (var master in existingTemplate.MasterSections)
                {
                    foreach (var table in master.Tables)
                        if (table.Columns.Any()) _context.TableColumns.RemoveRange(table.Columns);
                    foreach (var sub in master.SubSections)
                        foreach (var table in sub.Tables)
                            if (table.Columns.Any()) _context.TableColumns.RemoveRange(table.Columns);
                }

                foreach (var section in existingTemplate.Sections)
                    foreach (var table in section.Tables)
                        if (table.Columns.Any()) _context.TableColumns.RemoveRange(table.Columns);
                await _context.SaveChangesAsync();

                // ========== 4. Delete Tables ==========
                if (existingTemplate.CoverPage != null && existingTemplate.CoverPage.Tables.Any())
                    _context.DynamicTables.RemoveRange(existingTemplate.CoverPage.Tables);

                foreach (var master in existingTemplate.MasterSections)
                {
                    if (master.Tables.Any()) _context.DynamicTables.RemoveRange(master.Tables);
                    foreach (var sub in master.SubSections)
                        if (sub.Tables.Any()) _context.DynamicTables.RemoveRange(sub.Tables);
                }

                foreach (var section in existingTemplate.Sections)
                    if (section.Tables.Any()) _context.DynamicTables.RemoveRange(section.Tables);
                await _context.SaveChangesAsync();

                // ========== 5. Delete Paragraphs ==========
                foreach (var master in existingTemplate.MasterSections)
                {
                    if (master.Paragraphs.Any()) _context.MasterSectionParagraphs.RemoveRange(master.Paragraphs);
                    foreach (var sub in master.SubSections)
                        if (sub.Paragraphs.Any()) _context.SubSectionParagraphs.RemoveRange(sub.Paragraphs);
                }

                foreach (var section in existingTemplate.Sections)
                    if (section.Paragraphs.Any()) _context.SectionParagraphs.RemoveRange(section.Paragraphs);
                await _context.SaveChangesAsync();

                // ========== 6. Delete ImageGroups and Images ==========
                if (existingTemplate.CoverPage != null && existingTemplate.CoverPage.ImageGroups.Any())
                {
                    foreach (var group in existingTemplate.CoverPage.ImageGroups)
                    {
                        if (group.Images.Any())
                            _context.Images.RemoveRange(group.Images);
                    }
                    _context.ImageGroups.RemoveRange(existingTemplate.CoverPage.ImageGroups);
                }

                foreach (var master in existingTemplate.MasterSections)
                {
                    if (master.ImageGroups.Any())
                    {
                        foreach (var group in master.ImageGroups)
                        {
                            if (group.Images.Any())
                                _context.Images.RemoveRange(group.Images);
                        }
                        _context.ImageGroups.RemoveRange(master.ImageGroups);
                    }
                    foreach (var sub in master.SubSections)
                    {
                        if (sub.ImageGroups.Any())
                        {
                            foreach (var group in sub.ImageGroups)
                            {
                                if (group.Images.Any())
                                    _context.Images.RemoveRange(group.Images);
                            }
                            _context.ImageGroups.RemoveRange(sub.ImageGroups);
                        }
                    }
                }

                foreach (var section in existingTemplate.Sections)
                {
                    if (section.ImageGroups.Any())
                    {
                        foreach (var group in section.ImageGroups)
                        {
                            if (group.Images.Any())
                                _context.Images.RemoveRange(group.Images);
                        }
                        _context.ImageGroups.RemoveRange(section.ImageGroups);
                    }
                }
                await _context.SaveChangesAsync();

                // ========== 7. Delete SubSections ==========
                foreach (var master in existingTemplate.MasterSections)
                    if (master.SubSections.Any()) _context.SubSections.RemoveRange(master.SubSections);
                await _context.SaveChangesAsync();

                // ========== 8. Delete MasterSections ==========
                if (existingTemplate.MasterSections.Any())
                    _context.MasterSections.RemoveRange(existingTemplate.MasterSections);
                await _context.SaveChangesAsync();

                // ========== 9. Delete Sections ==========
                if (existingTemplate.Sections.Any())
                    _context.TemplateSections.RemoveRange(existingTemplate.Sections);
                await _context.SaveChangesAsync();

                // ========== 10. Delete CoverPage Items & CoverPage ==========
                if (existingTemplate.CoverPage != null)
                {
                    if (existingTemplate.CoverPage.Items.Any())
                        _context.CoverPageItems.RemoveRange(existingTemplate.CoverPage.Items);
                    _context.CoverPageTemplates.Remove(existingTemplate.CoverPage);
                }
                await _context.SaveChangesAsync();

                // ========== 11. Delete Template ==========
                _context.DocumentTemplates.Remove(existingTemplate);
                await _context.SaveChangesAsync();

                // ========== 12. Create New Template ==========
                var newTemplate = new DocumentTemplate
                {
                    Name = dto.Name,
                    CoverPage = dto.CoverPage == null ? null : new CoverPageTemplate
                    {
                        Title = dto.CoverPage.Title,
                        Items = dto.CoverPage.Items.OrderBy(x => x.Order).Select(x => new CoverPageItem
                        {
                            Label = x.Label,
                            Value = x.Value,
                            Order = x.Order
                        }).ToList(),
                        ImageGroups = dto.CoverPage.ImageGroups?.OrderBy(g => g.Order).Select(g => new ImageGroup
                        {
                            Title = g.Title,
                            Order = g.Order,
                            ImagesPerRow = g.ImagesPerRow > 0 ? g.ImagesPerRow : 2,
                            Images = g.Images.Select(img => new ImageItem
                            {
                                FileName = img.FileName,
                                Caption = img.Caption,
                                Order = img.Order,
                                Width = img.Width,
                                Height = img.Height,
                                ImageData = string.IsNullOrEmpty(img.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(img.ImageBase64)
                            }).ToList()
                        }).ToList() ?? new List<ImageGroup>(),
                        Tables = dto.CoverPage.Tables.Select(x => new DynamicTable
                        {
                            Title = x.Title,
                            Order = x.Order,
                            ShowRowNumbers = x.ShowRowNumbers,
                            RowNumberHeader = x.RowNumberHeader,
                            Columns = x.Columns.OrderBy(c => c.Order).Select(c => new TableColumnDefinition
                            {
                                Header = c.Header,
                                Width = c.Width,
                                Order = c.Order
                            }).ToList()
                        }).ToList()
                    },
                    MasterSections = dto.MasterSections.OrderBy(x => x.Order).Select(x => new MasterSection
                    {
                        Title = x.Title,
                        Order = x.Order,
                        ShowInToc = x.ShowInToc,
                        Paragraphs = x.Paragraphs.OrderBy(p => p.Order).Select(p => new MasterSectionParagraph
                        {
                            Text = p.Text,
                            Order = p.Order
                        }).ToList(),
                        ImageGroups = x.ImageGroups?.OrderBy(g => g.Order).Select(g => new ImageGroup
                        {
                            Title = g.Title,
                            Order = g.Order,
                            ImagesPerRow = g.ImagesPerRow > 0 ? g.ImagesPerRow : 2,
                            Images = g.Images.Select(img => new ImageItem
                            {
                                FileName = img.FileName,
                                Caption = img.Caption,
                                Order = img.Order,
                                Width = img.Width,
                                Height = img.Height,
                                ImageData = string.IsNullOrEmpty(img.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(img.ImageBase64)
                            }).ToList()
                        }).ToList() ?? new List<ImageGroup>(),
                        SubSections = x.SubSections.OrderBy(s => s.Order).Select(s => new SubSection
                        {
                            Title = s.Title,
                            Order = s.Order,
                            ShowInToc = s.ShowInToc,
                            Paragraphs = s.Paragraphs.OrderBy(p => p.Order).Select(p => new SubSectionParagraph
                            {
                                Text = p.Text,
                                Order = p.Order
                            }).ToList(),
                            ImageGroups = s.ImageGroups?.OrderBy(g => g.Order).Select(g => new ImageGroup
                            {
                                Title = g.Title,
                                Order = g.Order,
                                ImagesPerRow = g.ImagesPerRow > 0 ? g.ImagesPerRow : 2,
                                Images = g.Images.Select(img => new ImageItem
                                {
                                    FileName = img.FileName,
                                    Caption = img.Caption,
                                    Order = img.Order,
                                    Width = img.Width,
                                    Height = img.Height,
                                    ImageData = string.IsNullOrEmpty(img.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(img.ImageBase64)
                                }).ToList()
                            }).ToList() ?? new List<ImageGroup>(),
                            Tables = s.Tables.Select(t => new DynamicTable
                            {
                                Title = t.Title,
                                Order = t.Order,
                                ShowRowNumbers = t.ShowRowNumbers,
                                RowNumberHeader = t.RowNumberHeader,
                                Columns = t.Columns.OrderBy(c => c.Order).Select(c => new TableColumnDefinition
                                {
                                    Header = c.Header,
                                    Width = c.Width,
                                    Order = c.Order
                                }).ToList()
                            }).ToList()
                        }).ToList(),
                        Tables = x.Tables.Select(t => new DynamicTable
                        {
                            Title = t.Title,
                            Order = t.Order,
                            ShowRowNumbers = t.ShowRowNumbers,
                            RowNumberHeader = t.RowNumberHeader,
                            Columns = t.Columns.OrderBy(c => c.Order).Select(c => new TableColumnDefinition
                            {
                                Header = c.Header,
                                Width = c.Width,
                                Order = c.Order
                            }).ToList()
                        }).ToList()
                    }).ToList(),
                    Sections = dto.Sections.OrderBy(x => x.Order).Select(x => new TemplateSection
                    {
                        Title = x.Title,
                        Order = x.Order,
                        Paragraphs = x.Paragraphs.OrderBy(p => p.Order).Select(p => new SectionParagraph
                        {
                            Text = p.Text,
                            Order = p.Order
                        }).ToList(),
                        ImageGroups = x.ImageGroups?.OrderBy(g => g.Order).Select(g => new ImageGroup
                        {
                            Title = g.Title,
                            Order = g.Order,
                            ImagesPerRow = g.ImagesPerRow > 0 ? g.ImagesPerRow : 2,
                            Images = g.Images.Select(img => new ImageItem
                            {
                                FileName = img.FileName,
                                Caption = img.Caption,
                                Order = img.Order,
                                Width = img.Width,
                                Height = img.Height,
                                ImageData = string.IsNullOrEmpty(img.ImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(img.ImageBase64)
                            }).ToList()
                        }).ToList() ?? new List<ImageGroup>(),
                        Tables = x.Tables.Select(t => new DynamicTable
                        {
                            Title = t.Title,
                            Order = t.Order,
                            ShowRowNumbers = t.ShowRowNumbers,
                            RowNumberHeader = t.RowNumberHeader,
                            Columns = t.Columns.OrderBy(c => c.Order).Select(c => new TableColumnDefinition
                            {
                                Header = c.Header,
                                Width = c.Width,
                                Order = c.Order
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };

                _context.DocumentTemplates.Add(newTemplate);
                await _context.SaveChangesAsync();

                await AddAllTableRowsAndCells(dto, newTemplate);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { Id = newTemplate.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}