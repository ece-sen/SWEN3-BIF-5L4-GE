using Microsoft.EntityFrameworkCore;
using Paperless.Models;

namespace Paperless.DAL
{
    public class DMSDbContext : DbContext, IDMSDbContext
    {
        public DMSDbContext(DbContextOptions<DMSDbContext> options) : base(options)
        {
        }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Favorite)
                .WithOne(f => f.Document)
                .HasForeignKey<Favorite>(f => f.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => f.DocumentId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
