namespace Paperless.BatchProcess.Models;

public class AccessLogEntry
{
    public DateOnly Date { get; set; }
    public int DocumentId { get; set; }
    public int AccessCount { get; set; }
}