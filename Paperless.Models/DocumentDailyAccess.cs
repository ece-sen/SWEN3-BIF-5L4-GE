namespace Paperless.Models;

public class DocumentDailyAccess
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public DateOnly Date { get; set; }
    public int AccessCount { get; set; }
    public Document Document { get; set; } = null!;
}