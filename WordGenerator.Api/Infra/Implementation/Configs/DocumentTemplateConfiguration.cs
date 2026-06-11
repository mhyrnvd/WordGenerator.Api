using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Implementation.Configs
{
    public class DocumentTemplateConfiguration : IEntityTypeConfiguration<DocumentTemplate>
    {
        public void Configure(EntityTypeBuilder<DocumentTemplate> builder)
        {
            builder
                .HasOne(x => x.CoverPage)
                .WithOne(x => x.DocumentTemplate)
                .HasForeignKey<CoverPageTemplate>(x => x.DocumentTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
