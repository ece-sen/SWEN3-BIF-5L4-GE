using System.Diagnostics.Contracts;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Paperless.OcrWorker.Services
{
    public class ElasticsearchIndexingService: IElasticsearchIndexingService
    {
        private readonly ElasticsearchClient _client;

        public ElasticsearchIndexingService(string url = "http://elasticsearch:9200")
        {
            var settings = new ElasticsearchClientSettings(new Uri(url))
                .DefaultIndex("documents");

            _client = new ElasticsearchClient(settings);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var ping = await _client.PingAsync();
                Console.WriteLine($"[ES] Ping success: {ping.IsSuccess()}");
                return ping.IsSuccess();
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

            if (ocrText == null)
                ocrText = string.Empty;

            var doc = new
            {
                Id = documentId,
                Content = ocrText
            };

            var result = await _client.IndexAsync(doc, i => i.Index("documents").Id(documentId), ct);
            if (!result.IsSuccess())
                throw new Exception($"[ES] Failed to Index {documentId}. Debug: {result.DebugInformation}");

            Console.WriteLine($"[ES] Indexed OCR result for document id {documentId} successfully.");
        }
    }
}
