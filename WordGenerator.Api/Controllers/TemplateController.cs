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

        // TemplateController.cs - اصلاح متد Update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, CreateDocumentTemplateDto dto)
        {
            // پیدا کردن تمپلیت موجود
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

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ========== بروزرسانی نام ==========
                template.Name = dto.Name;

                // ========== مدیریت کاورپیج ==========
                if (dto.CoverPage != null)
                {
                    if (template.CoverPage == null)
                    {
                        template.CoverPage = new CoverPageTemplate { DocumentTemplateId = template.Id };
                    }

                    template.CoverPage.Title = dto.CoverPage.Title;

                    // حذف آیتم‌های قدیمی کاورپیج (بدون مشکل FK)
                    if (template.CoverPage.Items.Any())
                        _context.CoverPageItems.RemoveRange(template.CoverPage.Items);

                    // اضافه کردن آیتم‌های جدید
                    template.CoverPage.Items = dto.CoverPage.Items.OrderBy(x => x.Order).Select(x => new CoverPageItem
                    {
                        Label = x.Label,
                        Value = x.Value,
                        Order = x.Order,
                        CoverPageTemplateId = template.CoverPage.Id
                    }).ToList();

                    // مدیریت جداول کاورپیج
                    if (template.CoverPage.Tables.Any())
                    {
                        // حذف کامل جداول قدیمی (به صورت دستی و مرحله‌ای)
                        foreach (var oldTable in template.CoverPage.Tables.ToList())
                        {
                            await DeleteTableWithDependencies(oldTable);
                        }
                        template.CoverPage.Tables.Clear();
                    }

                    // اضافه کردن جداول جدید
                    foreach (var tableDto in dto.CoverPage.Tables)
                    {
                        var newTable = await CreateTableWithDependencies(tableDto, coverPageId: template.CoverPage.Id, sectionId: null);
                        template.CoverPage.Tables.Add(newTable);
                    }
                }
                else
                {
                    if (template.CoverPage != null)
                    {
                        // حذف کاورپیج و همه وابستگی‌ها
                        await DeleteCoverPageWithDependencies(template.CoverPage);
                        template.CoverPage = null;
                    }
                }

                // ========== مدیریت بخش‌ها ==========
                if (template.Sections.Any())
                {
                    // حذف کامل بخش‌های قدیمی و همه وابستگی‌ها
                    foreach (var oldSection in template.Sections.ToList())
                    {
                        await DeleteSectionWithDependencies(oldSection);
                    }
                    template.Sections.Clear();
                }

                // اضافه کردن بخش‌های جدید
                foreach (var sectionDto in dto.Sections.OrderBy(x => x.Order))
                {
                    var newSection = new TemplateSection
                    {
                        Title = sectionDto.Title,
                        Order = sectionDto.Order,
                        TemplateId = template.Id,
                        Paragraphs = new List<SectionParagraph>(),
                        Tables = new List<DynamicTable>()
                    };

                    // اضافه کردن پاراگراف‌ها
                    foreach (var paraDto in sectionDto.Paragraphs.OrderBy(x => x.Order))
                    {
                        newSection.Paragraphs.Add(new SectionParagraph
                        {
                            Text = paraDto.Text,
                            Order = paraDto.Order,
                            TemplateSectionId = newSection.Id
                        });
                    }

                    _context.TemplateSections.Add(newSection);
                    await _context.SaveChangesAsync(); // ذخیره برای گرفتن Id بخش

                    // اضافه کردن جداول بخش
                    foreach (var tableDto in sectionDto.Tables)
                    {
                        var newTable = await CreateTableWithDependencies(tableDto, coverPageId: null, sectionId: newSection.Id);
                        newSection.Tables.Add(newTable);
                    }

                    template.Sections.Add(newSection);
                }

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

        // متد کمکی برای حذف جدول با همه وابستگی‌ها
        private async Task DeleteTableWithDependencies(DynamicTable table)
        {
            // حذف سلول‌ها
            foreach (var row in table.Rows.ToList())
            {
                if (row.Cells.Any())
                    _context.TableCells.RemoveRange(row.Cells);
            }
            await _context.SaveChangesAsync();

            // حذف ردیف‌ها
            if (table.Rows.Any())
                _context.TableRows.RemoveRange(table.Rows);
            await _context.SaveChangesAsync();

            // حذف ستون‌ها
            if (table.Columns.Any())
                _context.TableColumns.RemoveRange(table.Columns);
            await _context.SaveChangesAsync();

            // حذف خود جدول
            _context.DynamicTables.Remove(table);
            await _context.SaveChangesAsync();
        }

        // متد کمکی برای حذف بخش با همه وابستگی‌ها
        private async Task DeleteSectionWithDependencies(TemplateSection section)
        {
            // حذف جداول بخش
            foreach (var table in section.Tables.ToList())
            {
                await DeleteTableWithDependencies(table);
            }

            // حذف پاراگراف‌ها
            if (section.Paragraphs.Any())
                _context.SectionParagraphs.RemoveRange(section.Paragraphs);

            // حذف خود بخش
            _context.TemplateSections.Remove(section);
            await _context.SaveChangesAsync();
        }

        // متد کمکی برای حذف کاورپیج با همه وابستگی‌ها
        private async Task DeleteCoverPageWithDependencies(CoverPageTemplate coverPage)
        {
            // حذف جداول کاورپیج
            foreach (var table in coverPage.Tables.ToList())
            {
                await DeleteTableWithDependencies(table);
            }

            // حذف آیتم‌ها
            if (coverPage.Items.Any())
                _context.CoverPageItems.RemoveRange(coverPage.Items);

            // حذف خود کاورپیج
            _context.CoverPageTemplates.Remove(coverPage);
            await _context.SaveChangesAsync();
        }

        // متد کمکی برای ایجاد جدول با همه وابستگی‌ها
        private async Task<DynamicTable> CreateTableWithDependencies(CreateDynamicTableDto tableDto, long? coverPageId, long? sectionId)
        {
            // مرحله 1: ایجاد جدول
            var table = new DynamicTable
            {
                Title = tableDto.Title,
                Order = tableDto.Order,
                ShowRowNumbers = tableDto.ShowRowNumbers,
                RowNumberHeader = tableDto.RowNumberHeader,
                CoverPageTemplateId = coverPageId,
                TemplateSectionId = sectionId,
                Columns = new List<TableColumnDefinition>(),
                Rows = new List<TableDataRow>()
            };

            _context.DynamicTables.Add(table);
            await _context.SaveChangesAsync(); // گرفتن Id جدول

            // مرحله 2: ایجاد ستون‌ها
            var columns = new List<TableColumnDefinition>();
            foreach (var colDto in tableDto.Columns.OrderBy(x => x.Order))
            {
                var column = new TableColumnDefinition
                {
                    Header = colDto.Header,
                    Width = colDto.Width,
                    Order = colDto.Order,
                    DynamicTableId = table.Id
                };
                _context.TableColumns.Add(column);
                columns.Add(column);
            }
            await _context.SaveChangesAsync(); // گرفتن Id ستون‌ها

            // مرحله 3: ایجاد ردیف‌ها و سلول‌ها
            foreach (var rowDto in tableDto.Rows.OrderBy(x => x.RowNumber))
            {
                var row = new TableDataRow
                {
                    RowNumber = rowDto.RowNumber,
                    DynamicTableId = table.Id,
                    Cells = new List<TableDataCell>()
                };

                _context.TableRows.Add(row);
                await _context.SaveChangesAsync(); // گرفتن Id ردیف

                // ایجاد سلول‌ها
                for (int i = 0; i < columns.Count && i < rowDto.Values.Count; i++)
                {
                    var cell = new TableDataCell
                    {
                        TableDataRowId = row.Id,
                        TableColumnDefinitionId = columns[i].Id,
                        Value = rowDto.Values[i]
                    };
                    _context.TableCells.Add(cell);
                }
            }

            await _context.SaveChangesAsync();

            return table;
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