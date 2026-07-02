using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Configurations
{
    public class BulletListItemConfiguration : IEntityTypeConfiguration<BulletListItem>
    {
        public void Configure(EntityTypeBuilder<BulletListItem> builder)
        {
            builder.ToTable("BulletListItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.Level)
                .IsRequired()
                .HasDefaultValue(0);

            // رابطه با BulletList - Restrict
            builder.HasOne(x => x.BulletList)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.BulletListId)
                .OnDelete(DeleteBehavior.Restrict);

            // ایندکس‌ها
            builder.HasIndex(x => new { x.BulletListId, x.Order });
            builder.HasIndex(x => x.Level);
        }
    }
}