using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Starting Messaging Infrastructure setup");
            
            // Setup RabbitMQ infrastructure
            var topologyInitializer = host.Services.GetRequiredService<TopologyInitializer>();
            await topologyInitializer.SetupInfrastructure();
            
            logger.LogInformation("Messaging Infrastructure setup completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting up Messaging Infrastructure");
            throw;
        }
    }
}
