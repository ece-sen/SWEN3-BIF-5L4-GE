using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Paperless.Services.Messaging;

public class RabbitMqProducer : IMessageProducer
{
    private readonly ILogger<RabbitMqProducer> _logger;
    private readonly ConnectionFactory _factory;

    public RabbitMqProducer(IConfiguration config, ILogger<RabbitMqProducer> logger)
    {
        _logger = logger;

        _factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "rabbitmq",
            UserName = config["RabbitMQ:User"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest",
        };
    }

    public async Task SendMessageAsync<T>(T message, string queueName)
    {
        try
        {
            await using var connection = await _factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                body: body
            );

            _logger.LogInformation("Sent message to {Queue}: {Message}", queueName, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to queue {Queue}", queueName);
            throw;
        }
    }
}