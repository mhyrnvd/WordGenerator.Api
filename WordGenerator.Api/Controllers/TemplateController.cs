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

                CoverPage = dto.CoverPage == null
                    ? null
                    : new CoverPageTemplate
                    {
                        Title = dto.CoverPage.Title,

                        Items = dto.CoverPage.Items
                            .OrderBy(x => x.Order)
                            .Select(x => new CoverPageItem
                            {
                                Label = x.Label,
                                Value = x.Value,
                                Order = x.Order
                            })
                            .ToList()
                    },

                Sections = dto.Sections
                    .OrderBy(x => x.Order)
                    .Select(x => new TemplateSection
                    {
                        Title = x.Title,
                        Order = x.Order,

                        Paragraphs = x.Paragraphs
                            .OrderBy(p => p.Order)
                            .Select(p => new SectionParagraph
                            {
                                Text = p.Text,
                                Order = p.Order
                            })
                            .ToList()
                    })
                    .ToList()
            };

            _context.DocumentTemplates.Add(template);
            await _context.SaveChangesAsync();

            return Ok(template.Id);
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
                    HasCoverPage = x.CoverPage != null
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
                .Include(x => x.Sections)
                    .ThenInclude(s => s.Paragraphs)
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
                            })
                            .ToList()
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
                            })
                            .ToList()
                    })
                    .ToList()
            });
        }
    }
}