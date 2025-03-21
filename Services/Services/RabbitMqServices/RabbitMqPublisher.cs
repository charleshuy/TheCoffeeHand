using System.Text;
using RabbitMQ.Client;

namespace Services.Services.RabbitMqServices
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly Task<IModel> _channelTask;
        private bool _disposed;

        public RabbitMqPublisher(Task<IModel> channelTask)
        {
            _channelTask = channelTask ?? throw new ArgumentNullException(nameof(channelTask));
            InitializeQueueAsync().GetAwaiter().GetResult(); // Blocking call in constructor
        }

        private async Task InitializeQueueAsync()
        {
            var channel = await _channelTask;
            channel.QueueDeclare(
                queue: "TheCoffeeHandQueue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public async Task PublishAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            var channel = await _channelTask;
            var body = Encoding.UTF8.GetBytes(message);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: "",
                routingKey: "TheCoffeeHandQueue",
                basicProperties: properties,
                body: body);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // Note: Do not dispose the channel here; it's managed by DI
            }
            _disposed = true;
        }
    }
}