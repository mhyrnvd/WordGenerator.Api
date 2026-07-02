using Microsoft.EntityFrameworkCore;
using WordGenerator.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WordGenerator.Api.Infra.Implementation.Configs
{
    public class PageHeaderConfiguration : IEntityTypeConfiguration<PageHeader>
    {
        public void Configure(EntityTypeBuilder<PageHeader> builder)
        {
            builder.ToTable("PageHeaders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.HeaderText)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // رابطه با DocumentTemplate
            builder.HasOne(x => x.DocumentTemplate)
                .WithOne(x => x.PageHeader)
                .HasForeignKey<PageHeader>(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // رابطه با HeaderLogo
            builder.HasMany(x => x.Logos)
                .WithOne(x => x.PageHeader)
                .HasForeignKey(x => x.PageHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.DocumentTemplateId)
                .IsUnique();
        }
    }
}
