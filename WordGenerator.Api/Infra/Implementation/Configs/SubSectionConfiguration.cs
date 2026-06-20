// WordGenerator.Api.Infra.Configurations/SubSectionConfiguration.cs
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
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.ShowInToc)
                .IsRequired()
                .HasDefaultValue(true);

            // رابطه با MasterSection
            builder.HasOne(x => x.MasterSection)
                .WithMany(x => x.SubSections)
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با ContentElement
            builder.HasMany(x => x.Elements)
                .WithOne(x => x.SubSection)
                .HasForeignKey(x => x.SubSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => new { x.MasterSectionId, x.Order });
            builder.HasIndex(x => x.Order);
        }
    }
}