using Microsoft.EntityFrameworkCore;
using Paperless.BatchProcess.Models;
using Paperless.DAL;
using Paperless.Models;
using Paperless.BatchProcess.Exceptions;

namespace Paperless.BatchProcess.Services;

public class PostgresAccessLogWriter : IAccessLogWriter
{
    private readonly DMSDbContext _context;

    public PostgresAccessLogWriter(DMSDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(IEnumerable<AccessLogEntry> entries)
    {
        foreach (var entry in entries)
        {
            var documentExists = await _context.Documents
                .AnyAsync(d => d.Id == entry.DocumentId);

            if (!documentExists)
            {
                throw new DocumentNotFoundException(entry.DocumentId);
            }

            var existing = await _context.DocumentDailyAccesses
                .FirstOrDefaultAsync(x =>
                    x.DocumentId == entry.DocumentId &&
                    x.Date == entry.Date);

            if (existing == null)
            {
                _context.DocumentDailyAccesses.Add(new DocumentDailyAccess
                {
                    DocumentId = entry.DocumentId,
                    Date = entry.Date,
                    AccessCount = entry.AccessCount
                });
            }
            else
            {
                existing.AccessCount += entry.AccessCount;
            }
        }

        await _context.SaveChangesAsync();
    }
}