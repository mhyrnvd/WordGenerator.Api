// WordGenerator.Api.Infra.Configurations/TableDataRowConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class TableDataRowConfiguration : IEntityTypeConfiguration<TableDataRow>
    {
        public void Configure(EntityTypeBuilder<TableDataRow> builder)
        {
            builder.ToTable("TableRows");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RowNumber)
                .IsRequired();

            // رابطه با DynamicTable
            builder.HasOne(x => x.Table)
                .WithMany(x => x.Rows)
                .HasForeignKey(x => x.DynamicTableId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با Cell
            builder.HasMany(x => x.Cells)
                .WithOne(x => x.Row)
                .HasForeignKey(x => x.TableDataRowId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => new { x.DynamicTableId, x.RowNumber });
        }
    }
}