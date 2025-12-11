using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Paperless.Models;

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
               
                await PublishToGenAiQueueAsync(id, text);

            }
            finally
            {

                if (File.Exists(tempPdf)) File.Delete(tempPdf);
                Console.WriteLine($"[OCRWorker] Deleted temp file: {tempPdf} and finished OCR job for Document ID={id}");

            }
        }

        private async Task PublishToGenAiQueueAsync(string documentId, string text)
        {
            try
            {
                string rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq";
                string queueName = Environment.GetEnvironmentVariable("GENAI_QUEUE") ?? "genai_queue";

                var factory = new ConnectionFactory
                {
                    HostName = rabbitHost,
                    UserName = "guest",
                    Password = "guest"
                };

                await using var connection = await factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queueName, durable: false, exclusive: false, autoDelete: false,
                    arguments: null);

                var msg = new OcrCompletedMessage
                {
                    DocumentId = documentId,
                    Text = text
                };

                var json = JsonSerializer.Serialize(msg);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: queueName,
                    mandatory: false,
                    body: body);

                Console.WriteLine(
                    $"[OCRWorker] Published OCR-completed message for document {documentId} to GenAI queue '{queueName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OCRWorker] ERROR while publishing to GenAI queue: {ex}");
            }
        }
    }
}
