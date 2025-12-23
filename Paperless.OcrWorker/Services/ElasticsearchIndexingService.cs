using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Paperless.OcrWorker.Services
{
    public class ElasticsearchIndexingService
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
    }
}
