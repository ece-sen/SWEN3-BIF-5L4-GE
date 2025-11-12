namespace Paperless.Services.Messaging;

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken token);
}