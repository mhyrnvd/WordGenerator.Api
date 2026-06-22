using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class HeaderLogoConfiguration : IEntityTypeConfiguration<HeaderLogo>
    {
        public void Configure(EntityTypeBuilder<HeaderLogo> builder)
        {
            builder.ToTable("HeaderLogos");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ImageData)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.Width)
                .IsRequired()
                .HasDefaultValue(60);

            builder.Property(x => x.Height)
                .IsRequired()
                .HasDefaultValue(40);

            // رابطه با PageHeader
            builder.HasOne(x => x.PageHeader)
                .WithMany(x => x.Logos)
                .HasForeignKey(x => x.PageHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.PageHeaderId, x.Order });
        }
    }
}