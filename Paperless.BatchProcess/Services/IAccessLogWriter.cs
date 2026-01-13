using Paperless.BatchProcess.Models;

namespace Paperless.BatchProcess.Services;

public interface IAccessLogWriter
{
    Task SaveAsync(IEnumerable<AccessLogEntry> entries);
}