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
                .OnDelete(DeleteBehavior.Restrict);

            // TableColumnDefinition -> TableDataCell (بدون Cascade)
            modelBuilder.Entity<TableColumnDefinition>()
                .HasMany(x => x.Cells)
                .WithOne(x => x.Column)
                .HasForeignKey(x => x.TableColumnDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
    .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
