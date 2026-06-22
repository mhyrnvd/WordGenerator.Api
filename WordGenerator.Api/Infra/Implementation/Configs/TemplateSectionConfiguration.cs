// WordGenerator.Api.Infra.Configurations/TemplateSectionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class TemplateSectionConfiguration : IEntityTypeConfiguration<TemplateSection>
    {
        public void Configure(EntityTypeBuilder<TemplateSection> builder)
        {
            builder.ToTable("TemplateSections");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            // رابطه با DocumentTemplate
            builder.HasOne(x => x.Template)
                .WithMany(x => x.Sections)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با ContentElement
            builder.HasMany(x => x.Elements)
                .WithOne(x => x.TemplateSection)
                .HasForeignKey(x => x.TemplateSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => new { x.TemplateId, x.Order });
            builder.HasIndex(x => x.Order);
        }
    }
}