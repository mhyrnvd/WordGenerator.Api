// WordGenerator.Api.Infra.Configurations/TableDataCellConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class TableDataCellConfiguration : IEntityTypeConfiguration<TableDataCell>
    {
        public void Configure(EntityTypeBuilder<TableDataCell> builder)
        {
            builder.ToTable("TableCells");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Value)
                .HasMaxLength(4000)
                .IsRequired(false);

            // رابطه با Row
            builder.HasOne(x => x.Row)
                .WithMany(x => x.Cells)
                .HasForeignKey(x => x.TableDataRowId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با Column
            builder.HasOne(x => x.Column)
                .WithMany(x => x.Cells)
                .HasForeignKey(x => x.TableColumnDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            //builder.HasIndex(x => new { x.TableDataRowId, x.TableColumnDefinitionId })
            //    .IsUnique();
        }
    }
}