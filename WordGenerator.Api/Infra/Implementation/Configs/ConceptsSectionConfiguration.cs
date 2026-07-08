using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

public class ConceptsSectionConfiguration : IEntityTypeConfiguration<ConceptsSection>
{
    public void Configure(EntityTypeBuilder<ConceptsSection> builder)
    {
        builder.ToTable("ConceptsSections");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).HasDefaultValue("مفاهیم");
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasOne(x => x.DocumentTemplate)
            .WithOne(x => x.ConceptsSection)
            .HasForeignKey<ConceptsSection>(x => x.DocumentTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Elements)
            .WithOne(x => x.ConceptsSection)
            .HasForeignKey(x => x.ConceptsSectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}