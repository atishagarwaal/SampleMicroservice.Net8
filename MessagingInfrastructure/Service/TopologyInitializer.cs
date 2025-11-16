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
    /// <summary>
    /// Topology initializer class for setting up RabbitMQ exchanges and queues.
    /// </summary>
    public class TopologyInitializer
    {
        private readonly TopologyConfiguration _config;
        private readonly IConnection _connection;
        private readonly ILogger<TopologyInitializer> _logger;
        private const string X_Message_TTL = "x-message-ttl";
        private const string X_Dead_Letter_Exchange = "x-dead-letter-exchange";
        private const string X_Dead_Letter_Routing_Key = "x-dead-letter-routing-key";
        private const string X_Max_Priority = "x-max-priority";

        /// <summary>
        /// Initializes a new instance of the <see cref="TopologyInitializer"/> class.
        /// </summary>
        /// <param name="options">Topology configuration options.</param>
        /// <param name="connection">RabbitMQ connection.</param>
        /// <param name="logger">Instance of logger.</param>
        public TopologyInitializer(
            IOptions<TopologyConfiguration> options,
            IConnection connection,
            ILogger<TopologyInitializer> logger)
        {
            _config = options.Value ?? throw new ArgumentNullException(nameof(options));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sets up RabbitMQ infrastructure (exchanges and queues).
        /// </summary>
        public async Task SetupInfrastructure()
        {
            _logger.LogInformation("Starting RabbitMQ topology initialization");

            try
            {
                using (var channel = await _connection.CreateChannelAsync())
                {
                    _logger.LogDebug("Channel created for topology initialization");

                    await CreateDeadLetterExchange(channel);

                    var exchangeCount = _config.Exchanges?.Count ?? 0;
                    _logger.LogInformation("Creating {ExchangeCount} exchanges", exchangeCount);

                    foreach (var exchange in _config.Exchanges)
                    {
                        try
                        {
                            await channel.ExchangeDeleteAsync(exchange.Name, false);
                            _logger.LogInformation("Deleted existing exchange: {ExchangeName}", exchange.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug("Exchange {ExchangeName} does not exist or could not be deleted: {Message}", 
                                exchange.Name, ex.Message);
                        }

                        await channel.ExchangeDeclareAsync(
                            exchange.Name,
                            exchange.Type,
                            exchange.Durable,
                            exchange.AutoDelete,
                            exchange.Arguments);

                        _logger.LogInformation("Exchange created successfully: {ExchangeName}", exchange.Name);
                    }

                    var queueCount = _config.Queues?.Count ?? 0;
                    _logger.LogInformation("Creating {QueueCount} queues", queueCount);

                    foreach (var queue in _config.Queues)
                    {
                        await CreateQueueWithEnhancedConfiguration(channel, queue);
                    }

                    _logger.LogInformation("RabbitMQ topology initialization completed successfully. Created {ExchangeCount} exchanges and {QueueCount} queues", 
                        exchangeCount, queueCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ topology");
                throw;
            }
        }

        /// <summary>
        /// Creates the dead letter exchange and queue.
        /// </summary>
        /// <param name="channel">RabbitMQ channel.</param>
        private async Task CreateDeadLetterExchange(IChannel channel)
        {
            _logger.LogInformation("Creating dead letter exchange and queue");

            try
            {
                try
                {
                    await channel.QueueDeleteAsync("dlq.failure", false, false);
                    _logger.LogInformation("Deleted existing dead letter queue: dlq.failure");
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Dead letter queue does not exist or could not be deleted: {Message}", ex.Message);
                }

                try
                {
                    await channel.ExchangeDeleteAsync("dlx.topic.exchange", false);
                    _logger.LogInformation("Deleted existing dead letter exchange: dlx.topic.exchange");
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Dead letter exchange does not exist or could not be deleted: {Message}", ex.Message);
                }

                _logger.LogDebug("Declaring dead letter exchange: dlx.topic.exchange");
                await channel.ExchangeDeclareAsync(
                    exchange: "dlx.topic.exchange",
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                _logger.LogDebug("Declaring dead letter queue: dlq.failure");
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

                _logger.LogDebug("Binding dead letter queue to exchange");
                await channel.QueueBindAsync(
                    queue: "dlq.failure",
                    exchange: "dlx.topic.exchange",
                    routingKey: "failure");

                _logger.LogInformation("Dead letter exchange and queue created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create dead letter exchange");
                throw;
            }
        }

        /// <summary>
        /// Creates a queue with enhanced configuration options.
        /// </summary>
        /// <param name="channel">RabbitMQ channel.</param>
        /// <param name="queue">Queue configuration.</param>
        private async Task CreateQueueWithEnhancedConfiguration(IChannel channel, QueueConfig queue)
        {
            _logger.LogInformation("Creating queue with enhanced configuration: {QueueName}", queue.Name);

            try
            {
                try
                {
                    await channel.QueueDeleteAsync(queue.Name, false, false);
                    _logger.LogInformation("Deleted existing queue: {QueueName}", queue.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Queue {QueueName} does not exist or could not be deleted: {Message}", queue.Name, ex.Message);
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

                _logger.LogDebug("Declaring queue: {QueueName} with {ArgumentCount} arguments", queue.Name, arguments.Count);
                await channel.QueueDeclareAsync(
                    queue.Name,
                    queue.Durable,
                    queue.Exclusive,
                    queue.AutoDelete,
                    arguments);

                _logger.LogInformation("Queue created successfully: {QueueName}", queue.Name);

                var bindingCount = queue.Bindings?.Count ?? 0;
                _logger.LogDebug("Creating {BindingCount} bindings for queue: {QueueName}", bindingCount, queue.Name);

                foreach (var binding in queue.Bindings)
                {
                    await channel.QueueBindAsync(
                        queue.Name,
                        binding.ExchangeName,
                        binding.RoutingKey,
                        binding.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value));

                    _logger.LogInformation("Binding created: {QueueName} -> {ExchangeName} ({RoutingKey})", 
                        queue.Name, binding.ExchangeName, binding.RoutingKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create queue: {QueueName}", queue.Name);
                throw;
            }
        }
    }
}
