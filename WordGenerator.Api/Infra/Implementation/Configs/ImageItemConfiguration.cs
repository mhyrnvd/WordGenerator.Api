// WordGenerator.Api.Infra.Configurations/ImageItemConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class ImageItemConfiguration : IEntityTypeConfiguration<ImageItem>
    {
        public void Configure(EntityTypeBuilder<ImageItem> builder)
        {
            builder.ToTable("Images");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ImageData)
                .IsRequired();

            builder.Property(x => x.Caption)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(x => x.Order)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.Width)
                .IsRequired()
                .HasDefaultValue(500);

            builder.Property(x => x.Height)
                .IsRequired()
                .HasDefaultValue(0);

            // رابطه با MasterSection (برای سازگاری با روش قدیمی)
            builder.HasOne(x => x.MasterSection)
                .WithMany()
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با SubSection (برای سازگاری با روش قدیمی)
            builder.HasOne(x => x.SubSection)
                .WithMany()
                .HasForeignKey(x => x.SubSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با TemplateSection (برای سازگاری با روش قدیمی)
            builder.HasOne(x => x.Section)
                .WithMany()
                .HasForeignKey(x => x.TemplateSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با CoverPage (برای سازگاری با روش قدیمی)
            builder.HasOne(x => x.CoverPage)
                .WithMany()
                .HasForeignKey(x => x.CoverPageId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => x.Order);
        }
    }
}