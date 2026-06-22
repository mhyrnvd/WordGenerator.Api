// WordGenerator.Api.Infra.Configurations/TableColumnDefinitionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class TableColumnDefinitionConfiguration : IEntityTypeConfiguration<TableColumnDefinition>
    {
        public void Configure(EntityTypeBuilder<TableColumnDefinition> builder)
        {
            builder.ToTable("TableColumns");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Header)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Width)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.IsRowNumberColumn)
                .IsRequired()
                .HasDefaultValue(false);

            // رابطه با DynamicTable
            builder.HasOne(x => x.Table)
                .WithMany(x => x.Columns)
                .HasForeignKey(x => x.DynamicTableId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با Cell
            builder.HasMany(x => x.Cells)
                .WithOne(x => x.Column)
                .HasForeignKey(x => x.TableColumnDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => new { x.DynamicTableId, x.Order });
            builder.HasIndex(x => x.Order);
        }
    }
}