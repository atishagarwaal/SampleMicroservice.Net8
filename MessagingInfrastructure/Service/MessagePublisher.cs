using CommonLibrary.Routes;
using MessagingLibrary.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<MessagePublisher>? _logger;

        public MessagePublisher(IConnection connection, IConfiguration configuration, ILogger<MessagePublisher>? logger = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
            
            _channel = _connection.CreateChannelAsync(new CreateChannelOptions(
                            publisherConfirmationsEnabled: true,
                            publisherConfirmationTrackingEnabled: true)).GetAwaiter().GetResult();
        }

        public async Task PublishAsync<T>(T message, string eventType)
        {
            try
            {
                var routes = _configuration.GetSection("MessagingConfiguration:PublishingRoutes")
                                        .Get<Dictionary<string, PublishingRoutes>>();

                if (routes == null || !routes.TryGetValue(eventType, out var route))
                {
                    throw new Exception($"No route configured for event type: {eventType}");
                }

                var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                
                // Ensure exchange exists
                await _channel.ExchangeDeclareAsync(route.Exchange, ExchangeType.Topic, true, false, null);
                
                // Create enhanced properties with standard headers
                var properties = new BasicProperties();
                properties.Persistent = true; // Ensure message persistence
                properties.Headers = CreateMessageHeaders(message, eventType);
                
                // Publish with enhanced properties
                await _channel.BasicPublishAsync(
                    exchange: route.Exchange, 
                    routingKey: route.RoutingKey, 
                    mandatory: true, // Enable mandatory flag for better error handling
                    basicProperties: properties, 
                    body: messageBody);
                
                _logger?.LogInformation("Message published successfully: {EventType} to {Exchange} with routing key {RoutingKey}", 
                    eventType, route.Exchange, route.RoutingKey);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to publish message for event type: {EventType}", eventType);
                throw;
            }
        }

        private IDictionary<string, object> CreateMessageHeaders<T>(T message, string eventType)
        {
            var headers = new Dictionary<string, object>
            {
                ["X-Message-Type"] = eventType,
                ["X-Message-Version"] = "1.0",
                ["X-Routing-Key"] = eventType,
                ["Content-Type"] = "application/json",
                ["X-Timestamp"] = DateTime.UtcNow.ToString("O"),
                ["X-Correlation-Id"] = Guid.NewGuid().ToString()
            };

            // Add message-specific headers if message implements IMessage
            if (message is CommonLibrary.MessageContract.IMessage messageWithHeaders)
            {
                // Use the enhanced headers from MessageBase
                foreach (var header in messageWithHeaders.Headers)
                {
                    if (!headers.ContainsKey(header.Key))
                    {
                        headers[header.Key] = header.Value;
                    }
                }
                
                // Add priority and TTL if the message has enhanced properties
                if (message is CommonLibrary.MessageContract.MessageBase enhancedMessage)
                {
                    if (enhancedMessage.Priority > 0)
                    {
                        headers["X-Priority"] = enhancedMessage.Priority.ToString();
                    }
                    if (enhancedMessage.TTL > 0)
                    {
                        headers["X-TTL"] = enhancedMessage.TTL.ToString();
                    }
                    if (!string.IsNullOrEmpty(enhancedMessage.BusinessContext))
                    {
                        headers["X-Business-Context"] = enhancedMessage.BusinessContext;
                    }
                }
            }

            return headers;
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
