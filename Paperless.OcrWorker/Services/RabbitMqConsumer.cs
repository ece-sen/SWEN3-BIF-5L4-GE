using Paperless.OcrWorker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class RabbitMqConsumer : IMessageConsumer
{
    private readonly IConnectionFactory _factory;
    private readonly string _queue;

    public RabbitMqConsumer(IConnectionFactory factory, string queue)
    {
        _factory = factory;
        _queue = queue;
    }

    public async void StartConsuming(Func<string, Task> onMessage)
    {
        var connection = await _factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(_queue, false, false, false, null);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            string id = Encoding.UTF8.GetString(ea.Body.ToArray());
            await onMessage(id);
        };

        await channel.BasicConsumeAsync(_queue, true, consumer);
    }
}
