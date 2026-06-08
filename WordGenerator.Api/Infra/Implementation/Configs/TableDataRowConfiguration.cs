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

            builder.Property(x => x.RowNumber).IsRequired();
            builder.Property(x => x.DynamicTableId).IsRequired();
        }
    }
}