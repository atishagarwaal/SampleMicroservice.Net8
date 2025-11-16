using CommonLibrary.Routes;
using MessagingLibrary.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    /// <summary>
    /// Message subscriber class for subscribing to messages from RabbitMQ.
    /// </summary>
    public class MessageSubscriber : IMessageSubscriber
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageSubscriber> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSubscriber"/> class.
        /// </summary>
        /// <param name="connection">RabbitMQ connection.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="logger">Instance of logger.</param>
        public MessageSubscriber(
            IConnection connection,
            IConfiguration configuration,
            ILogger<MessageSubscriber> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _logger.LogDebug("Creating channel for message subscriber");
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _logger.LogDebug("Channel created successfully");
        }

        /// <summary>
        /// Subscribes to messages of type T from RabbitMQ.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="handler">Handler function to process messages.</param>
        public async Task SubscribeAsync<T>(Func<T, Task> handler)
        {
            if (handler == null)
            {
                _logger.LogWarning("Attempted to subscribe with null handler for message type {MessageType}", typeof(T).Name);
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null.");
            }

            _logger.LogInformation("Setting up subscription for message type {MessageType}", typeof(T).Name);

            var routes = _configuration.GetSection("MessagingConfiguration:SubscriptionRoutes")
                                   .Get<Dictionary<string, SubscriptionRoutes>>();

            if (routes == null || routes.Count == 0)
            {
                _logger.LogError("SubscriptionRoutes configuration section is null or empty");
                throw new InvalidOperationException("SubscriptionRoutes configuration section is missing or empty");
            }

            string eventName = typeof(T).Name;
            if (eventName.EndsWith("Event", StringComparison.OrdinalIgnoreCase))
            {
                eventName = eventName.Substring(0, eventName.Length - "Event".Length);
            }

            _logger.LogInformation("Subscribing to event: {EventName} (from type {TypeName})", eventName, typeof(T).Name);

            if (!routes.TryGetValue(eventName, out var route))
            {
                var availableRoutes = string.Join(", ", routes.Keys);
                _logger.LogError("No route configured for event type: {EventName}. Available routes: {AvailableRoutes}", 
                    eventName, availableRoutes);
                throw new InvalidOperationException(
                    $"No route configured for event type: {eventName}. Available routes: {availableRoutes}");
            }

            _logger.LogInformation("Found route for {EventName}: Queue={QueueName}, Exchange={Exchange}, RoutingKey={RoutingKey}", 
                eventName, route.QueueName, route.Exchange, route.RoutingKey);

            // Clean up existing queue only (don't delete exchange as other services may be using it)
            await CleanupExistingQueue(route);

            // Build queue arguments based on configuration
            var queueArguments = new Dictionary<string, object>();
            
            if (!string.IsNullOrEmpty(route.DeadLetterExchange))
            {
                queueArguments["x-dead-letter-exchange"] = route.DeadLetterExchange;
            }
            
            if (!string.IsNullOrEmpty(route.DeadLetterRoutingKey))
            {
                queueArguments["x-dead-letter-routing-key"] = route.DeadLetterRoutingKey;
            }
            
            if (route.MessageTTL.HasValue)
            {
                queueArguments["x-message-ttl"] = route.MessageTTL.Value;
            }
            
            if (route.MaxLength.HasValue)
            {
                queueArguments["x-max-length"] = route.MaxLength.Value;
            }
            
            if (route.EnablePriority == true && route.MaxPriority.HasValue)
            {
                queueArguments["x-max-priority"] = route.MaxPriority.Value;
            }

            _logger.LogInformation("Declaring exchange: {Exchange}", route.Exchange);
            await _channel.ExchangeDeclareAsync(
                exchange: route.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null);
            _logger.LogInformation("Exchange declared: {Exchange}", route.Exchange);

            _logger.LogInformation("Declaring queue: {QueueName}", route.QueueName);
            await _channel.QueueDeclareAsync(
                queue: route.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments);
            _logger.LogInformation("Queue declared: {QueueName}", route.QueueName);

            _logger.LogInformation("Binding queue {QueueName} to exchange {Exchange} with routing key {RoutingKey}", 
                route.QueueName, route.Exchange, route.RoutingKey);
            await _channel.QueueBindAsync(queue: route.QueueName,
                             exchange: route.Exchange,
                             routingKey: route.RoutingKey).ConfigureAwait(false);
            _logger.LogInformation("Queue bound successfully: {QueueName} -> {Exchange} ({RoutingKey})", 
                route.QueueName, route.Exchange, route.RoutingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    _logger.LogInformation("Received message. DeliveryTag={DeliveryTag}, Exchange={Exchange}, RoutingKey={RoutingKey}", 
                        ea.DeliveryTag, ea.Exchange, ea.RoutingKey);

                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    _logger.LogDebug("Message body received. Size: {Size} bytes, DeliveryTag: {DeliveryTag}", 
                        body.Length, ea.DeliveryTag);
                    
                    var message = JsonSerializer.Deserialize<T>(messageJson);

                    if (message == null)
                    {
                        _logger.LogError("Failed to deserialize message. DeliveryTag: {DeliveryTag}, Body: {MessageBody}", 
                            ea.DeliveryTag, messageJson);
                        throw new Exception("Failed to deserialize message");
                    }

                    _logger.LogInformation("Successfully deserialized message. MessageType={MessageType}, DeliveryTag={DeliveryTag}", 
                        typeof(T).Name, ea.DeliveryTag);

                    if (ea.BasicProperties.Headers != null)
                    {
                        foreach (var header in ea.BasicProperties.Headers)
                        {
                            _logger.LogDebug("Message header. Key={Key}, Value={Value}, DeliveryTag={DeliveryTag}", 
                                header.Key, header.Value, ea.DeliveryTag);
                        }
                    }

                    _logger.LogInformation("Calling handler for message. MessageType={MessageType}, DeliveryTag={DeliveryTag}", 
                        typeof(T).Name, ea.DeliveryTag);
                    await handler(message).ConfigureAwait(false);

                    _logger.LogInformation("Message processed successfully. DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message. DeliveryTag={DeliveryTag}, Exchange={Exchange}, RoutingKey={RoutingKey}", 
                        ea.DeliveryTag, ea.Exchange, ea.RoutingKey);

                    var errorHeaders = new Dictionary<string, object>
                    {
                        ["X-Error-Message"] = ex.Message,
                        ["X-Error-Type"] = ex.GetType().Name,
                        ["X-Error-Timestamp"] = DateTime.UtcNow.ToString("O"),
                        ["X-Original-Exchange"] = ea.Exchange,
                        ["X-Original-Routing-Key"] = ea.RoutingKey
                    };

                    await PublishToDeadLetterExchange(ea, errorHeaders);
                    
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false).ConfigureAwait(false);
                    return;
                }
                finally
                {
                    try
                    {
                        await _channel.BasicAckAsync(ea.DeliveryTag, false).ConfigureAwait(false);
                    }
                    catch (Exception ackEx)
                    {
                        _logger.LogError(ackEx, "Failed to acknowledge message. DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                    }
                }
            };

            await _channel.BasicConsumeAsync(queue: route.QueueName,
                                            autoAck: false,
                                            consumer: consumer).ConfigureAwait(false);

            _logger.LogInformation("Subscribed to queue. QueueName={QueueName}, Exchange={Exchange}, RoutingKey={RoutingKey}", 
                route.QueueName, route.Exchange, route.RoutingKey);
        }

        /// <summary>
        /// Publishes a failed message to the dead letter exchange.
        /// </summary>
        /// <param name="ea">Basic deliver event arguments.</param>
        /// <param name="errorHeaders">Error headers to include.</param>
        private async Task PublishToDeadLetterExchange(BasicDeliverEventArgs ea, Dictionary<string, object> errorHeaders)
        {
            _logger.LogInformation("Publishing failed message to dead letter exchange. DeliveryTag={DeliveryTag}", ea.DeliveryTag);
            
            try
            {
                var deadLetterExchange = "dlx.topic.exchange";
                var deadLetterRoutingKey = "failure";

                _logger.LogDebug("Declaring dead letter exchange: {DeadLetterExchange}", deadLetterExchange);
                await _channel.ExchangeDeclareAsync(
                    exchange: deadLetterExchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                var errorMessage = new
                {
                    OriginalMessage = Encoding.UTF8.GetString(ea.Body.ToArray()),
                    ErrorDetails = errorHeaders,
                    OriginalProperties = new
                    {
                        Exchange = ea.Exchange,
                        RoutingKey = ea.RoutingKey,
                        DeliveryTag = ea.DeliveryTag,
                        Timestamp = DateTime.UtcNow
                    }
                };

                var errorBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorMessage));
                var properties = new BasicProperties();
                properties.Persistent = true;
                properties.Headers = errorHeaders;

                _logger.LogDebug("Publishing to dead letter exchange. Exchange={DeadLetterExchange}, RoutingKey={DeadLetterRoutingKey}, DeliveryTag={DeliveryTag}", 
                    deadLetterExchange, deadLetterRoutingKey, ea.DeliveryTag);

                await _channel.BasicPublishAsync(
                    exchange: deadLetterExchange,
                    routingKey: deadLetterRoutingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: errorBody);

                _logger.LogInformation("Message sent to dead letter exchange successfully. DeliveryTag={DeliveryTag}", ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to dead letter exchange. DeliveryTag={DeliveryTag}", ea.DeliveryTag);
            }
        }

        /// <summary>
        /// Disposes the message subscriber and closes the channel.
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("Disposing message subscriber and closing channel");
            _channel?.Dispose();
        }

        /// <summary>
        /// Cleans up existing queue before creating a new subscription.
        /// </summary>
        /// <param name="route">Subscription route configuration.</param>
        private async Task CleanupExistingQueue(SubscriptionRoutes route)
        {
            _logger.LogInformation("Cleaning up existing queue: {QueueName}", route.QueueName);

            try
            {
                try
                {
                    await _channel.QueueDeleteAsync(route.QueueName, false, false);
                    _logger.LogInformation("Deleted existing queue: {QueueName}", route.QueueName);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Queue {QueueName} does not exist or could not be deleted: {Message}", 
                        route.QueueName, ex.Message);
                }

                _logger.LogInformation("Queue cleanup completed for: {QueueName}", route.QueueName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during cleanup of existing queue: {QueueName}", route.QueueName);
            }
        }
    }
}
