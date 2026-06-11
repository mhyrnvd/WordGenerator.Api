// Infra/Configurations/SubSectionParagraphConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class SubSectionParagraphConfiguration : IEntityTypeConfiguration<SubSectionParagraph>
    {
        public void Configure(EntityTypeBuilder<SubSectionParagraph> builder)
        {
            builder.ToTable("SubSectionParagraphs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(x => x.Order)
                .IsRequired();
        }
    }
}