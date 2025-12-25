using Elastic.Clients.Elasticsearch;

namespace Paperless.OcrWorker.Services.Elasticsearch;

public class ElasticClientWrapper: IElasticClientWrapper
{
    private readonly ElasticsearchClient _client;

    public ElasticClientWrapper(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        var resp = await _client.PingAsync(ct);
        return resp.IsSuccess();
    }

    public async Task<bool> IndexAsync(string indexName, string id, object document, CancellationToken ct = default)
    {
        var resp = await _client.IndexAsync(document, i => i.Index(indexName).Id(id), ct);
        return resp.IsSuccess();
    }
}