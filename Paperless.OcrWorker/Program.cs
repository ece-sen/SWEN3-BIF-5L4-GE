using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("OCR Worker (PaperlessServices) started on port 8082 (logical).");
Console.WriteLine("Waiting for OCR jobs...");

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
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

    Console.WriteLine($"[OCR Worker] Received job for Document ID={message}");

    // Sprint 3: empty worker → only logs the job
    await Task.CompletedTask;
};

await channel.BasicConsumeAsync(
    queue: "OCR_QUEUE",
    autoAck: true,
    consumer: consumer
);

await Task.Delay(Timeout.Infinite);