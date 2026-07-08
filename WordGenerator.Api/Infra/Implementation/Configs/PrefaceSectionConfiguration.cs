using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

public class PrefaceSectionConfiguration : IEntityTypeConfiguration<PrefaceSection>
{
    public void Configure(EntityTypeBuilder<PrefaceSection> builder)
    {
        builder.ToTable("PrefaceSections");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).HasDefaultValue("پیش‌گفتار");
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasOne(x => x.DocumentTemplate)
            .WithOne(x => x.PrefaceSection)
            .HasForeignKey<PrefaceSection>(x => x.DocumentTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Elements)
            .WithOne(x => x.PrefaceSection)
            .HasForeignKey(x => x.PrefaceSectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
