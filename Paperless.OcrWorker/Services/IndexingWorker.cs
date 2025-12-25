using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.OcrWorker.Services
{
    public class IndexingWorker
    {
        private readonly IStorageService _storage;
        private readonly IElasticsearchIndexingService _indexingService;
        private readonly string _bucket;

        public IndexingWorker(IStorageService storage, IElasticsearchIndexingService indexingService, string bucket)
        {
            _storage = storage;
            _indexingService = indexingService;
            _bucket = bucket;
        }

        public async Task ProcessAsync(string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("documentId must not be empty", nameof(documentId));

            var tempTxt = Path.Combine(Path.GetTempPath(), $"{documentId}.txt");

            Console.WriteLine($"[INDEX] Starting indexing for documentId={documentId}");

            try
            {
                await _storage.DownloadFileAsync(_bucket, $"{documentId}.txt", tempTxt);
                var text = await File.ReadAllTextAsync(tempTxt);

                await _indexingService.IndexOcrResultAsync(documentId, text);

                Console.WriteLine($"[INDEX] Finished indexing for documentId={documentId}");
            }
            finally
            {
                if (File.Exists(tempTxt)) File.Delete(tempTxt);
                Console.WriteLine($"[INDEX] Deleted temp file: {tempTxt}");
            }
        }
    }
}
