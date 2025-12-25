using System.Text;
using Minio;
using Paperless.OcrWorker.Services;
using RabbitMQ.Client;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

var minio = new MinioClient()
    .WithEndpoint("minio:9000")
    .WithCredentials("minioadmin", "minioadmin")
    .WithSSL(false)
    .Build();

var bucket = Environment.GetEnvironmentVariable("MINIO_BUCKET") ?? "documents";
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
var indexingQueueName = Environment.GetEnvironmentVariable("INDEXING_QUEUE") ?? "indexing_queue";

var consumer = new RabbitMqConsumer(factory, queueName);
var indexingConsumer = new RabbitMqConsumer(factory, indexingQueueName);

var worker = new OcrWorker(storage, ocrService, bucket);

var elasticUrl = Environment.GetEnvironmentVariable("ELASTIC_URL") ?? "http://elasticsearch:9200";
IElasticsearchIndexingService elastic = new ElasticsearchIndexingService(elasticUrl);

var indexingWorker = new IndexingWorker(storage, elastic, bucket);

Console.WriteLine("OCR Worker started…");

consumer.StartConsuming(async documentId =>
{
    try
    {
        Console.WriteLine($"[WORKER] Received {documentId}, WAITING...");
        await worker.ProcessDocumentAsync(documentId);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WORKER] ERROR: {ex}");
    }

});

Console.WriteLine($"Indexing Worker started… consuming '{indexingQueueName}'");
indexingConsumer.StartConsuming(async documentId =>
{
    try
    {
        Console.WriteLine($"[INDEX] Received {documentId}");
        await indexingWorker.ProcessAsync(documentId);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[INDEX] ERROR: {ex}");
    }
});

await Task.Delay(-1);
