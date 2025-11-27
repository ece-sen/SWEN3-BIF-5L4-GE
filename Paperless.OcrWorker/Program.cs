using Minio;
using Minio.DataModel.Args;
using Paperless.OcrWorker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("OCR Worker (PaperlessServices) started on port 8082.");
Console.WriteLine("Waiting for OCR jobs...");

// MinIO client
var minio = new MinioClient()
    .WithEndpoint("minio:9000")
    .WithCredentials("minioadmin", "minioadmin")
    .Build();

string bucket = "paperless";

// OCR service
var ocr = new OcrService();

// RabbitMQ
var factory = new ConnectionFactory
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
    UserName = "guest",
    Password = "guest"
};

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "OCR_QUEUE",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var id = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.WriteLine($"[OCR Worker] Received job for Document ID={id}");

    string pdfName = $"{id}.pdf";
    string txtName = $"{id}.txt";

    string tempPath = Path.GetTempFileName();

    // PDF aus MinIO herunterladen
    Console.WriteLine($"[Worker] Downloading {pdfName}...");
    await minio.GetObjectAsync(
        new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(pdfName)
            .WithFile(tempPath)
    );

    // OCR ausführen
    Console.WriteLine("[Worker] Running OCR...");
    string text = ocr.ExtractTextFromPdf(tempPath);

    byte[] bytes = Encoding.UTF8.GetBytes(text);

    // OCR-Ergebnis speichern
    Console.WriteLine($"[Worker] Uploading {txtName}...");
    await minio.PutObjectAsync(
        new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(txtName)
            .WithStreamData(new MemoryStream(bytes))
            .WithObjectSize(bytes.Length)
            .WithContentType("text/plain")
    );

    Console.WriteLine($"[Worker] OCR completed for Document {id}");
};

await channel.BasicConsumeAsync(
    queue: "OCR_QUEUE",
    autoAck: true,
    consumer: consumer
);

await Task.Delay(Timeout.Infinite);
