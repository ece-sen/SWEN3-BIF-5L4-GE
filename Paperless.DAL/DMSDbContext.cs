using Microsoft.EntityFrameworkCore;
using Paperless.Models;

namespace Paperless.DAL
{
    public class DMSDbContext : DbContext
    {
        public DMSDbContext(DbContextOptions<DMSDbContext> options) : base(options)
        {
        }
        public DbSet<Document> Documents { get; set; }
    }
}
