using CommonLibrary.Routes;
using MessagingLibrary.Interface;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessagingLibrary.Service
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly IConfiguration _configuration;

        public MessagePublisher(IConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _channel = _connection.CreateChannelAsync(new CreateChannelOptions(
                            publisherConfirmationsEnabled: true,
                            publisherConfirmationTrackingEnabled: true)).GetAwaiter().GetResult();
            _configuration = configuration;
        }

        public async Task PublishAsync<T>(T message, string eventType)
        {
            var routes = _configuration.GetSection("MessagingConfiguration:PublishingRoutes")
                                    .Get<Dictionary<string, PublishingRoutes>>();

            if (!routes.TryGetValue(eventType, out var route))
            {
                throw new Exception($"No route configured for event type: {eventType}");
            }

            var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await _channel.ExchangeDeclareAsync(route.Exchange, ExchangeType.Topic, true);
            await _channel.BasicPublishAsync(route.Exchange, route.RoutingKey, messageBody);
        }
    }
}
