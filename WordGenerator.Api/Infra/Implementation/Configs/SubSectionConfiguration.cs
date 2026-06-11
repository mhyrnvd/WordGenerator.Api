// Infra/Configurations/SubSectionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class SubSectionConfiguration : IEntityTypeConfiguration<SubSection>
    {
        public void Configure(EntityTypeBuilder<SubSection> builder)
        {
            builder.ToTable("SubSections");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.ShowInToc)
                .HasDefaultValue(true);

            // رابطه با MasterSection
            builder.HasOne(x => x.MasterSection)
                .WithMany(x => x.SubSections)
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با پاراگراف‌ها
            builder.HasMany(x => x.Paragraphs)
                .WithOne(x => x.SubSection)
                .HasForeignKey(x => x.SubSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با جداول
            builder.HasMany(x => x.Tables)
                .WithOne(x => x.SubSection)
                .HasForeignKey(x => x.SubSectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}