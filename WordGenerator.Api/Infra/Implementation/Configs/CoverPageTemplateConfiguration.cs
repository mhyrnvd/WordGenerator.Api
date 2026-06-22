// WordGenerator.Api.Infra.Configurations/CoverPageTemplateConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class CoverPageTemplateConfiguration : IEntityTypeConfiguration<CoverPageTemplate>
    {
        public void Configure(EntityTypeBuilder<CoverPageTemplate> builder)
        {
            builder.ToTable("CoverPageTemplates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(500)
                .IsRequired(false);

            // رابطه با DocumentTemplate
            builder.HasOne(x => x.DocumentTemplate)
                .WithOne(x => x.CoverPage)
                .HasForeignKey<CoverPageTemplate>(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با CoverPageItem
            builder.HasMany(x => x.Items)
                .WithOne(x => x.CoverPageTemplate)
                .HasForeignKey(x => x.CoverPageTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با ContentElement
            builder.HasMany(x => x.Elements)
                .WithOne(x => x.CoverPage)
                .HasForeignKey(x => x.CoverPageId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => x.DocumentTemplateId)
                .IsUnique();
        }
    }
}