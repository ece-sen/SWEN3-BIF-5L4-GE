using Microsoft.EntityFrameworkCore;
using Paperless.Models;

namespace Paperless.DAL;

public interface IDMSDbContext
{
    DbSet<Document> Documents
    {
        get; set; 
    }
    DbSet<Favorite> Favorites
    {
        get; set;
    }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
