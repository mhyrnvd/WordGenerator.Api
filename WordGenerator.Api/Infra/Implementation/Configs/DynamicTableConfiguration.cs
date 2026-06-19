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

            builder.Property(x => x.Title).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Order).IsRequired();
            builder.Property(x => x.ShowRowNumbers).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.RowNumberHeader).HasMaxLength(100).IsRequired(false);

            builder.HasOne(x => x.CoverPage)
                .WithMany(x => x.Tables)
                .HasForeignKey(x => x.CoverPageTemplateId)
                .OnDelete(DeleteBehavior.Cascade); // تغییر به SetNull

            builder.HasOne(x => x.Section)
                .WithMany(x => x.Tables)
                .HasForeignKey(x => x.TemplateSectionId)
                .OnDelete(DeleteBehavior.Cascade); // تغییر به SetNull
        }
    }
}