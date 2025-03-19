using CloudinaryDotNet.Core;
using Domain.Entities;
using Interfracture.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.ServiceInterfaces;
using Services.Services.MessageQueue;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Services {
    public class RabbitMQConsumerService: BackgroundService, IRabbitMQConsumerService {
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly ConnectionFactory _factory;
        private IChannel _channel;
        private IConnection _connection;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRabbitMQService _rabbitMQService;
        private const string QueueName = "test_queue";

        public RabbitMQConsumerService(ConnectionFactory factory, ILogger<RabbitMQConsumerService> logger, IServiceProvider serviceProvider, IRabbitMQService rabbitMQService) {
            _factory = factory;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _rabbitMQService = rabbitMQService;
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

        public async Task ProcessMessageAsync(string message) {

            var orderMessage = JsonConvert.DeserializeObject<OrderMessage>(message);
            try {
                using (var scope = _serviceProvider.CreateScope()) {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    foreach (var drink in orderMessage.Drinks) {
                        var ingredientDetails = await GetDrinkDetailFromDatabaseAsync(drink.DrinkId, unitOfWork);

                        for (int i = 0; i < drink.Quantity; i++) {
                            var actions = new List<dynamic>();
                            int sequence = 1;

                            foreach (var ingredient in ingredientDetails) {
                                string ingName = ingredient.IngredientName.ToString().ToLower();

                                switch (ingName) {
                                    case string s when s.Contains("coffee beans"):
                                        actions.Add(new {
                                            action_id = "grind_coffee_beans",
                                            device = "coffee_grinder",
                                            type = "grinding",
                                            parameters = new {
                                                ingredient = ingredient.IngredientName,
                                                required_quantity = ingredient.RequiredQuantity
                                            },
                                            sequence = sequence++
                                        });

                                        actions.Add(new {
                                            action_id = "brew_coffee",
                                            device = "coffee_machine",
                                            type = "brewing",
                                            parameters = new {
                                                drink = drink.DrinkName,
                                                ingredient = ingredient.IngredientName
                                            },
                                            sequence = sequence++
                                        });
                                        break;

                                    case string s when s.Contains("milk"):

                                        actions.Add(new {
                                            action_id = "heat_milk",
                                            device = "milk_heater",
                                            type = "heating",
                                            parameters = new {
                                                ingredient = ingredient.IngredientName,
                                                required_quantity = ingredient.RequiredQuantity,
                                            },
                                            sequence = sequence++
                                        });

                                        actions.Add(new {
                                            action_id = "brew_milk",
                                            device = "milk_machine",
                                            type = "brewing",
                                            parameters = new {
                                                drink = drink.DrinkName,
                                                ingredient = ingredient.IngredientName
                                            },
                                            sequence = sequence++
                                        });
                                        break;

                                    case string s when s.Contains("sugar"):
                                        actions.Add(new {
                                            action_id = "add_sugar",
                                            device = "sugar_machine",
                                            type = "adding",
                                            parameters = new {
                                                ingredient = ingredient.IngredientName,
                                                required_quantity = ingredient.RequiredQuantity
                                            },
                                            sequence = sequence++
                                        });
                                        break;

                                    case string s when s.Contains("ice"):
                                        actions.Add(new {
                                            action_id = "add_ice",
                                            device = "ice_making_machine",
                                            type = "adding",
                                            parameters = new {
                                                ingredient = ingredient.IngredientName,
                                                required_quantity = ingredient.RequiredQuantity
                                            },
                                            sequence = sequence++
                                        });
                                        break;

                                    case string s when s.Contains("water"):
                                        actions.Add(new {
                                            action_id = "heat_water",
                                            device = "water_heater",
                                            type = "heating",
                                            parameters = new {
                                                ingredient = ingredient.IngredientName,
                                                required_quantity = ingredient.RequiredQuantity,
                                            },
                                            sequence = sequence++
                                        });

                                        actions.Add(new {
                                            action_id = "brew_water",
                                            device = "water_machine",
                                            type = "brewing",
                                            parameters = new {
                                                drink = drink.DrinkName,
                                                ingredient = ingredient.IngredientName
                                            },
                                            sequence = sequence++
                                        });
                                        break;

                                    default:
                                        break;
                                }
                            }

                            var machineMessage = new {
                                activity_id = $"machine_{drink.DrinkId}_{orderMessage.OrderId}_{i + 1}",
                                name = $"Pha {drink.DrinkName}",
                                description = $"Các bước để pha {drink.DrinkName} theo công thức.",
                                actions = actions
                            };

                            // Serialize message cho machine thành JSON (định dạng đẹp)
                            string machineMessageJson = JsonConvert.SerializeObject(machineMessage, Formatting.Indented);

                            // Đẩy message này lên message queue "machine_queue"
                            await _rabbitMQService.SendMessageAsync("machine_queue", machineMessageJson);
                        }
                    }
                }
            } catch (Exception ex) {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        }

        private async Task<List<dynamic>> GetDrinkDetailFromDatabaseAsync(Guid drinkId, IUnitOfWork unitOfWork) {

            var recipes = await unitOfWork.GetRepository<Recipe>()
                .Entities
                .Where(r => r.DrinkId == drinkId)
                .Include(r => r.Ingredient)
                .ToListAsync();

            var ingredientDetails = recipes
                .Where(r => r.Ingredient != null) // Ensure Ingredient is not null
                .Select(r => new {
                    IngredientId = r.Ingredient!.Id,
                    IngredientName = r.Ingredient.Name,
                    RequiredQuantity = r.Quantity
                }).ToList<dynamic>();

            return ingredientDetails;
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
