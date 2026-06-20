using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Implementation.Configs
{
    public class ImageGroupConfiguration : IEntityTypeConfiguration<ImageGroup>
    {
        public void Configure(EntityTypeBuilder<ImageGroup> builder)
        {
            builder.ToTable("ImageGroups");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.Order)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.ImagesPerRow)
                .IsRequired()
                .HasDefaultValue(2);

            // روابط با ImageItem
            builder.HasMany(x => x.Images)
                .WithOne(x => x.ImageGroup)
                .HasForeignKey(x => x.ImageGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها برای بهبود عملکرد
            builder.HasIndex(x => x.Order);
            builder.HasIndex(x => x.MasterSectionId);
            builder.HasIndex(x => x.SubSectionId);
            builder.HasIndex(x => x.TemplateSectionId);
            builder.HasIndex(x => x.CoverPageId);

        }
    }
}