using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Implementation.Configs
{
    public class TemplateSectionConfiguration : IEntityTypeConfiguration<TemplateSection>
    {
        public void Configure(EntityTypeBuilder<TemplateSection> builder)
        {
            builder.Property(x => x.Title)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasOne(x => x.Template)
                .WithMany(x => x.Sections)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Paragraphs)
                .WithOne(x => x.Section)
                .HasForeignKey(x => x.TemplateSectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
