using Paperless.BatchProcess.Models;

namespace Paperless.BatchProcess.Services;

public interface IAccessLogReader
{
    IEnumerable<AccessLogEntry> Read(string filePath);

}