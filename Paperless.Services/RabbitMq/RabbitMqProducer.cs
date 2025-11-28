using System.Text;
using Microsoft.Extensions.Options;
using Paperless.Services.RabbitMq;
using RabbitMQ.Client;

namespace Paperless.Services;

public class RabbitMqProducer : IRabbitMqProducer
{
    private readonly RabbitMqSettings _settings;

    public RabbitMqProducer(IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendMessageAsync(string message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.User,
            Password = _settings.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var queue = _settings.QueueName;

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var bytes = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queue,
            mandatory: false,
            body: bytes
        );

        Console.WriteLine($"[REST] Sent OCR job for Document {message}");
    }
}