using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class ReferenceSectionConfiguration : IEntityTypeConfiguration<ReferenceSection>
    {
        public void Configure(EntityTypeBuilder<ReferenceSection> builder)
        {
            builder.ToTable("ReferenceSections");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .HasDefaultValue("ب) References");

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.Order)
                .HasDefaultValue(2);

            // رابطه با DocumentTemplate - Restrict
            builder.HasOne(x => x.DocumentTemplate)
                .WithOne(x => x.ReferenceSection)
                .HasForeignKey<ReferenceSection>(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با Elements - Restrict
            builder.HasMany(x => x.Elements)
                .WithOne(x => x.ReferenceSection)
                .HasForeignKey(x => x.ReferenceSectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}