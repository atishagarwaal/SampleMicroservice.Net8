using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Configuration;

namespace MessagingInfrastructure.Service
{
    public class TopologyInitializer
    {
        private readonly TopologyConfiguration _config;
        private readonly IConnection _connection;
        private readonly ILogger<TopologyInitializer>? _logger;
        private const string X_Message_TTL = "x-message-ttl";
        private const string X_Dead_Letter_Exchange = "x-dead-letter-exchange";
        private const string X_Dead_Letter_Routing_Key = "x-dead-letter-routing-key";
        private const string X_Max_Priority = "x-max-priority";

        public TopologyInitializer(IOptions<TopologyConfiguration> options, IConnection connection, ILogger<TopologyInitializer>? logger = null)
        {
            _config = options.Value ?? throw new ArgumentNullException(nameof(options));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger;
        }

        public async Task SetupInfrastructure()
        {
            try
            {
                _logger?.LogInformation("Starting RabbitMQ topology initialization...");

                // Establish a connection
                using (var channel = await _connection.CreateChannelAsync())
                {
                    // Create dead letter exchange first
                    await CreateDeadLetterExchange(channel);

                    // Create exchanges idempotently
                    foreach (var exchange in _config.Exchanges)
                    {
                        // First, try to delete the existing exchange if it exists to avoid PRECONDITION_FAILED errors
                        try
                        {
                            await channel.ExchangeDeleteAsync(exchange.Name, false);
                            _logger?.LogInformation("Deleted existing exchange: {ExchangeName}", exchange.Name);
                        }
                        catch (Exception ex)
                        {
                            // Exchange might not exist, which is fine
                            _logger?.LogDebug("Exchange {ExchangeName} does not exist or could not be deleted: {Message}", exchange.Name, ex.Message);
                        }

                        await channel.ExchangeDeclareAsync(
                            exchange.Name,
                            exchange.Type,
                            exchange.Durable,
                            exchange.AutoDelete,
                            exchange.Arguments);

                        _logger?.LogInformation("Exchange created successfully: {ExchangeName}", exchange.Name);
                    }

                    // Create queues idempotently
                    foreach (var queue in _config.Queues)
                    {
                        await CreateQueueWithEnhancedConfiguration(channel, queue);
                    }

                    _logger?.LogInformation("RabbitMQ topology initialization completed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize RabbitMQ topology");
                throw;
            }
        }

        private async Task CreateDeadLetterExchange(IChannel channel)
        {
            try
            {
                // First, try to delete existing dead letter queue and exchange to avoid PRECONDITION_FAILED errors
                try
                {
                    await channel.QueueDeleteAsync("dlq.failure", false, false);
                    _logger?.LogInformation("Deleted existing dead letter queue: dlq.failure");
                }
                catch (Exception ex)
                {
                    // Queue might not exist, which is fine
                    _logger?.LogDebug("Dead letter queue does not exist or could not be deleted: {Message}", ex.Message);
                }

                try
                {
                    await channel.ExchangeDeleteAsync("dlx.topic.exchange", false);
                    _logger?.LogInformation("Deleted existing dead letter exchange: dlx.topic.exchange");
                }
                catch (Exception ex)
                {
                    // Exchange might not exist, which is fine
                    _logger?.LogDebug("Dead letter exchange does not exist or could not be deleted: {Message}", ex.Message);
                }

                // Create dead letter exchange for failed messages
                await channel.ExchangeDeclareAsync(
                    exchange: "dlx.topic.exchange",
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                // Create dead letter queue
                await channel.QueueDeclareAsync(
                    queue: "dlq.failure",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        ["x-message-ttl"] = 604800000, // 7 days in milliseconds
                        ["x-max-length"] = 10000 // Maximum number of messages in DLQ
                    });

                // Bind DLQ to DLX
                await channel.QueueBindAsync(
                    queue: "dlq.failure",
                    exchange: "dlx.topic.exchange",
                    routingKey: "failure");

                _logger?.LogInformation("Dead letter exchange and queue created successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create dead letter exchange");
                throw;
            }
        }

        private async Task CreateQueueWithEnhancedConfiguration(IChannel channel, QueueConfig queue)
        {
            try
            {
                // First, try to delete the existing queue if it exists to avoid PRECONDITION_FAILED errors
                try
                {
                    await channel.QueueDeleteAsync(queue.Name, false, false);
                    _logger?.LogInformation("Deleted existing queue: {QueueName}", queue.Name);
                }
                catch (Exception ex)
                {
                    // Queue might not exist, which is fine
                    _logger?.LogDebug("Queue {QueueName} does not exist or could not be deleted: {Message}", queue.Name, ex.Message);
                }

                var arguments = new Dictionary<string, object>();

                // Add dead letter configuration
                arguments[X_Dead_Letter_Exchange] = "dlx.topic.exchange";
                arguments[X_Dead_Letter_Routing_Key] = "failure";

                // Add message TTL if specified in enhanced configuration
                if (queue.MessageTTL > 0)
                {
                    arguments[X_Message_TTL] = queue.MessageTTL;
                }
                // Fallback to arguments if not set in enhanced config
                else if (queue.Arguments != null && queue.Arguments.ContainsKey(X_Message_TTL))
                {
                    arguments[X_Message_TTL] = Convert.ToInt32(queue.Arguments[X_Message_TTL]);
                }

                // Add priority support for queues that need it
                if (queue.EnablePriority)
                {
                    arguments[X_Max_Priority] = queue.MaxPriority;
                }
                // Fallback to arguments if not set in enhanced config
                else if (queue.Arguments != null && queue.Arguments.ContainsKey("priority-enabled") && 
                    Convert.ToBoolean(queue.Arguments["priority-enabled"]))
                {
                    arguments[X_Max_Priority] = 10; // Support priority levels 0-10
                }

                // Add max length to prevent queue overflow
                if (queue.MaxLength > 0)
                {
                    arguments["x-max-length"] = queue.MaxLength;
                }
                // Fallback to arguments if not set in enhanced config
                else if (queue.Arguments != null && queue.Arguments.ContainsKey("max-length"))
                {
                    arguments["x-max-length"] = Convert.ToInt32(queue.Arguments["max-length"]);
                }
                else
                {
                    arguments["x-max-length"] = 1000; // Default max length
                }

                // Merge with existing arguments
                if (queue.Arguments != null)
                {
                    foreach (var arg in queue.Arguments)
                    {
                        if (!arguments.ContainsKey(arg.Key))
                        {
                            arguments[arg.Key] = arg.Value;
                        }
                    }
                }

                await channel.QueueDeclareAsync(
                    queue.Name,
                    queue.Durable,
                    queue.Exclusive,
                    queue.AutoDelete,
                    arguments);

                _logger?.LogInformation("Queue created successfully: {QueueName}", queue.Name);

                // Bind queues to exchanges
                foreach (var binding in queue.Bindings)
                {
                    await channel.QueueBindAsync(
                        queue.Name,
                        binding.ExchangeName,
                        binding.RoutingKey,
                        binding.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value));

                    _logger?.LogInformation("Binding created: {QueueName} -> {ExchangeName} ({RoutingKey})", 
                        queue.Name, binding.ExchangeName, binding.RoutingKey);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create queue: {QueueName}", queue.Name);
                throw;
            }
        }
    }
}
