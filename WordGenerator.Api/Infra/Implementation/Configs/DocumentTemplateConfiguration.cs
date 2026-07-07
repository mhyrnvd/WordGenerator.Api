// WordGenerator.Api.Infra.Configurations/DocumentTemplateConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class DocumentTemplateConfiguration : IEntityTypeConfiguration<DocumentTemplate>
    {
        public void Configure(EntityTypeBuilder<DocumentTemplate> builder)
        {
            builder.ToTable("DocumentTemplates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(500)
                .IsRequired();

            // رابطه با CoverPage
            builder.HasOne(x => x.CoverPage)
                .WithOne(x => x.DocumentTemplate)
                .HasForeignKey<CoverPageTemplate>(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با MasterSection
            builder.HasMany(x => x.MasterSections)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با TemplateSection (Legacy)
            builder.HasMany(x => x.Sections)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با AttachmentSection
            builder.HasOne(x => x.AttachmentSection)
                .WithOne(x => x.DocumentTemplate)
                .HasForeignKey<AttachmentSection>(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با ReferenceSection
            builder.HasOne(x => x.ReferenceSection)
                .WithOne(x => x.DocumentTemplate)
                .HasForeignKey<ReferenceSection>(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با DocumentSection
            builder.HasOne(x => x.DocumentSection)
                .WithOne(x => x.DocumentTemplate)
                .HasForeignKey<DocumentSection>(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => x.Name);
        }
    }
}