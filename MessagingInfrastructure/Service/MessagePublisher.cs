using CommonLibrary.Routes;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessagingLibrary.Service
{
    public class MessagePublisher : IMessagePublisher
    {
        private const string MessageCreationDateProperty = "CreationDate";

        private readonly IConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessagePublisher>? _logger;

        public MessagePublisher(IConnection connection, IConfiguration configuration, ILogger<MessagePublisher>? logger = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task PublishAsync<T>(T message, string eventType)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var routes = _configuration.GetSection("MessagingConfiguration:PublishingRoutes")
                                    .Get<Dictionary<string, PublishingRoutes>>();

            if (routes == null || !routes.TryGetValue(eventType, out var route))
            {
                throw new InvalidOperationException($"No route configured for event type: {eventType}");
            }

            // Set CreationDate property on the message object if it exists (like other repos)
            SetMessageMetadata(message);

            var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            
            // Channel-per-operation pattern: create a new channel for each publish operation
            // This provides better thread safety and resource management
            var channel = await _connection.CreateChannelAsync(new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true));
            
            try
            {
                // Ensure exchange exists
                await channel.ExchangeDeclareAsync(route.Exchange, ExchangeType.Topic, true, false, null);
                
                // Create properties with standard headers
                var properties = new BasicProperties();
                properties.Persistent = true; // Ensure message persistence
                properties.Headers = CreateMessageHeaders(eventType, route.RoutingKey);
                
                // Publish message
                await channel.BasicPublishAsync(
                    exchange: route.Exchange, 
                    routingKey: route.RoutingKey, 
                    mandatory: true, // Enable mandatory flag for better error handling
                    basicProperties: properties, 
                    body: messageBody);
                
                _logger?.LogInformation("Message published successfully: EventType={EventType} to {Exchange} with routing key {RoutingKey}", 
                    eventType, route.Exchange, route.RoutingKey);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to publish message for event type: {EventType}", eventType);
                throw;
            }
            finally
            {
                // Dispose channel after operation completes
                channel?.Dispose();
            }
        }

        /// <summary>
        /// Sets CreationDate property on the message object if it exists.
        /// This matches the pattern used in other repositories where metadata is set directly on the contract.
        /// Note: Id is not set automatically as it's typically domain-specific data (not metadata).
        /// </summary>
        private void SetMessageMetadata<T>(T message)
        {
            if (message == null)
            {
                return;
            }

            var messageType = message.GetType();
            
            // Set CreationDate property if it exists
            var creationDateProperty = messageType.GetProperty(MessageCreationDateProperty, BindingFlags.Public | BindingFlags.Instance);
            if (creationDateProperty != null && creationDateProperty.CanWrite)
            {
                creationDateProperty.SetValue(message, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Creates standard message headers for RabbitMQ.
        /// Headers are stored separately from the message body, following best practices for microservices:
        /// - X-Message-Type: Identifies the message contract type
        /// - X-Message-Version: Enables contract evolution and backward compatibility
        /// - X-Routing-Key: Useful for debugging and logging (mirrors RabbitMQ routing key)
        /// - X-Timestamp: Enables message flow tracking and latency diagnosis
        /// - X-Correlation-Id: Essential for distributed tracing across microservices
        /// - Content-Type: Specifies message body format
        /// </summary>
        private IDictionary<string, object?> CreateMessageHeaders(string eventType, string routingKey)
        {
            var headers = new Dictionary<string, object?>
            {
                [RabbitmqConstants.MessageTypeHeader] = eventType,
                [RabbitmqConstants.MessageVersionHeader] = RabbitmqConstants.DefaultMessageVersion,
                [RabbitmqConstants.RoutingKeyHeader] = routingKey,
                [RabbitmqConstants.ContentTypeHeader] = RabbitmqConstants.DefaultContentType,
                [RabbitmqConstants.TimestampHeader] = DateTime.UtcNow.ToString("O"),
                [RabbitmqConstants.CorrelationIdHeader] = Guid.NewGuid().ToString()
            };

            return headers;
        }
    }
}
