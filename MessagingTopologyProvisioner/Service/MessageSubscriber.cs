using CommonLibrary.Routes;
using MessagingLibrary.Interface;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessagingLibrary.Service
{
    public class MessageSubscriber : IMessageSubscriber
    {
        private static IConnection _connection;
        private static IChannel _channel;
        private static readonly object _lock = new();
        private readonly IConfiguration _configuration;

        public MessageSubscriber(IConnection connection, IConfiguration configuration)
        {
            if (_connection == null)
            {
                lock (_lock)
                {
                    if (_connection == null)
                    {
                        _connection = connection;
                        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                    }
                }
            }
            _configuration = configuration;
        }

        public async Task SubscribeAsync<T>(Func<T, Task> handler)
        {
            // Validate handler is not null
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null.");
            }

            var routes = _configuration.GetSection("MessagingConfiguration:SubscriptionRoutes")
                                   .Get<Dictionary<string, SubscriptionRoutes>>();

            if (routes == null || routes.Count == 0)
            {
                throw new Exception("No subscription routes found in configuration.");
            }

            // Extract event name by removing "Handler" at the end
            string eventName = handler.Target.ToString()
                                .Substring(handler.Target.ToString()
                                .LastIndexOf('.') + 1)
                                .Replace("EventHandler", "");

            if (!routes.TryGetValue(eventName, out var route))
            {
                throw new Exception($"No route configured for event type: {eventName}");
            }

            await _channel.QueueBindAsync(queue: route.QueueName,
                             exchange: route.Exchange,
                             routingKey: route.RoutingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));

                    try
                    {
                        await handler(message);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");

                        // Requeue for immediate re-processing
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");

                    // Send to DLX queue
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            // Disable auto acknowledgment
            await _channel.BasicConsumeAsync(queue: route.QueueName,
                                            autoAck: false,
                                            consumer: consumer);
        }
    }
}
