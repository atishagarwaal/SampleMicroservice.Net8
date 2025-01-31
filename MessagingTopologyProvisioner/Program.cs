using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommonLibrary.Configuration;
using MessagingInfrastructure.Service;

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
                var topologyConfig = context.Configuration.GetSection("TopologyConfiguration");
                services.Configure<TopologyConfiguration>(topologyConfig);
                services.AddSingleton<TopologyInitializer>();
            })
            .Build();

        // Setup RabbitMQ infrastructure
        var rabbitMQService = host.Services.GetRequiredService<TopologyInitializer>();
        await rabbitMQService.SetupInfrastructure();
    }
}
