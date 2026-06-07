using Microsoft.EntityFrameworkCore;
using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Infra.Context
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
