using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopologyManager.Configuration;

namespace TopologyManager.Service
{
    public class RabbitMQService
    {
        private readonly RabbitMQConfig _config;

        public RabbitMQService(IOptions<RabbitMQConfig> options)
        {
            this._config = options.Value;
        }

        public async Task SetupInfrastructure()
        {
            var factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                Port = _config.Port,
                UserName = _config.Username,
                Password = _config.Password
            };

            // Establish a connection
            using (var connection = await factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                // Create exchanges
                foreach (var exchange in _config.Exchanges)
                {
                    await channel.ExchangeDeclareAsync(
                        exchange.Name,
                        exchange.Type,
                        exchange.Durable,
                        exchange.AutoDelete,
                        exchange.Arguments);

                    Console.WriteLine($"Exchange created: {exchange.Name}");
                }

                // Create queues
                foreach (var queue in _config.Queues)
                {
                    await channel.QueueDeclareAsync(
                        queue.Name,
                        queue.Durable,
                        queue.Exclusive,
                        queue.AutoDelete,
                        queue.Arguments);

                    Console.WriteLine($"Queue created: {queue.Name}");
                }

                // Bind queues to exchanges
                foreach (var binding in _config.Bindings)
                {
                    await channel.QueueBindAsync(
                        binding.QueueName,
                        binding.ExchangeName,
                        binding.RoutingKey,
                        binding.Arguments);

                    Console.WriteLine($"Binding created: {binding.QueueName} -> {binding.ExchangeName} ({binding.RoutingKey})");
                }
            }
        }
    }
}
