using CommonLibrary.Configuration;
using MessagingLibrary.Interface;
using MessagingLibrary.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingInfrastructure.Service
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddRabbitMQServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Load configuration
            services.Configure<TopologyConfiguration>(configuration.GetSection("TopologyConfiguration"));

            // Register RabbitMQ ConnectionFactory as Singleton
            services.AddSingleton<IConnectionFactory>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<TopologyConfiguration>>().Value;
                return new ConnectionFactory
                {
                    HostName = config.HostName,
                    Port = config.Port,
                    UserName = config.Username,
                    Password = config.Password
                };
            });

            // Register IConnection as Singleton (Shared RabbitMQ Connection)
            services.AddSingleton<IConnection>(sp =>
            {
                var factory = sp.GetRequiredService<IConnectionFactory>();
                return factory.CreateConnectionAsync().Result;
            });

            // Register TopologyInitializer to set up RabbitMQ exchanges & queues
            services.AddSingleton<TopologyInitializer>();

            // Register Publisher & Subscriber
            services.AddTransient(typeof(IMessagePublisher), typeof(MessagePublisher));
            services.AddTransient(typeof(IMessageSubscriber), typeof(MessageSubscriber));

            return services;
        }
    }
}
