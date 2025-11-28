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

            Console.WriteLine($"[OCRWorker] Starting OCR job for Document ID={id}");


            try
            {
                Console.WriteLine($"[OCRWorker] Downloading PDF '{id}.pdf' from MinIO bucket '{_bucket}'...");

                await _storage.DownloadFileAsync(_bucket, $"{id}.pdf", tempPdf);

                Console.WriteLine($"[OCRWorker] Download complete: {tempPdf}");

                string text = _ocr.ExtractTextFromPdf(tempPdf);
                
                Console.WriteLine($"[OCRWorker] OCR complete. Extracted {text.Length} characters.");

                Console.WriteLine($"[OCRWorker] Uploading OCR result to MinIO as '{id}.txt'...");

                await _storage.UploadTextAsync(_bucket, $"{id}.txt", text);

                Console.WriteLine($"[OCRWorker] Upload complete.");

            }
            finally
            {

                if (File.Exists(tempPdf)) File.Delete(tempPdf);
                Console.WriteLine($"[OCRWorker] Deleted temp file: {tempPdf} and finished OCR job for Document ID={id}");

            }
        }
    }
}
