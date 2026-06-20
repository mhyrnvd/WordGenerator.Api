// WordGenerator.Api.Infra.Configurations/MasterSectionConfiguration.cs
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
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.ShowInToc)
                .IsRequired()
                .HasDefaultValue(true);

            // رابطه با DocumentTemplate
            builder.HasOne(x => x.Template)
                .WithMany(x => x.MasterSections)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با SubSection
            builder.HasMany(x => x.SubSections)
                .WithOne(x => x.MasterSection)
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با ContentElement
            builder.HasMany(x => x.Elements)
                .WithOne(x => x.MasterSection)
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => new { x.TemplateId, x.Order });
            builder.HasIndex(x => x.Order);
        }
    }
}