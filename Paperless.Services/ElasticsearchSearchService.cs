using Paperless.Services.Elasticsearch;

namespace Paperless.Services
{
    public class ElasticsearchSearchService : IElasticsearchSearchService
    {
        private readonly IElasticSearchClientWrapper _elastic;

        public ElasticsearchSearchService(IElasticSearchClientWrapper elastic)
        {
            _elastic = elastic;
        }

        public async Task<List<string>> SearchDocumentIdsAsync(string query)
        {
            return await _elastic.SearchIdsAsync(
                index: "documents",
                field: "content",
                query: query
            );
        }
    }
}
