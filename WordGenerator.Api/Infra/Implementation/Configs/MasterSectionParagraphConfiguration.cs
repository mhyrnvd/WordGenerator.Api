// Infra/Configurations/MasterSectionParagraphConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class MasterSectionParagraphConfiguration : IEntityTypeConfiguration<MasterSectionParagraph>
    {
        public void Configure(EntityTypeBuilder<MasterSectionParagraph> builder)
        {
            builder.ToTable("MasterSectionParagraphs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(x => x.Order)
                .IsRequired();
        }
    }
}