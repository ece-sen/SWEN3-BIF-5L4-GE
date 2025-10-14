using Microsoft.EntityFrameworkCore;
using Paperless.Models;

namespace Paperless.DAL;

public interface IDMSDbContext
{
    DbSet<Document> Documents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
