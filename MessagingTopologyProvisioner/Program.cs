using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommonLibrary.Configuration;
using MessagingInfrastructure.Service;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using MessagingLibrary.Interface;
using MessagingLibrary.Service;

public class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Register RabbitMQ services
                services.AddRabbitMQServices(context.Configuration);
            })
            .Build();

        // Setup RabbitMQ infrastructure
        var topologyInitializer = host.Services.GetRequiredService<TopologyInitializer>();
        await topologyInitializer.SetupInfrastructure();
    }
}
