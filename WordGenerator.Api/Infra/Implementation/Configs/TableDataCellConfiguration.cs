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
                .IsRequired(false)
                .HasMaxLength(4000);

            // فقط فیلدها رو تعریف کن، هیچ رابطه‌ای تعریف نکن
            builder.Property(x => x.TableDataRowId).IsRequired();
            builder.Property(x => x.TableColumnDefinitionId).IsRequired();

            // ایندکس یکتا
            builder.HasIndex(x => new { x.TableDataRowId, x.TableColumnDefinitionId })
                .IsUnique();
        }
    }
}