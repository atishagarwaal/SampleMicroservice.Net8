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
            var routes = _configuration.GetSection("MessagingConfiguration:SubscriptionRoutes")
                                   .Get<Dictionary<string, SubscriptionRoutes>>();

            if (routes == null || routes.Count == 0)
            {
                throw new Exception("No subscription routes found in configuration.");
            }

            // Extract event name by removing "Handler" at the end
            string eventName = handler.Target.ToString().Substring(handler.Target.ToString().LastIndexOf('.') + 1)
                                           .Replace("Handler", "");

            if (!routes.TryGetValue(eventName, out var route))
            {
                throw new Exception($"No route configured for event type: {eventName}");
            }

            await _channel.QueueDeclareAsync(route.QueueName, true, false, false, null);
            await _channel.QueueBindAsync(route.QueueName, route.Exchange, route.RoutingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));
                await handler(message);
            };

            await _channel.BasicConsumeAsync(route.QueueName, true, consumer);
        }
    }
}
