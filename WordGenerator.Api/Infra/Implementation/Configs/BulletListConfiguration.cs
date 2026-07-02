using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class BulletListConfiguration : IEntityTypeConfiguration<BulletList>
    {
        public void Configure(EntityTypeBuilder<BulletList> builder)
        {
            builder.ToTable("BulletLists");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.Order)
                .IsRequired()
                .HasDefaultValue(1);

            // رابطه با MasterSection - Restrict
            builder.HasOne(x => x.MasterSection)
                .WithMany()
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // رابطه با SubSection - Restrict
            builder.HasOne(x => x.SubSection)
                .WithMany()
                .HasForeignKey(x => x.SubSectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // رابطه با TemplateSection - Restrict
            builder.HasOne(x => x.TemplateSection)
                .WithMany()
                .HasForeignKey(x => x.TemplateSectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // رابطه با CoverPage - Restrict
            builder.HasOne(x => x.CoverPage)
                .WithMany()
                .HasForeignKey(x => x.CoverPageId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // رابطه با آیتم‌های لیست - Restrict (والد نمی‌تونه حذف بشه اگر آیتم داشته باشه)
            builder.HasMany(x => x.Items)
                .WithOne(x => x.BulletList)
                .HasForeignKey(x => x.BulletListId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => new { x.MasterSectionId, x.Order });
            builder.HasIndex(x => new { x.SubSectionId, x.Order });
            builder.HasIndex(x => new { x.TemplateSectionId, x.Order });
            builder.HasIndex(x => new { x.CoverPageId, x.Order });
        }
    }
}