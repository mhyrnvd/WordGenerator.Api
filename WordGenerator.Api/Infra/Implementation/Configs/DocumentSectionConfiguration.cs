using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class DocumentSectionConfiguration : IEntityTypeConfiguration<DocumentSection>
    {
        public void Configure(EntityTypeBuilder<DocumentSection> builder)
        {
            builder.ToTable("DocumentSections");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .HasDefaultValue("پ) مدارک");

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.Order)
                .HasDefaultValue(3);

            // رابطه با DocumentTemplate - Restrict
            builder.HasOne(x => x.DocumentTemplate)
                .WithOne(x => x.DocumentSection)
                .HasForeignKey<DocumentSection>(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با Elements - Restrict
            builder.HasMany(x => x.Elements)
                .WithOne(x => x.DocumentSection)
                .HasForeignKey(x => x.DocumentSectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}