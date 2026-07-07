using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class ContentElementConfiguration : IEntityTypeConfiguration<ContentElement>
    {
        public void Configure(EntityTypeBuilder<ContentElement> builder)
        {
            builder.ToTable("ContentElements");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Order)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.ParagraphText)
                .HasMaxLength(4000)
                .IsRequired(false);

            // رابطه با ImageItem
            builder.HasOne(x => x.Image)
                .WithMany()
                .HasForeignKey(x => x.ImageId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با DynamicTable
            builder.HasOne(x => x.Table)
                .WithMany()
                .HasForeignKey(x => x.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با MasterSection
            builder.HasOne(x => x.MasterSection)
                .WithMany(x => x.Elements)
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با SubSection
            builder.HasOne(x => x.SubSection)
                .WithMany(x => x.Elements)
                .HasForeignKey(x => x.SubSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با TemplateSection (Legacy)
            builder.HasOne(x => x.TemplateSection)
                .WithMany(x => x.Elements)
                .HasForeignKey(x => x.TemplateSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با CoverPage
            builder.HasOne(x => x.CoverPage)
                .WithMany(x => x.Elements)
                .HasForeignKey(x => x.CoverPageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BulletList)
                .WithMany()
                .HasForeignKey(x => x.BulletListId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // رابطه با AttachmentSection - Restrict
            builder.HasOne(x => x.AttachmentSection)
                .WithMany(x => x.Elements)
                .HasForeignKey(x => x.AttachmentSectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // رابطه با ReferenceSection - Restrict
            builder.HasOne(x => x.ReferenceSection)
                .WithMany(x => x.Elements)
                .HasForeignKey(x => x.ReferenceSectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // رابطه با DocumentSection - Restrict
            builder.HasOne(x => x.DocumentSection)
                .WithMany(x => x.Elements)
                .HasForeignKey(x => x.DocumentSectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ایندکس‌ها
            builder.HasIndex(x => x.Order);
            builder.HasIndex(x => x.MasterSectionId);
            builder.HasIndex(x => x.SubSectionId);
            builder.HasIndex(x => x.TemplateSectionId);
            builder.HasIndex(x => x.CoverPageId);
            builder.HasIndex(x => new { x.MasterSectionId, x.Order });
            builder.HasIndex(x => new { x.SubSectionId, x.Order });
            builder.HasIndex(x => new { x.TemplateSectionId, x.Order });
            builder.HasIndex(x => new { x.CoverPageId, x.Order });
        }
    }
}