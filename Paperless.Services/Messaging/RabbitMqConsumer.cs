using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Paperless.Services.Messaging;

public class RabbitMqConsumer
{
    private readonly ConnectionFactory _factory;

    public RabbitMqConsumer(IConfiguration config)
    {
        _factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "rabbitmq",
            UserName = config["RabbitMQ:User"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest"
        };
    }

    public async Task ListenAsync(string queueName, Func<string, Task> onMessage, CancellationToken token)
    {
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: queueName,
            durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea, _) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            await onMessage(message);
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
        Console.WriteLine($"✅ Listening on '{queueName}'...");
        await Task.Delay(-1, token);
    }
}