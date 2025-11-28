using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.OcrWorker.Services
{
    public class OcrWorker
    {
        private readonly IStorageService _storage;
        private readonly OcrService _ocr;
        private readonly string _bucket;

        public OcrWorker(IStorageService storage, OcrService ocr, string bucket)
        {
            _storage = storage;
            _ocr = ocr;
            _bucket = bucket;
        }

        public async Task ProcessDocumentAsync(string id)
        {
            string tempPdf = Path.Combine(Path.GetTempPath(), $"{id}.pdf");

            try
            {
                await _storage.DownloadFileAsync(_bucket, $"{id}.pdf", tempPdf);

                string text = _ocr.ExtractTextFromPdf(tempPdf);

                await _storage.UploadTextAsync(_bucket, $"{id}.txt", text);
            }
            finally
            {
                if (File.Exists(tempPdf)) File.Delete(tempPdf);
            }
        }
    }
}
