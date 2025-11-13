namespace Paperless.Services.RabbitMq;

public interface IRabbitMqProducer
{
    Task SendMessageAsync(string message);
}