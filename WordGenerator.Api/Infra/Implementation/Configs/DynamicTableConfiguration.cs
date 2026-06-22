// WordGenerator.Api.Infra.Configurations/DynamicTableConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class DynamicTableConfiguration : IEntityTypeConfiguration<DynamicTable>
    {
        public void Configure(EntityTypeBuilder<DynamicTable> builder)
        {
            builder.ToTable("DynamicTables");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.ShowRowNumbers)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.RowNumberHeader)
                .HasMaxLength(100)
                .IsRequired(false);

            // رابطه با ColumnDefinition
            builder.HasMany(x => x.Columns)
                .WithOne(x => x.Table)
                .HasForeignKey(x => x.DynamicTableId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با Row
            builder.HasMany(x => x.Rows)
                .WithOne(x => x.Table)
                .HasForeignKey(x => x.DynamicTableId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => x.Order);
        }
    }
}