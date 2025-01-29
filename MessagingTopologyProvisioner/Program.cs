using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TopologyManager.Configuration;
using TopologyManager.Service;

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
                var rabbitMQConfig = context.Configuration.GetSection("RabbitMQ");
                services.Configure<RabbitMQConfig>(rabbitMQConfig);
                services.AddSingleton<RabbitMQService>();
            })
            .Build();

        // Setup RabbitMQ infrastructure
        var rabbitMQService = host.Services.GetRequiredService<RabbitMQService>();
        await rabbitMQService.SetupInfrastructure();
    }
}
