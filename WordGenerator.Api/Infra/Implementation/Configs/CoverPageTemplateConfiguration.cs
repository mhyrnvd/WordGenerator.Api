using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Implementation.Configs
{
    public class CoverPageTemplateConfiguration : IEntityTypeConfiguration<CoverPageTemplate>
    {
        public void Configure(EntityTypeBuilder<CoverPageTemplate> builder)
        {
            builder
                .HasMany(x => x.Items)
                .WithOne(x => x.CoverPageTemplate)
                .HasForeignKey(x => x.CoverPageTemplateId);
        }
    }
}
