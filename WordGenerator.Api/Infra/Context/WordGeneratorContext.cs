using Microsoft.EntityFrameworkCore;
using WordGenerator.Api.Domain.Entities;
using WordGenerator.Api.Infra.Configurations;
using WordGenerator.Api.Infra.Implementation.Configs;

namespace WordGenerator.Api.Infra.Context
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<CoverPageTemplate> CoverPageTemplates { get; set; }
        public DbSet<CoverPageItem> CoverPageItems { get; set; }
        public DbSet<TemplateSection> TemplateSections { get; set; }
        public DbSet<SectionParagraph> SectionParagraphs { get; set; }
        public DbSet<DynamicTable> DynamicTables { get; set; }
        public DbSet<TableColumnDefinition> TableColumns { get; set; }
        public DbSet<TableDataRow> TableRows { get; set; }
        public DbSet<TableDataCell> TableCells { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new DocumentTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new CoverPageTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new TemplateSectionConfiguration());
            modelBuilder.ApplyConfiguration(new DynamicTableConfiguration());
            modelBuilder.ApplyConfiguration(new TableColumnDefinitionConfiguration());
            modelBuilder.ApplyConfiguration(new TableDataRowConfiguration());
            modelBuilder.ApplyConfiguration(new TableDataCellConfiguration());

            modelBuilder.Entity<DynamicTable>()
               .HasMany(x => x.Columns)
               .WithOne(x => x.Table)
               .HasForeignKey(x => x.DynamicTableId)
               .OnDelete(DeleteBehavior.Cascade);

            // DynamicTable -> TableDataRow
            modelBuilder.Entity<DynamicTable>()
                .HasMany(x => x.Rows)
                .WithOne(x => x.Table)
                .HasForeignKey(x => x.DynamicTableId)
                .OnDelete(DeleteBehavior.Cascade);

            // TableDataRow -> TableDataCell (بدون Cascade)
            modelBuilder.Entity<TableDataRow>()
                .HasMany(x => x.Cells)
                .WithOne(x => x.Row)
                .HasForeignKey(x => x.TableDataRowId)
                .OnDelete(DeleteBehavior.Cascade);

            // TableColumnDefinition -> TableDataCell (بدون Cascade)
            modelBuilder.Entity<TableColumnDefinition>()
                .HasMany(x => x.Cells)
                .WithOne(x => x.Column)
                .HasForeignKey(x => x.TableColumnDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TemplateSection>()
       .HasOne(s => s.Template)
       .WithMany(t => t.Sections)
       .HasForeignKey(s => s.TemplateId)
       .OnDelete(DeleteBehavior.Cascade);

            // رابطه TemplateSection با SectionParagraph - Cascade Delete
            modelBuilder.Entity<SectionParagraph>()
                .HasOne(p => p.Section)
                .WithMany(s => s.Paragraphs)
                .HasForeignKey(p => p.TemplateSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // رابطه TemplateSection با DynamicTable - Cascade Delete
            modelBuilder.Entity<DynamicTable>()
                .HasOne(t => t.Section)
                .WithMany(s => s.Tables)
                .HasForeignKey(t => t.TemplateSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // رابطه DynamicTable با TableColumnDefinition - Cascade Delete
            modelBuilder.Entity<TableColumnDefinition>()
                .HasOne(c => c.Table)
                .WithMany(t => t.Columns)
                .HasForeignKey(c => c.DynamicTableId)
                .OnDelete(DeleteBehavior.Cascade);

            // رابطه DynamicTable با TableDataRow - Cascade Delete
            modelBuilder.Entity<TableDataRow>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Rows)
                .HasForeignKey(r => r.DynamicTableId)
                .OnDelete(DeleteBehavior.Cascade);

            // رابطه TableDataRow با TableDataCell - Cascade Delete
            modelBuilder.Entity<TableDataCell>()
                .HasOne(c => c.Row)
                .WithMany(r => r.Cells)
                .HasForeignKey(c => c.TableDataRowId)
                .OnDelete(DeleteBehavior.Cascade);

            // رابطه TableColumnDefinition با TableDataCell - Restrict (برای جلوگیری از حذف ستون‌هایی که سلول دارند)
            modelBuilder.Entity<TableDataCell>()
                .HasOne(c => c.Column)
                .WithMany(c => c.Cells)
                .HasForeignKey(c => c.TableColumnDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه CoverPageTemplate با CoverPageItem - Cascade Delete
            modelBuilder.Entity<CoverPageItem>()
                .HasOne(i => i.CoverPageTemplate)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CoverPageTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // رابطه CoverPageTemplate با DynamicTable - Cascade Delete
            modelBuilder.Entity<DynamicTable>()
                .HasOne(t => t.CoverPage)
                .WithMany(c => c.Tables)
                .HasForeignKey(t => t.CoverPageTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
