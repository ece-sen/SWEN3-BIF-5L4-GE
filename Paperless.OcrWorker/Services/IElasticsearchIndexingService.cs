namespace Paperless.OcrWorker.Services;

public interface IElasticsearchIndexingService
{
    Task<bool> TestConnectionAsync();
    Task IndexOcrResultAsync(string documentId, string ocrText, CancellationToken ct = default);
}