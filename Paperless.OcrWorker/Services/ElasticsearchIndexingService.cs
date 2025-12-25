using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Paperless.OcrWorker.Services.Elasticsearch;
using System.Diagnostics.Contracts;

namespace Paperless.OcrWorker.Services
{
    public class ElasticsearchIndexingService: IElasticsearchIndexingService
    {
        private readonly IElasticClientWrapper _wrapper;
        private const string IndexName = "documents";

        // production ctor
        public ElasticsearchIndexingService(string url = "http://elasticsearch:9200")
            : this(CreateWrapper(url))
        { }

        // testable ctor
        public ElasticsearchIndexingService(IElasticClientWrapper wrapper)
        {
            _wrapper = wrapper;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var ok = await _wrapper.PingAsync();
                Console.WriteLine($"[ES] Ping success: {ok}");
                return ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ES] Ping failed: {ex.Message}");
                return false;
            }
        }

        public async Task IndexOcrResultAsync(string documentId, string ocrText, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("documentId must not be empty", nameof(documentId));

            ocrText ??= string.Empty;

            var doc = new { Id = documentId, Content = ocrText };

            var ok = await _wrapper.IndexAsync(IndexName, documentId, doc, ct);
            if (!ok)
                throw new Exception($"[ES] Failed to Index {documentId}.");

            Console.WriteLine($"[ES] Indexed OCR result for document id {documentId} successfully.");
        }

        private static IElasticClientWrapper CreateWrapper(string url)
        {
            var settings = new ElasticsearchClientSettings(new Uri(url))
                .DefaultIndex(IndexName);

            var client = new ElasticsearchClient(settings);
            return new ElasticClientWrapper(client);
        }
    }
}
