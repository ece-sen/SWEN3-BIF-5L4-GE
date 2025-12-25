namespace Paperless.OcrWorker.Services.Elasticsearch;

public interface IElasticClientWrapper
{
    Task<bool> PingAsync(CancellationToken ct = default);
    Task<bool> IndexAsync(string indexName, string id, object document, CancellationToken ct = default);
}