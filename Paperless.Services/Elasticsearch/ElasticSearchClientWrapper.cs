using Elastic.Clients.Elasticsearch;

namespace Paperless.Services.Elasticsearch
{
    public class ElasticSearchClientWrapper : IElasticSearchClientWrapper
    {
        private readonly ElasticsearchClient _client;

        public ElasticSearchClientWrapper(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task<List<string>> SearchIdsAsync(
            string index,
            string field,
            string query,
            CancellationToken ct = default)
        {
            var response = await _client.SearchAsync<object>(s => s
                .Index(index)
                .Query(q => q
                    .Match(m => m
                        .Field(field)
                        .Query(query)
                    )
                ),
                ct
            );

            if (!response.IsSuccess())
                throw new Exception(response.DebugInformation);

            return response.Hits
                .Select(h => h.Id!)
                .ToList();
        }
    }
}
