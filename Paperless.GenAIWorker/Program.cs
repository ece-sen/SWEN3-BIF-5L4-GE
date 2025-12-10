using Paperless.GenAIWorker.Services;
using RabbitMQ.Client;

Console.WriteLine("GenAI Worker started…");

var factory = new ConnectionFactory
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
    UserName = "guest",
    Password = "guest"
};

var queueName = Environment.GetEnvironmentVariable("GENAI_QUEUE") ?? "genai_queue";

var consumer = new RabbitMqConsumer(factory, queueName);

var genAiService = new GenAIService();
var worker = new GenAIWorkerService(genAiService);

consumer.StartConsuming(async messageJson =>
{
    await worker.ProcessMessageAsync(messageJson);
});

await Task.Delay(-1);
