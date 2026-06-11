// Infra/Configurations/MasterSectionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class MasterSectionConfiguration : IEntityTypeConfiguration<MasterSection>
    {
        public void Configure(EntityTypeBuilder<MasterSection> builder)
        {
            builder.ToTable("MasterSections");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.ShowInToc)
                .HasDefaultValue(true);

            // رابطه با DocumentTemplate
            builder.HasOne(x => x.DocumentTemplate)
                .WithMany(x => x.MasterSections)
                .HasForeignKey(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با SubSections
            builder.HasMany(x => x.SubSections)
                .WithOne(x => x.MasterSection)
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با پاراگراف‌ها
            builder.HasMany(x => x.Paragraphs)
                .WithOne(x => x.MasterSection)
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با جداول
            builder.HasMany(x => x.Tables)
                .WithOne(x => x.MasterSection)
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}