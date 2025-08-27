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
    public class MessageSubscriber : IMessageSubscriber
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageSubscriber>? _logger;

        public MessageSubscriber(IConnection connection, IConfiguration configuration, ILogger<MessageSubscriber>? logger = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
            
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
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

            // Ensure queue and exchange exist
            await _channel.QueueDeclareAsync(
                queue: route.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = "dlx.topic.exchange",
                    ["x-dead-letter-routing-key"] = "failure"
                });

            await _channel.ExchangeDeclareAsync(
                exchange: route.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            await _channel.QueueBindAsync(queue: route.QueueName,
                             exchange: route.Exchange,
                             routingKey: route.RoutingKey).ConfigureAwait(false);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    _logger?.LogDebug("Processing message: {DeliveryTag} from {Exchange} with routing key {RoutingKey}", 
                        ea.DeliveryTag, ea.Exchange, ea.RoutingKey);

                    var body = ea.Body.ToArray();
                    var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));

                    if (message == null)
                    {
                        throw new Exception("Failed to deserialize message");
                    }

                    // Log message headers for debugging
                    if (ea.BasicProperties.Headers != null)
                    {
                        foreach (var header in ea.BasicProperties.Headers)
                        {
                            _logger?.LogDebug("Message header: {Key} = {Value}", header.Key, header.Value);
                        }
                    }

                    await handler(message).ConfigureAwait(false);

                    _logger?.LogDebug("Message processed successfully: {DeliveryTag}", ea.DeliveryTag);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error processing message: {DeliveryTag}. Error: {ErrorMessage}", 
                        ea.DeliveryTag, ex.Message);

                    // Send to DLX queue with enhanced error information
                    var errorHeaders = new Dictionary<string, object>
                    {
                        ["X-Error-Message"] = ex.Message,
                        ["X-Error-Type"] = ex.GetType().Name,
                        ["X-Error-Timestamp"] = DateTime.UtcNow.ToString("O"),
                        ["X-Original-Exchange"] = ea.Exchange,
                        ["X-Original-Routing-Key"] = ea.RoutingKey
                    };

                    // Publish to dead letter exchange
                    await PublishToDeadLetterExchange(ea, errorHeaders);
                    
                    // Reject the message
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false).ConfigureAwait(false);
                    return;
                }
                finally
                {
                    // Acknowledge message only if processing was successful
                    try
                    {
                        await _channel.BasicAckAsync(ea.DeliveryTag, false).ConfigureAwait(false);
                    }
                    catch (Exception ackEx)
                    {
                        _logger?.LogError(ackEx, "Failed to acknowledge message: {DeliveryTag}", ea.DeliveryTag);
                    }
                }
            };

            // Disable auto acknowledgment
            await _channel.BasicConsumeAsync(queue: route.QueueName,
                                            autoAck: false,
                                            consumer: consumer).ConfigureAwait(false);

            _logger?.LogInformation("Subscribed to queue: {QueueName} on exchange: {Exchange} with routing key: {RoutingKey}", 
                route.QueueName, route.Exchange, route.RoutingKey);
        }

        private async Task PublishToDeadLetterExchange(BasicDeliverEventArgs ea, Dictionary<string, object> errorHeaders)
        {
            try
            {
                var deadLetterExchange = "dlx.topic.exchange";
                var deadLetterRoutingKey = "failure";

                // Ensure DLX exists
                await _channel.ExchangeDeclareAsync(
                    exchange: deadLetterExchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                // Create error message with original content and error details
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

                await _channel.BasicPublishAsync(
                    exchange: deadLetterExchange,
                    routingKey: deadLetterRoutingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: errorBody);

                _logger?.LogInformation("Message sent to dead letter exchange: {DeliveryTag}", ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to publish message to dead letter exchange: {DeliveryTag}", ea.DeliveryTag);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
