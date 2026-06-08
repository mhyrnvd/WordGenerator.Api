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

            builder.Property(x => x.Header).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Width).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.Order).IsRequired();
            builder.Property(x => x.IsRowNumberColumn).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.DynamicTableId).IsRequired();
        }
    }
}