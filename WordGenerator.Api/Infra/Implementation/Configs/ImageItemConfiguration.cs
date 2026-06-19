using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Implementation.Configs
{
    public class ImageItemConfiguration : IEntityTypeConfiguration<ImageItem>
    {
        public void Configure(EntityTypeBuilder<ImageItem> builder)
        {
            builder.ToTable("Images");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            builder.Property(e => e.ImageData).IsRequired();
            builder.Property(e => e.Caption).HasMaxLength(500);
            builder.Property(e => e.Width).HasDefaultValue(500);
            builder.Property(e => e.Height).HasDefaultValue(0);

            // روابط
            builder.HasOne(e => e.Section)
                .WithMany(s => s.Images)
                .HasForeignKey(e => e.TemplateSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.MasterSection)
                .WithMany(s => s.Images)
                .HasForeignKey(e => e.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.SubSection)
                .WithMany(s => s.Images)
                .HasForeignKey(e => e.SubSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.CoverPage)
                .WithMany(s => s.Images)
                .HasForeignKey(e => e.CoverPageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
