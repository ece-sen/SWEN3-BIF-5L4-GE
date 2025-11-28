using System.Text;
using Minio;
using Paperless.OcrWorker.Services;
using RabbitMQ.Client;

var minio = new MinioClient()
    .WithEndpoint("minio:9000")
    .WithCredentials("minioadmin", "minioadmin")
    .WithSSL(false)
    .Build();

var process = new ProcessRunner();
var fs = new FileSystem();
var ocrService = new OcrService(process, fs);

var storage = new MinioStorageService(minio);

var factory = new ConnectionFactory
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
    UserName = "guest",
    Password = "guest"
};
var queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE") ?? "ocr_queue";

var consumer = new RabbitMqConsumer(factory, queueName);

var worker = new OcrWorker(storage, ocrService, "documents");

Console.WriteLine("OCR Worker started…");

consumer.StartConsuming(async documentId =>
{
    Console.WriteLine($"[WORKER] Received {documentId}, WAITING...");
    await worker.ProcessDocumentAsync(documentId);
});

await Task.Delay(-1);
