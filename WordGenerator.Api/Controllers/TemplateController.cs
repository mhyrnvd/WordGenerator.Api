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
                    Sections = new List<TemplateSection>(),
                    // ===== بخش‌های انتهای سند =====
                    AttachmentSection = dto.AttachmentSection != null ? new AttachmentSection
                    {
                        Title = dto.AttachmentSection.Title ?? "الف) پیوست‌ها",
                        IsActive = dto.AttachmentSection.IsActive,
                        Order = dto.AttachmentSection.Order,
                        Elements = new List<ContentElement>()
                    } : null,
                    ReferenceSection = dto.ReferenceSection != null ? new ReferenceSection
                    {
                        Title = dto.ReferenceSection.Title ?? "ب) References",
                        IsActive = dto.ReferenceSection.IsActive,
                        Order = dto.ReferenceSection.Order,
                        Elements = new List<ContentElement>()
                    } : null,
                    DocumentSection = dto.DocumentSection != null ? new DocumentSection
                    {
                        Title = dto.DocumentSection.Title ?? "پ) مدارک",
                        IsActive = dto.DocumentSection.IsActive,
                        Order = dto.DocumentSection.Order,
                        Elements = new List<ContentElement>()
                    } : null,
                    // ===== پیش‌گفتار و مفاهیم =====
                    PrefaceSection = dto.PrefaceSection != null ? new PrefaceSection
                    {
                        Title = dto.PrefaceSection.Title ?? "پیش‌گفتار",
                        IsActive = dto.PrefaceSection.IsActive,
                        Elements = new List<ContentElement>()
                    } : null,
                    ConceptsSection = dto.ConceptsSection != null ? new ConceptsSection
                    {
                        Title = dto.ConceptsSection.Title ?? "مفاهیم",
                        IsActive = dto.ConceptsSection.IsActive,
                        Elements = new List<ContentElement>()
                    } : null
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
                        template.CoverPage.Id,
                        null,
                        null,
                        null,
                        null,
                        null
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
                        null,
                        null,
                        null,
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
                            null,
                            null,
                            null,
                            null,
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
                        null,
                        null,
                        null,
                        null,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== Attachment Section Elements ==========
                if (template.AttachmentSection != null && dto.AttachmentSection != null)
                {
                    template.AttachmentSection.Elements = await MapContentElements(
                        dto.AttachmentSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        template.AttachmentSection.Id,
                        null,
                        null,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== Reference Section Elements ==========
                if (template.ReferenceSection != null && dto.ReferenceSection != null)
                {
                    template.ReferenceSection.Elements = await MapContentElements(
                        dto.ReferenceSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        null,
                        template.ReferenceSection.Id,
                        null,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== Document Section Elements ==========
                if (template.DocumentSection != null && dto.DocumentSection != null)
                {
                    template.DocumentSection.Elements = await MapContentElements(
                        dto.DocumentSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        template.DocumentSection.Id,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== Preface Section Elements ==========
                if (template.PrefaceSection != null && dto.PrefaceSection != null)
                {
                    template.PrefaceSection.Elements = await MapContentElements(
                        dto.PrefaceSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        template.PrefaceSection.Id,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== Concepts Section Elements ==========
                if (template.ConceptsSection != null && dto.ConceptsSection != null)
                {
                    template.ConceptsSection.Elements = await MapContentElements(
                        dto.ConceptsSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        template.ConceptsSection.Id
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
                    HasAttachmentSection = x.AttachmentSection != null,
                    HasReferenceSection = x.ReferenceSection != null,
                    HasDocumentSection = x.DocumentSection != null,
                    HasPrefaceSection = x.PrefaceSection != null,
                    HasConceptsSection = x.ConceptsSection != null,
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
                // ===== بخش‌های انتهای سند =====
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
                // ===== پیش‌گفتار و مفاهیم =====
                .Include(x => x.PrefaceSection)
                    .ThenInclude(p => p.Elements)
                        .ThenInclude(e => e.Image)
                .Include(x => x.PrefaceSection)
                    .ThenInclude(p => p.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.PrefaceSection)
                    .ThenInclude(p => p.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                                    .ThenInclude(c => c.Column)
                .Include(x => x.PrefaceSection)
                    .ThenInclude(p => p.Elements)
                        .ThenInclude(e => e.BulletList)
                            .ThenInclude(b => b.Items)
                .Include(x => x.ConceptsSection)
                    .ThenInclude(c => c.Elements)
                        .ThenInclude(e => e.Image)
                .Include(x => x.ConceptsSection)
                    .ThenInclude(c => c.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Columns)
                .Include(x => x.ConceptsSection)
                    .ThenInclude(c => c.Elements)
                        .ThenInclude(e => e.Table)
                            .ThenInclude(t => t.Rows)
                                .ThenInclude(r => r.Cells)
                                    .ThenInclude(c => c.Column)
                .Include(x => x.ConceptsSection)
                    .ThenInclude(c => c.Elements)
                        .ThenInclude(e => e.BulletList)
                            .ThenInclude(b => b.Items)
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
                }),

                // ===== بخش‌های انتهای سند =====
                AttachmentSection = template.AttachmentSection == null ? null : new
                {
                    template.AttachmentSection.Id,
                    template.AttachmentSection.Title,
                    template.AttachmentSection.IsActive,
                    template.AttachmentSection.Order,
                    Elements = template.AttachmentSection.Elements.OrderBy(e => e.Order).Select(e => MapContentElementToDto(e))
                },

                ReferenceSection = template.ReferenceSection == null ? null : new
                {
                    template.ReferenceSection.Id,
                    template.ReferenceSection.Title,
                    template.ReferenceSection.IsActive,
                    template.ReferenceSection.Order,
                    Elements = template.ReferenceSection.Elements.OrderBy(e => e.Order).Select(e => MapContentElementToDto(e))
                },

                DocumentSection = template.DocumentSection == null ? null : new
                {
                    template.DocumentSection.Id,
                    template.DocumentSection.Title,
                    template.DocumentSection.IsActive,
                    template.DocumentSection.Order,
                    Elements = template.DocumentSection.Elements.OrderBy(e => e.Order).Select(e => MapContentElementToDto(e))
                },

                // ===== پیش‌گفتار و مفاهیم =====
                PrefaceSection = template.PrefaceSection == null ? null : new
                {
                    template.PrefaceSection.Id,
                    template.PrefaceSection.Title,
                    template.PrefaceSection.IsActive,
                    Elements = template.PrefaceSection.Elements.OrderBy(e => e.Order).Select(e => MapContentElementToDto(e))
                },

                ConceptsSection = template.ConceptsSection == null ? null : new
                {
                    template.ConceptsSection.Id,
                    template.ConceptsSection.Title,
                    template.ConceptsSection.IsActive,
                    Elements = template.ConceptsSection.Elements.OrderBy(e => e.Order).Select(e => MapContentElementToDto(e))
                }
            });
        }

        // =========================
        // DELETE TEMPLATE
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var template = await _context.DocumentTemplates
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
                    .Include(x => x.Sections)
                        .ThenInclude(s => s.Elements)
                            .ThenInclude(e => e.BulletList)
                                .ThenInclude(b => b.Items)
                    // ===== بخش‌های انتهای سند =====
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
                    .Include(x => x.DocumentSection)
                        .ThenInclude(d => d.Elements)
                            .ThenInclude(e => e.BulletList)
                                .ThenInclude(b => b.Items)
                    // ===== پیش‌گفتار و مفاهیم =====
                    .Include(x => x.PrefaceSection)
                        .ThenInclude(p => p.Elements)
                            .ThenInclude(e => e.Image)
                    .Include(x => x.PrefaceSection)
                        .ThenInclude(p => p.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Columns)
                    .Include(x => x.PrefaceSection)
                        .ThenInclude(p => p.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Rows)
                                    .ThenInclude(r => r.Cells)
                    .Include(x => x.PrefaceSection)
                        .ThenInclude(p => p.Elements)
                            .ThenInclude(e => e.BulletList)
                                .ThenInclude(b => b.Items)
                    .Include(x => x.ConceptsSection)
                        .ThenInclude(c => c.Elements)
                            .ThenInclude(e => e.Image)
                    .Include(x => x.ConceptsSection)
                        .ThenInclude(c => c.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Columns)
                    .Include(x => x.ConceptsSection)
                        .ThenInclude(c => c.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Rows)
                                    .ThenInclude(r => r.Cells)
                    .Include(x => x.ConceptsSection)
                        .ThenInclude(c => c.Elements)
                            .ThenInclude(e => e.BulletList)
                                .ThenInclude(b => b.Items)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (template == null)
                    return NotFound();

                // ========== حذف به ترتیب با رعایت روابط Restrict ==========

                // 1. حذف سلول‌های جداول
                await DeleteAllCells(template);
                await _context.SaveChangesAsync();

                // 2. حذف ردیف‌های جداول
                await DeleteAllRows(template);
                await _context.SaveChangesAsync();

                // 3. حذف ستون‌های جداول
                await DeleteAllColumns(template);
                await _context.SaveChangesAsync();

                // 4. حذف جداول
                await DeleteAllTables(template);
                await _context.SaveChangesAsync();

                // 5. حذف Bullet List Items
                await DeleteAllBulletListItems(template);
                await _context.SaveChangesAsync();

                // 6. حذف Bullet Lists
                await DeleteAllBulletLists(template);
                await _context.SaveChangesAsync();

                // 7. حذف تصاویر
                await DeleteAllImages(template);
                await _context.SaveChangesAsync();

                // 8. حذف تمام Elementها
                await DeleteAllElements(template);
                await _context.SaveChangesAsync();

                // 9. حذف آیتم‌های کاورپیج
                if (template.CoverPage != null && template.CoverPage.Items.Any())
                {
                    _context.CoverPageItems.RemoveRange(template.CoverPage.Items);
                    await _context.SaveChangesAsync();
                }

                // 10. حذف کاورپیج
                if (template.CoverPage != null)
                {
                    _context.CoverPageTemplates.Remove(template.CoverPage);
                    await _context.SaveChangesAsync();
                }

                // 11. حذف Attachment Section
                if (template.AttachmentSection != null)
                {
                    if (template.AttachmentSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(template.AttachmentSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.AttachmentSections.Remove(template.AttachmentSection);
                    await _context.SaveChangesAsync();
                }

                // 12. حذف Reference Section
                if (template.ReferenceSection != null)
                {
                    if (template.ReferenceSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(template.ReferenceSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.ReferenceSections.Remove(template.ReferenceSection);
                    await _context.SaveChangesAsync();
                }

                // 13. حذف Document Section
                if (template.DocumentSection != null)
                {
                    if (template.DocumentSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(template.DocumentSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.DocumentSections.Remove(template.DocumentSection);
                    await _context.SaveChangesAsync();
                }

                // 14. حذف Preface Section
                if (template.PrefaceSection != null)
                {
                    if (template.PrefaceSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(template.PrefaceSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.PrefaceSections.Remove(template.PrefaceSection);
                    await _context.SaveChangesAsync();
                }

                // 15. حذف Concepts Section
                if (template.ConceptsSection != null)
                {
                    if (template.ConceptsSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(template.ConceptsSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.ConceptsSections.Remove(template.ConceptsSection);
                    await _context.SaveChangesAsync();
                }

                // 16. حذف زیربخش‌ها
                foreach (var master in template.MasterSections)
                {
                    if (master.SubSections.Any())
                    {
                        _context.SubSections.RemoveRange(master.SubSections);
                        await _context.SaveChangesAsync();
                    }
                }

                // 17. حذف بخش‌های اصلی
                if (template.MasterSections.Any())
                {
                    _context.MasterSections.RemoveRange(template.MasterSections);
                    await _context.SaveChangesAsync();
                }

                // 18. حذف بخش‌های قدیمی
                if (template.Sections.Any())
                {
                    _context.TemplateSections.RemoveRange(template.Sections);
                    await _context.SaveChangesAsync();
                }

                // 19. حذف لوگوهای هدر
                if (template.PageHeader != null && template.PageHeader.Logos.Any())
                {
                    _context.HeaderLogos.RemoveRange(template.PageHeader.Logos);
                    await _context.SaveChangesAsync();
                }

                // 20. حذف PageHeader
                if (template.PageHeader != null)
                {
                    _context.PageHeaders.Remove(template.PageHeader);
                    await _context.SaveChangesAsync();
                }

                // 21. حذف خود تمپلیت
                _context.DocumentTemplates.Remove(template);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { Message = "Template deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
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
                    .Include(x => x.Sections)
                        .ThenInclude(s => s.Elements)
                            .ThenInclude(e => e.BulletList)
                                .ThenInclude(b => b.Items)
                    // ===== بخش‌های انتهای سند =====
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
                    .Include(x => x.DocumentSection)
                        .ThenInclude(d => d.Elements)
                            .ThenInclude(e => e.BulletList)
                                .ThenInclude(b => b.Items)
                    // ===== پیش‌گفتار و مفاهیم =====
                    .Include(x => x.PrefaceSection)
                        .ThenInclude(p => p.Elements)
                            .ThenInclude(e => e.Image)
                    .Include(x => x.PrefaceSection)
                        .ThenInclude(p => p.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Columns)
                    .Include(x => x.PrefaceSection)
                        .ThenInclude(p => p.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Rows)
                                    .ThenInclude(r => r.Cells)
                    .Include(x => x.PrefaceSection)
                        .ThenInclude(p => p.Elements)
                            .ThenInclude(e => e.BulletList)
                                .ThenInclude(b => b.Items)
                    .Include(x => x.ConceptsSection)
                        .ThenInclude(c => c.Elements)
                            .ThenInclude(e => e.Image)
                    .Include(x => x.ConceptsSection)
                        .ThenInclude(c => c.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Columns)
                    .Include(x => x.ConceptsSection)
                        .ThenInclude(c => c.Elements)
                            .ThenInclude(e => e.Table)
                                .ThenInclude(t => t.Rows)
                                    .ThenInclude(r => r.Cells)
                    .Include(x => x.ConceptsSection)
                        .ThenInclude(c => c.Elements)
                            .ThenInclude(e => e.BulletList)
                                .ThenInclude(b => b.Items)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existingTemplate == null)
                    return NotFound();

                // ========== حذف به ترتیب با رعایت روابط Restrict ==========

                // 1. حذف سلول‌های جداول
                await DeleteAllCells(existingTemplate);
                await _context.SaveChangesAsync();

                // 2. حذف ردیف‌های جداول
                await DeleteAllRows(existingTemplate);
                await _context.SaveChangesAsync();

                // 3. حذف ستون‌های جداول
                await DeleteAllColumns(existingTemplate);
                await _context.SaveChangesAsync();

                // 4. حذف جداول
                await DeleteAllTables(existingTemplate);
                await _context.SaveChangesAsync();

                // 5. حذف Bullet List Items
                await DeleteAllBulletListItems(existingTemplate);
                await _context.SaveChangesAsync();

                // 6. حذف Bullet Lists
                await DeleteAllBulletLists(existingTemplate);
                await _context.SaveChangesAsync();

                // 7. حذف تصاویر
                await DeleteAllImages(existingTemplate);
                await _context.SaveChangesAsync();

                // 8. حذف تمام Elementها
                await DeleteAllElements(existingTemplate);
                await _context.SaveChangesAsync();

                // 9. حذف آیتم‌های کاورپیج
                if (existingTemplate.CoverPage != null && existingTemplate.CoverPage.Items.Any())
                {
                    _context.CoverPageItems.RemoveRange(existingTemplate.CoverPage.Items);
                    await _context.SaveChangesAsync();
                }

                // 10. حذف کاورپیج
                if (existingTemplate.CoverPage != null)
                {
                    _context.CoverPageTemplates.Remove(existingTemplate.CoverPage);
                    await _context.SaveChangesAsync();
                }

                // 11. حذف Attachment Section
                if (existingTemplate.AttachmentSection != null)
                {
                    if (existingTemplate.AttachmentSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(existingTemplate.AttachmentSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.AttachmentSections.Remove(existingTemplate.AttachmentSection);
                    await _context.SaveChangesAsync();
                }

                // 12. حذف Reference Section
                if (existingTemplate.ReferenceSection != null)
                {
                    if (existingTemplate.ReferenceSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(existingTemplate.ReferenceSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.ReferenceSections.Remove(existingTemplate.ReferenceSection);
                    await _context.SaveChangesAsync();
                }

                // 13. حذف Document Section
                if (existingTemplate.DocumentSection != null)
                {
                    if (existingTemplate.DocumentSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(existingTemplate.DocumentSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.DocumentSections.Remove(existingTemplate.DocumentSection);
                    await _context.SaveChangesAsync();
                }

                // 14. حذف Preface Section
                if (existingTemplate.PrefaceSection != null)
                {
                    if (existingTemplate.PrefaceSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(existingTemplate.PrefaceSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.PrefaceSections.Remove(existingTemplate.PrefaceSection);
                    await _context.SaveChangesAsync();
                }

                // 15. حذف Concepts Section
                if (existingTemplate.ConceptsSection != null)
                {
                    if (existingTemplate.ConceptsSection.Elements.Any())
                    {
                        _context.ContentElements.RemoveRange(existingTemplate.ConceptsSection.Elements);
                        await _context.SaveChangesAsync();
                    }
                    _context.ConceptsSections.Remove(existingTemplate.ConceptsSection);
                    await _context.SaveChangesAsync();
                }

                // 16. حذف زیربخش‌ها
                foreach (var master in existingTemplate.MasterSections)
                {
                    if (master.SubSections.Any())
                    {
                        _context.SubSections.RemoveRange(master.SubSections);
                        await _context.SaveChangesAsync();
                    }
                }

                // 17. حذف بخش‌های اصلی
                if (existingTemplate.MasterSections.Any())
                {
                    _context.MasterSections.RemoveRange(existingTemplate.MasterSections);
                    await _context.SaveChangesAsync();
                }

                // 18. حذف بخش‌های قدیمی
                if (existingTemplate.Sections.Any())
                {
                    _context.TemplateSections.RemoveRange(existingTemplate.Sections);
                    await _context.SaveChangesAsync();
                }

                // 19. حذف لوگوهای هدر
                if (existingTemplate.PageHeader != null && existingTemplate.PageHeader.Logos.Any())
                {
                    _context.HeaderLogos.RemoveRange(existingTemplate.PageHeader.Logos);
                    await _context.SaveChangesAsync();
                }

                // 20. حذف PageHeader
                if (existingTemplate.PageHeader != null)
                {
                    _context.PageHeaders.Remove(existingTemplate.PageHeader);
                    await _context.SaveChangesAsync();
                }

                // 21. حذف خود تمپلیت
                _context.DocumentTemplates.Remove(existingTemplate);
                await _context.SaveChangesAsync();

                // ========== 22. ایجاد تمپلیت جدید ==========
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
                    Sections = new List<TemplateSection>(),
                    AttachmentSection = dto.AttachmentSection != null ? new AttachmentSection
                    {
                        Title = dto.AttachmentSection.Title ?? "الف) پیوست‌ها",
                        IsActive = dto.AttachmentSection.IsActive,
                        Order = dto.AttachmentSection.Order,
                        Elements = new List<ContentElement>()
                    } : null,
                    ReferenceSection = dto.ReferenceSection != null ? new ReferenceSection
                    {
                        Title = dto.ReferenceSection.Title ?? "ب) References",
                        IsActive = dto.ReferenceSection.IsActive,
                        Order = dto.ReferenceSection.Order,
                        Elements = new List<ContentElement>()
                    } : null,
                    DocumentSection = dto.DocumentSection != null ? new DocumentSection
                    {
                        Title = dto.DocumentSection.Title ?? "پ) مدارک",
                        IsActive = dto.DocumentSection.IsActive,
                        Order = dto.DocumentSection.Order,
                        Elements = new List<ContentElement>()
                    } : null,
                    // ===== جدید: پیش‌گفتار و مفاهیم =====
                    PrefaceSection = dto.PrefaceSection != null ? new PrefaceSection
                    {
                        Title = dto.PrefaceSection.Title ?? "پیش‌گفتار",
                        IsActive = dto.PrefaceSection.IsActive,
                        Elements = new List<ContentElement>()
                    } : null,
                    ConceptsSection = dto.ConceptsSection != null ? new ConceptsSection
                    {
                        Title = dto.ConceptsSection.Title ?? "مفاهیم",
                        IsActive = dto.ConceptsSection.IsActive,
                        Elements = new List<ContentElement>()
                    } : null
                };

                _context.DocumentTemplates.Add(newTemplate);
                await _context.SaveChangesAsync();

                // ========== 23. اضافه کردن Cover Page Elements ==========
                if (dto.CoverPage != null && newTemplate.CoverPage != null)
                {
                    newTemplate.CoverPage.Elements = await MapContentElements(
                        dto.CoverPage.Elements,
                        null,
                        null,
                        null,
                        newTemplate.CoverPage.Id,
                        null,
                        null,
                        null,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== 24. اضافه کردن Master Sections ==========
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
                        null,
                        null,
                        null,
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
                            null,
                            null,
                            null,
                            null,
                            null,
                            null
                        );
                        await _context.SaveChangesAsync();
                    }
                }

                // ========== 25. اضافه کردن Legacy Sections ==========
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
                        null,
                        null,
                        null,
                        null,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== 26. اضافه کردن Attachment Section Elements ==========
                if (newTemplate.AttachmentSection != null && dto.AttachmentSection != null)
                {
                    newTemplate.AttachmentSection.Elements = await MapContentElements(
                        dto.AttachmentSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        newTemplate.AttachmentSection.Id,
                        null,
                        null,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== 27. اضافه کردن Reference Section Elements ==========
                if (newTemplate.ReferenceSection != null && dto.ReferenceSection != null)
                {
                    newTemplate.ReferenceSection.Elements = await MapContentElements(
                        dto.ReferenceSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        null,
                        newTemplate.ReferenceSection.Id,
                        null,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== 28. اضافه کردن Document Section Elements ==========
                if (newTemplate.DocumentSection != null && dto.DocumentSection != null)
                {
                    newTemplate.DocumentSection.Elements = await MapContentElements(
                        dto.DocumentSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        newTemplate.DocumentSection.Id,
                        null,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== 29. اضافه کردن Preface Section Elements ==========
                if (newTemplate.PrefaceSection != null && dto.PrefaceSection != null)
                {
                    newTemplate.PrefaceSection.Elements = await MapContentElements(
                        dto.PrefaceSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        newTemplate.PrefaceSection.Id,
                        null
                    );
                    await _context.SaveChangesAsync();
                }

                // ========== 30. اضافه کردن Concepts Section Elements ==========
                if (newTemplate.ConceptsSection != null && dto.ConceptsSection != null)
                {
                    newTemplate.ConceptsSection.Elements = await MapContentElements(
                        dto.ConceptsSection.Elements,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        newTemplate.ConceptsSection.Id
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
            long? coverPageId,
            long? attachmentSectionId,
            long? referenceSectionId,
            long? documentSectionId,
            long? prefaceSectionId,
            long? conceptsSectionId)
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
                        "bulletlist" => ContentElementType.BulletList,
                        _ => ContentElementType.Paragraph
                    },
                    Order = elementDto.Order,
                    MasterSectionId = masterSectionId,
                    SubSectionId = subSectionId,
                    TemplateSectionId = templateSectionId,
                    CoverPageId = coverPageId,
                    AttachmentSectionId = attachmentSectionId,
                    ReferenceSectionId = referenceSectionId,
                    DocumentSectionId = documentSectionId,
                    PrefaceSectionId = prefaceSectionId,
                    ConceptsSectionId = conceptsSectionId
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

                        var columnIdMap = new Dictionary<long, long>();
                        for (int i = 0; i < columns.Count && i < elementDto.Table.Columns.Count; i++)
                        {
                            var frontendColumn = elementDto.Table.Columns.OrderBy(c => c.Order).ElementAt(i);
                            var dbColumn = columns[i];
                            if (frontendColumn.Id.HasValue)
                            {
                                columnIdMap[frontendColumn.Id.Value] = dbColumn.Id;
                            }
                        }

                        foreach (var rowDto in elementDto.Table.Rows.OrderBy(x => x.RowNumber))
                        {
                            var row = new TableDataRow
                            {
                                RowNumber = rowDto.RowNumber,
                                DynamicTableId = table.Id,
                                Cells = new List<TableDataCell>()
                            };

                            if (rowDto.Cells != null && rowDto.Cells.Any())
                            {
                                foreach (var cellDto in rowDto.Cells)
                                {
                                    if (columnIdMap.TryGetValue(cellDto.ColumnId, out long dbColumnId))
                                    {
                                        row.Cells.Add(new TableDataCell
                                        {
                                            TableColumnDefinitionId = dbColumnId,
                                            Value = cellDto.Value ?? ""
                                        });
                                    }
                                    else
                                    {
                                        var column = columns.FirstOrDefault(c => c.Id == cellDto.ColumnId);
                                        if (column != null)
                                        {
                                            row.Cells.Add(new TableDataCell
                                            {
                                                TableColumnDefinitionId = column.Id,
                                                Value = cellDto.Value ?? ""
                                            });
                                        }
                                    }
                                }
                            }
                            else if (rowDto.Values != null && rowDto.Values.Any())
                            {
                                for (int i = 0; i < columns.Count && i < rowDto.Values.Count; i++)
                                {
                                    row.Cells.Add(new TableDataCell
                                    {
                                        TableColumnDefinitionId = columns[i].Id,
                                        Value = rowDto.Values[i] ?? ""
                                    });
                                }
                            }

                            if (row.Cells.Any())
                            {
                                _context.TableRows.Add(row);
                            }
                        }

                        await _context.SaveChangesAsync();

                        element.Table = table;
                        break;

                    case ContentElementType.BulletList when elementDto.BulletList != null:
                        var bulletList = new BulletList
                        {
                            Title = elementDto.BulletList.Title,
                            Order = elementDto.BulletList.Order,
                            Items = elementDto.BulletList.Items.OrderBy(x => x.Order).Select(x => new BulletListItem
                            {
                                Text = x.Text,
                                Order = x.Order,
                                Level = x.Level
                            }).ToList()
                        };

                        _context.BulletLists.Add(bulletList);
                        await _context.SaveChangesAsync();

                        element.BulletList = bulletList;
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
                            }).ToList(),
                            values = r.Cells.OrderBy(c => c.Column.Order).Select(c => c.Value).ToList()
                        })
                    }
                },
                ContentElementType.BulletList when element.BulletList != null => new
                {
                    baseObj.Id,
                    baseObj.Type,
                    baseObj.Order,
                    bulletList = new
                    {
                        element.BulletList.Id,
                        element.BulletList.Title,
                        element.BulletList.Order,
                        items = element.BulletList.Items.OrderBy(x => x.Order).Select(x => new
                        {
                            x.Id,
                            x.Text,
                            x.Order,
                            x.Level
                        }).ToList()
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

            if (template.AttachmentSection != null)
            {
                foreach (var element in template.AttachmentSection.Elements.Where(e => e.Table != null))
                {
                    foreach (var row in element.Table.Rows)
                        allCells.AddRange(row.Cells);
                }
            }

            if (template.ReferenceSection != null)
            {
                foreach (var element in template.ReferenceSection.Elements.Where(e => e.Table != null))
                {
                    foreach (var row in element.Table.Rows)
                        allCells.AddRange(row.Cells);
                }
            }

            if (template.DocumentSection != null)
            {
                foreach (var element in template.DocumentSection.Elements.Where(e => e.Table != null))
                {
                    foreach (var row in element.Table.Rows)
                        allCells.AddRange(row.Cells);
                }
            }

            if (template.PrefaceSection != null)
            {
                foreach (var element in template.PrefaceSection.Elements.Where(e => e.Table != null))
                {
                    foreach (var row in element.Table.Rows)
                        allCells.AddRange(row.Cells);
                }
            }

            if (template.ConceptsSection != null)
            {
                foreach (var element in template.ConceptsSection.Elements.Where(e => e.Table != null))
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

            if (template.AttachmentSection != null)
            {
                foreach (var element in template.AttachmentSection.Elements.Where(e => e.Table != null))
                    allRows.AddRange(element.Table.Rows);
            }

            if (template.ReferenceSection != null)
            {
                foreach (var element in template.ReferenceSection.Elements.Where(e => e.Table != null))
                    allRows.AddRange(element.Table.Rows);
            }

            if (template.DocumentSection != null)
            {
                foreach (var element in template.DocumentSection.Elements.Where(e => e.Table != null))
                    allRows.AddRange(element.Table.Rows);
            }

            if (template.PrefaceSection != null)
            {
                foreach (var element in template.PrefaceSection.Elements.Where(e => e.Table != null))
                    allRows.AddRange(element.Table.Rows);
            }

            if (template.ConceptsSection != null)
            {
                foreach (var element in template.ConceptsSection.Elements.Where(e => e.Table != null))
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

            if (template.AttachmentSection != null)
            {
                foreach (var element in template.AttachmentSection.Elements.Where(e => e.Table != null))
                    allColumns.AddRange(element.Table.Columns);
            }

            if (template.ReferenceSection != null)
            {
                foreach (var element in template.ReferenceSection.Elements.Where(e => e.Table != null))
                    allColumns.AddRange(element.Table.Columns);
            }

            if (template.DocumentSection != null)
            {
                foreach (var element in template.DocumentSection.Elements.Where(e => e.Table != null))
                    allColumns.AddRange(element.Table.Columns);
            }

            if (template.PrefaceSection != null)
            {
                foreach (var element in template.PrefaceSection.Elements.Where(e => e.Table != null))
                    allColumns.AddRange(element.Table.Columns);
            }

            if (template.ConceptsSection != null)
            {
                foreach (var element in template.ConceptsSection.Elements.Where(e => e.Table != null))
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

            if (template.AttachmentSection != null)
            {
                foreach (var element in template.AttachmentSection.Elements.Where(e => e.Table != null))
                    allTables.Add(element.Table);
            }

            if (template.ReferenceSection != null)
            {
                foreach (var element in template.ReferenceSection.Elements.Where(e => e.Table != null))
                    allTables.Add(element.Table);
            }

            if (template.DocumentSection != null)
            {
                foreach (var element in template.DocumentSection.Elements.Where(e => e.Table != null))
                    allTables.Add(element.Table);
            }

            if (template.PrefaceSection != null)
            {
                foreach (var element in template.PrefaceSection.Elements.Where(e => e.Table != null))
                    allTables.Add(element.Table);
            }

            if (template.ConceptsSection != null)
            {
                foreach (var element in template.ConceptsSection.Elements.Where(e => e.Table != null))
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

        private async Task DeleteAllBulletListItems(DocumentTemplate template)
        {
            var allItems = new List<BulletListItem>();

            if (template.CoverPage != null)
            {
                foreach (var element in template.CoverPage.Elements.Where(e => e.BulletList != null))
                {
                    allItems.AddRange(element.BulletList.Items);
                }
            }

            if (template.AttachmentSection != null)
            {
                foreach (var element in template.AttachmentSection.Elements.Where(e => e.BulletList != null))
                {
                    allItems.AddRange(element.BulletList.Items);
                }
            }

            if (template.ReferenceSection != null)
            {
                foreach (var element in template.ReferenceSection.Elements.Where(e => e.BulletList != null))
                {
                    allItems.AddRange(element.BulletList.Items);
                }
            }

            if (template.DocumentSection != null)
            {
                foreach (var element in template.DocumentSection.Elements.Where(e => e.BulletList != null))
                {
                    allItems.AddRange(element.BulletList.Items);
                }
            }

            if (template.PrefaceSection != null)
            {
                foreach (var element in template.PrefaceSection.Elements.Where(e => e.BulletList != null))
                {
                    allItems.AddRange(element.BulletList.Items);
                }
            }

            if (template.ConceptsSection != null)
            {
                foreach (var element in template.ConceptsSection.Elements.Where(e => e.BulletList != null))
                {
                    allItems.AddRange(element.BulletList.Items);
                }
            }

            foreach (var master in template.MasterSections)
            {
                foreach (var element in master.Elements.Where(e => e.BulletList != null))
                {
                    allItems.AddRange(element.BulletList.Items);
                }
                foreach (var sub in master.SubSections)
                {
                    foreach (var element in sub.Elements.Where(e => e.BulletList != null))
                    {
                        allItems.AddRange(element.BulletList.Items);
                    }
                }
            }

            foreach (var section in template.Sections)
            {
                foreach (var element in section.Elements.Where(e => e.BulletList != null))
                {
                    allItems.AddRange(element.BulletList.Items);
                }
            }

            if (allItems.Any())
                _context.BulletListItems.RemoveRange(allItems);

            await _context.SaveChangesAsync();
        }

        private async Task DeleteAllBulletLists(DocumentTemplate template)
        {
            var allBulletLists = new List<BulletList>();

            if (template.CoverPage != null)
            {
                foreach (var element in template.CoverPage.Elements.Where(e => e.BulletList != null))
                    allBulletLists.Add(element.BulletList);
            }

            if (template.AttachmentSection != null)
            {
                foreach (var element in template.AttachmentSection.Elements.Where(e => e.BulletList != null))
                    allBulletLists.Add(element.BulletList);
            }

            if (template.ReferenceSection != null)
            {
                foreach (var element in template.ReferenceSection.Elements.Where(e => e.BulletList != null))
                    allBulletLists.Add(element.BulletList);
            }

            if (template.DocumentSection != null)
            {
                foreach (var element in template.DocumentSection.Elements.Where(e => e.BulletList != null))
                    allBulletLists.Add(element.BulletList);
            }

            if (template.PrefaceSection != null)
            {
                foreach (var element in template.PrefaceSection.Elements.Where(e => e.BulletList != null))
                    allBulletLists.Add(element.BulletList);
            }

            if (template.ConceptsSection != null)
            {
                foreach (var element in template.ConceptsSection.Elements.Where(e => e.BulletList != null))
                    allBulletLists.Add(element.BulletList);
            }

            foreach (var master in template.MasterSections)
            {
                foreach (var element in master.Elements.Where(e => e.BulletList != null))
                    allBulletLists.Add(element.BulletList);
                foreach (var sub in master.SubSections)
                {
                    foreach (var element in sub.Elements.Where(e => e.BulletList != null))
                        allBulletLists.Add(element.BulletList);
                }
            }

            foreach (var section in template.Sections)
            {
                foreach (var element in section.Elements.Where(e => e.BulletList != null))
                    allBulletLists.Add(element.BulletList);
            }

            if (allBulletLists.Any())
                _context.BulletLists.RemoveRange(allBulletLists);

            await _context.SaveChangesAsync();
        }

        private async Task DeleteAllElements(DocumentTemplate template)
        {
            var allElements = new List<ContentElement>();

            if (template.CoverPage != null)
                allElements.AddRange(template.CoverPage.Elements);

            if (template.AttachmentSection != null)
                allElements.AddRange(template.AttachmentSection.Elements);

            if (template.ReferenceSection != null)
                allElements.AddRange(template.ReferenceSection.Elements);

            if (template.DocumentSection != null)
                allElements.AddRange(template.DocumentSection.Elements);

            if (template.PrefaceSection != null)
                allElements.AddRange(template.PrefaceSection.Elements);

            if (template.ConceptsSection != null)
                allElements.AddRange(template.ConceptsSection.Elements);

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

            if (template.AttachmentSection != null)
            {
                foreach (var element in template.AttachmentSection.Elements.Where(e => e.Image != null))
                    allImages.Add(element.Image);
            }

            if (template.ReferenceSection != null)
            {
                foreach (var element in template.ReferenceSection.Elements.Where(e => e.Image != null))
                    allImages.Add(element.Image);
            }

            if (template.DocumentSection != null)
            {
                foreach (var element in template.DocumentSection.Elements.Where(e => e.Image != null))
                    allImages.Add(element.Image);
            }

            if (template.PrefaceSection != null)
            {
                foreach (var element in template.PrefaceSection.Elements.Where(e => e.Image != null))
                    allImages.Add(element.Image);
            }

            if (template.ConceptsSection != null)
            {
                foreach (var element in template.ConceptsSection.Elements.Where(e => e.Image != null))
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