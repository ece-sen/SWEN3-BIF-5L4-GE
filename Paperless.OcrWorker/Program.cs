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

var settings = new ElasticsearchClientSettings(new Uri("http://elasticsearch:9200"))
    .DefaultIndex("documents")
    .CertificateFingerprint("")
    .ServerCertificateValidationCallback((obj, cert, chain, errors) => true)
    .RequestTimeout(TimeSpan.FromSeconds(30))
    .PrettyJson()
    .DisableDirectStreaming();  

var elastic = new ElasticsearchClient(settings);

try
{
    var doc = new { Id = "test", Content = "Hello ES" };

    var result = await elastic.IndexAsync(doc, i => i.Index("documents").Id("test"));

    Console.WriteLine($"[ES] Index success: {result.IsSuccess()}");
    Console.WriteLine(result.DebugInformation);
}
catch (Exception ex)
{
    Console.WriteLine("[ES] Index error:");
    Console.WriteLine(ex.ToString());
}


Console.WriteLine("OCR Worker started…");

consumer.StartConsuming(async documentId =>
{
    Console.WriteLine($"[WORKER] Received {documentId}, WAITING...");
    await worker.ProcessDocumentAsync(documentId);
});

await Task.Delay(-1);
