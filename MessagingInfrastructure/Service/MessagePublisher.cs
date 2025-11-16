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
    /// <summary>
    /// Message publisher class for publishing messages to RabbitMQ.
    /// </summary>
    public class MessagePublisher : IMessagePublisher
    {
        private const string MessageCreationDateProperty = "CreationDate";

        private readonly IConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessagePublisher> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePublisher"/> class.
        /// </summary>
        /// <param name="connection">RabbitMQ connection.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="logger">Instance of logger.</param>
        public MessagePublisher(
            IConnection connection,
            IConfiguration configuration,
            ILogger<MessagePublisher> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Publishes a message to RabbitMQ.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">Message to publish.</param>
        /// <param name="eventType">Event type identifier.</param>
        public async Task PublishAsync<T>(T message, string eventType)
        {
            if (message == null)
            {
                _logger.LogWarning("Attempted to publish null message for event type {EventType}", eventType);
                throw new ArgumentNullException(nameof(message));
            }

            _logger.LogInformation("Publishing message. EventType: {EventType}, MessageType: {MessageType}", 
                eventType, typeof(T).Name);

            var routes = _configuration.GetSection("MessagingConfiguration:PublishingRoutes")
                                    .Get<Dictionary<string, PublishingRoutes>>();

            if (routes == null)
            {
                _logger.LogError("PublishingRoutes configuration section is null or empty");
                throw new InvalidOperationException("PublishingRoutes configuration section is missing or empty");
            }

            if (!routes.TryGetValue(eventType, out var route))
            {
                var availableRoutes = string.Join(", ", routes.Keys);
                _logger.LogError("No route configured for event type: {EventType}. Available routes: {AvailableRoutes}", 
                    eventType, availableRoutes);
                throw new InvalidOperationException(
                    $"No route configured for event type: {eventType}. Available routes: {availableRoutes}");
            }

            _logger.LogDebug("Found route for {EventType}: Exchange={Exchange}, RoutingKey={RoutingKey}", 
                eventType, route.Exchange, route.RoutingKey);

            SetMessageMetadata(message);

            var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            _logger.LogDebug("Serialized message body. Size: {Size} bytes", messageBody.Length);
            
            var channel = await _connection.CreateChannelAsync(new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true));
            
            try
            {
                _logger.LogDebug("Declaring exchange: {Exchange}", route.Exchange);
                await channel.ExchangeDeclareAsync(route.Exchange, ExchangeType.Topic, true, false, null);
                
                var properties = new BasicProperties();
                properties.Persistent = true;
                properties.Headers = CreateMessageHeaders(eventType, route.RoutingKey);
                
                _logger.LogDebug("Publishing message to Exchange={Exchange}, RoutingKey={RoutingKey}", 
                    route.Exchange, route.RoutingKey);
                
                await channel.BasicPublishAsync(
                    exchange: route.Exchange, 
                    routingKey: route.RoutingKey, 
                    mandatory: true,
                    basicProperties: properties, 
                    body: messageBody);
                
                _logger.LogInformation("Message published successfully. EventType={EventType}, Exchange={Exchange}, RoutingKey={RoutingKey}", 
                    eventType, route.Exchange, route.RoutingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to publish message. EventType={EventType}, Exchange={Exchange}, RoutingKey={RoutingKey}, MessageType={MessageType}", 
                    eventType, route?.Exchange ?? "unknown", route?.RoutingKey ?? "unknown", typeof(T).Name);
                throw;
            }
            finally
            {
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
        /// - X-Service-Name: Identifies the originating service for distributed tracing
        /// - X-Timestamp: Enables message flow tracking and latency diagnosis
        /// - X-Correlation-Id: Essential for distributed tracing across microservices
        /// - Content-Type: Specifies message body format
        /// </summary>
        private IDictionary<string, object?> CreateMessageHeaders(string eventType, string routingKey)
        {
            // Get service name from configuration or use assembly name as fallback
            var serviceName = _configuration["ServiceName"] 
                ?? Assembly.GetExecutingAssembly().GetName().Name 
                ?? "Unknown";

            var headers = new Dictionary<string, object?>
            {
                [RabbitmqConstants.MessageTypeHeader] = eventType,
                [RabbitmqConstants.MessageVersionHeader] = RabbitmqConstants.DefaultMessageVersion,
                [RabbitmqConstants.RoutingKeyHeader] = routingKey,
                [RabbitmqConstants.ServiceNameHeader] = serviceName,
                [RabbitmqConstants.ContentTypeHeader] = RabbitmqConstants.DefaultContentType,
                [RabbitmqConstants.TimestampHeader] = DateTime.UtcNow.ToString("O"),
                [RabbitmqConstants.CorrelationIdHeader] = Guid.NewGuid().ToString()
            };

            return headers;
        }
    }
}
