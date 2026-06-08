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
                        // Rows را فعلاً اضافه نکن
                    }).ToList()
                },
                Sections = dto.Sections.OrderBy(x => x.Order).Select(x => new TemplateSection
                {
                    Title = x.Title,
                    Order = x.Order,
                    Paragraphs = x.Paragraphs.OrderBy(p => p.Order).Select(p => new SectionParagraph
                    {
                        Text = p.Text,
                        Order = p.Order
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
                }).ToList()
            };

            _context.DocumentTemplates.Add(template);

            // اول ذخیره کن تا Idها ساخته شود
            await _context.SaveChangesAsync();

            // حالا ردیف‌ها و سلول‌ها را با Idهای واقعی اضافه کن
            await AddTableRowsAndCells(dto, template);

            await _context.SaveChangesAsync();

            return Ok(new { Id = template.Id });
        }

        private async Task AddTableRowsAndCells(CreateDocumentTemplateDto dto, DocumentTemplate template)
        {
            // اضافه کردن ردیف‌های کاورپیج
            if (dto.CoverPage != null && template.CoverPage != null)
            {
                for (int i = 0; i < dto.CoverPage.Tables.Count; i++)
                {
                    var tableDto = dto.CoverPage.Tables[i];
                    var table = template.CoverPage.Tables.ElementAt(i);

                    await AddRowsToTable(tableDto, table);
                }
            }

            // اضافه کردن ردیف‌های سکشن‌ها
            for (int s = 0; s < dto.Sections.Count; s++)
            {
                var sectionDto = dto.Sections[s];
                var section = template.Sections.ElementAt(s);

                for (int t = 0; t < sectionDto.Tables.Count; t++)
                {
                    var tableDto = sectionDto.Tables[t];
                    var table = section.Tables.ElementAt(t);

                    await AddRowsToTable(tableDto, table);
                }
            }
        }

        private async Task AddRowsToTable(CreateDynamicTableDto tableDto, DynamicTable table)
        {
            var columns = table.Columns.OrderBy(c => c.Order).ToList();

            foreach (var rowDto in tableDto.Rows.OrderBy(x => x.RowNumber))
            {
                var row = new TableDataRow
                {
                    RowNumber = rowDto.RowNumber,
                    DynamicTableId = table.Id,
                    Cells = new List<TableDataCell>()
                };

                for (int i = 0; i < columns.Count && i < rowDto.Values.Count; i++)
                {
                    row.Cells.Add(new TableDataCell
                    {
                        TableColumnDefinitionId = columns[i].Id,
                        Value = rowDto.Values[i]
                    });
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
                .FirstOrDefaultAsync(x => x.Id == id);

            if (template == null)
                return NotFound();

            return Ok(new
            {
                template.Id,
                template.Name,

                CoverPage = template.CoverPage == null
                    ? null
                    : new
                    {
                        template.CoverPage.Id,
                        template.CoverPage.Title,

                        Items = template.CoverPage.Items
                            .OrderBy(x => x.Order)
                            .Select(x => new
                            {
                                x.Id,
                                x.Label,
                                x.Value,
                                x.Order
                            }),

                        Tables = template.CoverPage.Tables
                            .OrderBy(x => x.Order)
                            .Select(x => MapDynamicTable(x))
                    },

                Sections = template.Sections
                    .OrderBy(x => x.Order)
                    .Select(x => new
                    {
                        x.Id,
                        x.Title,
                        x.Order,

                        Paragraphs = x.Paragraphs
                            .OrderBy(p => p.Order)
                            .Select(p => new
                            {
                                p.Id,
                                p.Text,
                                p.Order
                            }),

                        Tables = x.Tables
                            .OrderBy(t => t.Order)
                            .Select(t => MapDynamicTable(t))
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
        // Helper Methods
        // =========================
       
        private object MapDynamicTable(DynamicTable table)
        {
            return new
            {
                table.Id,
                table.Title,
                table.Order,
                table.ShowRowNumbers,
                table.RowNumberHeader,

                Columns = table.Columns
                    .OrderBy(x => x.Order)
                    .Select(x => new
                    {
                        x.Id,
                        x.Header,
                        x.Width,
                        x.Order,
                        x.IsRowNumberColumn
                    }),

                Rows = table.Rows
                    .OrderBy(x => x.RowNumber)
                    .Select(x => new
                    {
                        x.Id,
                        x.RowNumber,
                        Cells = x.Cells
                            .OrderBy(c => c.Column.Order)
                            .Select(c => new
                            {
                                c.Id,
                                ColumnId = c.TableColumnDefinitionId,
                                c.Value
                            })
                    })
            };
        }
    }
}