// WordGenerator.Api.Infra.Context/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using WordGenerator.Api.Domain.Entities;
using WordGenerator.Api.Infra.Configurations;
using WordGenerator.Api.Infra.Implementation.Configs;

namespace WordGenerator.Api.Infra.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<CoverPageTemplate> CoverPageTemplates { get; set; }
        public DbSet<CoverPageItem> CoverPageItems { get; set; }
        public DbSet<MasterSection> MasterSections { get; set; }
        public DbSet<SubSection> SubSections { get; set; }
        public DbSet<TemplateSection> TemplateSections { get; set; }
        public DbSet<ContentElement> ContentElements { get; set; }
        public DbSet<ImageItem> Images { get; set; }
        public DbSet<DynamicTable> DynamicTables { get; set; }
        public DbSet<TableColumnDefinition> TableColumns { get; set; }
        public DbSet<TableDataRow> TableRows { get; set; }
        public DbSet<TableDataCell> TableCells { get; set; }
        public DbSet<HeaderLogo> HeaderLogos { get; set; }
        public DbSet<PageHeader> PageHeaders { get; set; }
        public DbSet<BulletList> BulletLists { get; set; } = null!;
        public DbSet<BulletListItem> BulletListItems { get; set; } = null!;
        public DbSet<AttachmentSection> AttachmentSections { get; set; } = null!;
        public DbSet<ReferenceSection> ReferenceSections { get; set; } = null!;
        public DbSet<DocumentSection> DocumentSections { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ثبت همه Configurations
            modelBuilder.ApplyConfiguration(new ContentElementConfiguration());
            modelBuilder.ApplyConfiguration(new ImageItemConfiguration());
            modelBuilder.ApplyConfiguration(new MasterSectionConfiguration());
            modelBuilder.ApplyConfiguration(new SubSectionConfiguration());
            modelBuilder.ApplyConfiguration(new TemplateSectionConfiguration());
            modelBuilder.ApplyConfiguration(new CoverPageTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new DynamicTableConfiguration());
            modelBuilder.ApplyConfiguration(new TableColumnDefinitionConfiguration());
            modelBuilder.ApplyConfiguration(new TableDataRowConfiguration());
            modelBuilder.ApplyConfiguration(new TableDataCellConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new PageHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new HeaderLogoConfiguration());
            modelBuilder.ApplyConfiguration(new BulletListItemConfiguration());
            modelBuilder.ApplyConfiguration(new BulletListConfiguration());
            modelBuilder.ApplyConfiguration(new AttachmentSectionConfiguration());
            modelBuilder.ApplyConfiguration(new ReferenceSectionConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentSectionConfiguration());
        }
    }
}