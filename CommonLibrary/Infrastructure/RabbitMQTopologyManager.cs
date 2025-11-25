//-----------------------------------------------------------------------
// <copyright file="RabbitMQTopologyManager.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommonLibrary.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using RabbitMQ.Client;
    using TopologyManager.Configuration;

    /// <summary>
    /// Manages RabbitMQ topology setup including exchanges, queues, and bindings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RabbitMQTopologyManager : IRabbitMQTopologyManager
    {
        private readonly IConnection connection;
        private readonly TopologyConfiguration topologyConfig;
        private readonly ILogger<RabbitMQTopologyManager> logger;
        private const string X_Message_TTL = "x-message-ttl";
        private const string X_Dead_Letter_Exchange = "x-dead-letter-exchange";
        private const string X_Dead_Letter_Routing_Key = "x-dead-letter-routing-key";
        private const string X_Max_Priority = "x-max-priority";

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQTopologyManager"/> class.
        /// </summary>
        /// <param name="connection">RabbitMQ connection.</param>
        /// <param name="topologyConfig">Topology configuration options.</param>
        /// <param name="logger">Logger instance.</param>
        public RabbitMQTopologyManager(
            IConnection connection,
            IOptions<TopologyConfiguration> topologyConfig,
            ILogger<RabbitMQTopologyManager> logger)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.topologyConfig = topologyConfig?.Value ?? throw new ArgumentNullException(nameof(topologyConfig));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sets up the RabbitMQ topology by creating exchanges, queues, and bindings.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SetupTopologyAsync(CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation("Starting RabbitMQ topology setup");

            try
            {
                using var channel = await this.connection.CreateChannelAsync();
                this.logger.LogDebug("Channel created for topology setup");

                await this.SetupDeadLetterInfrastructure(channel, cancellationToken);

                // Setup exchanges
                var exchangeCount = this.topologyConfig.Exchanges?.Count ?? 0;
                this.logger.LogInformation("Setting up {ExchangeCount} exchanges", exchangeCount);

                if (this.topologyConfig.Exchanges != null)
                {
                    foreach (var exchange in this.topologyConfig.Exchanges)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await this.SetupExchange(channel, exchange);
                    }
                }

                // Setup queues
                var queueCount = this.topologyConfig.Queues?.Count ?? 0;
                this.logger.LogInformation("Setting up {QueueCount} queues", queueCount);

                if (this.topologyConfig.Queues != null)
                {
                    foreach (var queue in this.topologyConfig.Queues)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await this.SetupQueueWithBindings(channel, queue, cancellationToken);
                    }
                }

                this.logger.LogInformation("RabbitMQ topology setup completed successfully. Created {ExchangeCount} exchanges and {QueueCount} queues",
                    exchangeCount, queueCount);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to setup RabbitMQ topology");
                throw;
            }
        }

        /// <summary>
        /// Sets up the dead letter exchange and queue infrastructure.
        /// </summary>
        /// <param name="channel">RabbitMQ channel.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        private async Task SetupDeadLetterInfrastructure(IChannel channel, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Setting up dead letter exchange and queue infrastructure");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                const string dlqName = "dlq.failure";
                const string dlxName = "dlx.topic.exchange";

                try
                {
                    await channel.QueueDeleteAsync(dlqName, false, false);
                    this.logger.LogInformation("Deleted existing dead letter queue: {DlqName}", dlqName);
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug("Dead letter queue does not exist or could not be deleted: {Message}", ex.Message);
                }

                try
                {
                    await channel.ExchangeDeleteAsync(dlxName, false);
                    this.logger.LogInformation("Deleted existing dead letter exchange: {DlxName}", dlxName);
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug("Dead letter exchange does not exist or could not be deleted: {Message}", ex.Message);
                }

                this.logger.LogDebug("Declaring dead letter exchange: {DlxName}", dlxName);
                await channel.ExchangeDeclareAsync(
                    exchange: dlxName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                this.logger.LogDebug("Declaring dead letter queue: {DlqName}", dlqName);
                await channel.QueueDeclareAsync(
                    queue: dlqName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>
                    {
                        [X_Message_TTL] = 604800000, // 7 days in milliseconds
                        ["x-max-length"] = 10000 // Maximum number of messages in DLQ
                    });

                this.logger.LogDebug("Binding dead letter queue to exchange");
                await channel.QueueBindAsync(
                    queue: dlqName,
                    exchange: dlxName,
                    routingKey: "failure");

                this.logger.LogInformation("Dead letter exchange and queue infrastructure setup completed successfully");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to setup dead letter infrastructure");
                throw;
            }
        }

        /// <summary>
        /// Sets up a single exchange.
        /// </summary>
        /// <param name="channel">RabbitMQ channel.</param>
        /// <param name="exchange">Exchange configuration.</param>
        private async Task SetupExchange(IChannel channel, ExchangeConfig exchange)
        {
            try
            {
                try
                {
                    await channel.ExchangeDeleteAsync(exchange.Name, false);
                    this.logger.LogInformation("Deleted existing exchange: {ExchangeName}", exchange.Name);
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug("Exchange {ExchangeName} does not exist or could not be deleted: {Message}",
                        exchange.Name, ex.Message);
                }

                await channel.ExchangeDeclareAsync(
                    exchange.Name,
                    exchange.Type,
                    exchange.Durable,
                    exchange.AutoDelete,
                    exchange.Arguments);

                this.logger.LogInformation("Exchange setup completed: {ExchangeName}", exchange.Name);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to setup exchange: {ExchangeName}", exchange.Name);
                throw;
            }
        }

        /// <summary>
        /// Sets up a queue with enhanced configuration and bindings.
        /// </summary>
        /// <param name="channel">RabbitMQ channel.</param>
        /// <param name="queue">Queue configuration.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        private async Task SetupQueueWithBindings(IChannel channel, QueueConfig queue, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Setting up queue: {QueueName}", queue.Name);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await channel.QueueDeleteAsync(queue.Name, false, false);
                    this.logger.LogInformation("Deleted existing queue: {QueueName}", queue.Name);
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug("Queue {QueueName} does not exist or could not be deleted: {Message}", queue.Name, ex.Message);
                }

                var arguments = this.BuildQueueArguments(queue);

                this.logger.LogDebug("Declaring queue: {QueueName} with {ArgumentCount} arguments", queue.Name, arguments.Count);
                await channel.QueueDeclareAsync(
                    queue.Name,
                    queue.Durable,
                    queue.Exclusive,
                    queue.AutoDelete,
                    arguments);

                this.logger.LogInformation("Queue setup completed: {QueueName}", queue.Name);

                // Setup bindings
                var bindingCount = queue.Bindings?.Count ?? 0;
                this.logger.LogDebug("Setting up {BindingCount} bindings for queue: {QueueName}", bindingCount, queue.Name);

                if (queue.Bindings != null)
                {
                    foreach (var binding in queue.Bindings)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await this.SetupBinding(channel, queue.Name, binding);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to setup queue: {QueueName}", queue.Name);
                throw;
            }
        }

        /// <summary>
        /// Builds queue arguments from configuration.
        /// </summary>
        /// <param name="queue">Queue configuration.</param>
        /// <returns>Dictionary of queue arguments.</returns>
        private Dictionary<string, object?> BuildQueueArguments(QueueConfig queue)
        {
            var arguments = new Dictionary<string, object?>();

            // Add dead letter configuration
            arguments[X_Dead_Letter_Exchange] = "dlx.topic.exchange";
            arguments[X_Dead_Letter_Routing_Key] = "failure";

            // Add message TTL if specified
            if (queue.MessageTTL > 0)
            {
                arguments[X_Message_TTL] = queue.MessageTTL;
            }
            else if (queue.Arguments != null && queue.Arguments.ContainsKey(X_Message_TTL))
            {
                arguments[X_Message_TTL] = Convert.ToInt32(queue.Arguments[X_Message_TTL]);
            }

            // Add priority support for queues that need it
            if (queue.EnablePriority)
            {
                arguments[X_Max_Priority] = queue.MaxPriority;
            }
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

            return arguments;
        }

        /// <summary>
        /// Sets up a binding between a queue and an exchange.
        /// </summary>
        /// <param name="channel">RabbitMQ channel.</param>
        /// <param name="queueName">Queue name.</param>
        /// <param name="binding">Binding configuration.</param>
        private async Task SetupBinding(IChannel channel, string queueName, BindingConfig binding)
        {
            if (string.IsNullOrEmpty(binding.ExchangeName) || string.IsNullOrEmpty(binding.RoutingKey))
            {
                this.logger.LogWarning("Skipping binding for queue {QueueName} - ExchangeName or RoutingKey is null or empty", queueName);
                return;
            }

            await channel.QueueBindAsync(
                queueName,
                binding.ExchangeName,
                binding.RoutingKey,
                binding.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value));

            this.logger.LogInformation("Binding setup completed: {QueueName} -> {ExchangeName} ({RoutingKey})",
                queueName, binding.ExchangeName, binding.RoutingKey);
        }
    }
}

