using System.Text;
using Minio;
using Minio.DataModel.Args;
using Paperless.OcrWorker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("OCR Worker started…");

// Build MinIO client
var minio = new MinioClient()
    .WithEndpoint("minio:9000")
    .WithCredentials("minioadmin", "minioadmin")
    .WithSSL(false)
    .Build();

string bucket = "documents";

var process = new ProcessRunner();
var fs = new FileSystem();
var ocr = new OcrService(process, fs);

// RabbitMQ connection
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

Console.WriteLine("Waiting for OCR jobs…");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var id = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.WriteLine($"Received OCR job for Document ID={id}");

    string pdfName = $"{id}.pdf";
    string txtName = $"{id}.txt";

    string tempPdf = Path.Combine(Path.GetTempPath(), $"{id}.pdf");
    string tempTxt = Path.Combine(Path.GetTempPath(), $"{id}.txt");

    try
    {
        // 1. Download file from MinIO
        Console.WriteLine($"Downloading {pdfName}...");
        await minio.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(bucket)
                .WithObject(pdfName)
                .WithFile(tempPdf)
        );

        // 2. OCR extract
        Console.WriteLine("Running OCR...");
        string text = ocr.ExtractTextFromPdf(tempPdf);

        File.WriteAllText(tempTxt, text);
        var bytes = Encoding.UTF8.GetBytes(text);

        // 3. Upload result back to MinIO
        Console.WriteLine($"Uploading {txtName}...");
        await minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(txtName)
                .WithStreamData(new MemoryStream(bytes))
                .WithObjectSize(bytes.Length)
                .WithContentType("text/plain")
        );

        Console.WriteLine($"OCR completed for Document {id}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR processing {id}: {ex.Message}");
    }
    finally
    {
        // Cleanup
        if (File.Exists(tempPdf)) File.Delete(tempPdf);
        if (File.Exists(tempTxt)) File.Delete(tempTxt);
    }
};

await channel.BasicConsumeAsync("OCR_QUEUE", true, consumer);

await Task.Delay(-1);
