namespace Paperless.Services.Messaging;

public interface IMessageProducer
{
    Task SendMessageAsync<T>(T message, string queueName);
}