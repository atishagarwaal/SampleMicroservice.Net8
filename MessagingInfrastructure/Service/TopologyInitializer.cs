using Microsoft.Extensions.Options;
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
        private const string X_Message_TTL = "x-message-ttl";

        public TopologyInitializer(IOptions<TopologyConfiguration> options, IConnection connection)
        {
            _config = options.Value;
            _connection = connection;
        }

        public async Task SetupInfrastructure()
        {
            // Establish a connection
            using (var channel = await _connection.CreateChannelAsync())
            {
                // Create exchanges idempotently
                foreach (var exchange in _config.Exchanges)
                {
                    await channel.ExchangeDeclareAsync(
                        exchange.Name,
                        exchange.Type,
                        exchange.Durable,
                        exchange.AutoDelete,
                        exchange.Arguments);

                    Console.WriteLine($"Exchange created or already exists: {exchange.Name}");
                }

                // Create queues idempotently
                foreach (var queue in _config.Queues)
                {
                    if (queue.Arguments != null && queue.Arguments.ContainsKey(X_Message_TTL))
                    {
                        queue.Arguments[X_Message_TTL] = Convert.ToInt32(queue.Arguments[X_Message_TTL]);
                    }

                    await channel.QueueDeclareAsync(
                        queue.Name,
                        queue.Durable,
                        queue.Exclusive,
                        queue.AutoDelete,
                        queue.Arguments?.Count > 0 ? queue.Arguments : null);

                    Console.WriteLine($"Queue created or already exists: {queue.Name}");

                    // Bind queues to exchanges
                    foreach (var binding in queue.Bindings)
                    {
                        await channel.QueueBindAsync(
                            queue.Name,
                            binding.ExchangeName,
                            binding.RoutingKey,
                            binding.Arguments?.Count > 0 ? binding.Arguments : null);

                        Console.WriteLine($"Binding created: {queue.Name} -> {binding.ExchangeName} ({binding.RoutingKey})");
                    }
                }
            }
        }
    }
}
