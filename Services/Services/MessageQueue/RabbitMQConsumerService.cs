using CloudinaryDotNet.Core;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.ServiceInterfaces;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Services {
    public class RabbitMQConsumerService: BackgroundService, IRabbitMQConsumerService {
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly ConnectionFactory _factory;
        private IChannel _channel;
        private IConnection _connection;
        private const string QueueName = "order_queue";

        public RabbitMQConsumerService(ConnectionFactory factory, ILogger<RabbitMQConsumerService> logger, IConnection connection, IChannel channel) {
            _factory = factory;
            _logger = logger;
            _channel = channel;
            _connection = connection;
        }

        public async Task StartConsumingAsync(string queueName) {
            try {
                _connection = await _factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) => {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($" [x] Received: {message}");

                    try {
                        await ProcessMessageAsync(message);
                    } catch (Exception ex) {
                        _logger.LogError($"Error processing message: {ex.Message}");
                    }

                    // Xác nhận tin nhắn đã xử lý
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                };

                await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

                _logger.LogInformation($"[*] Listening on queue: {queueName}");
            } catch (Exception ex) {
                _logger.LogError($"Error consuming messages from {queueName}: {ex.Message}");
            }
        }

        private Task ProcessMessageAsync(string message) {
            _logger.LogInformation($"Processing: {message}");
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            await StartConsumingAsync(QueueName);
        }

        public void Dispose() {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
            base.Dispose();
        }
    }
}
