//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Api.Products
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Retail.Api.Products.Application;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
    using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;

    /// <summary>
    /// Contains the main entry point of the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        /// <param name="args">Command line arguments which will be passed to the application host.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(CompositionRoot.Configure)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(CompositionRoot.ConfigureServices)
                .Build();

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Program");

            try
            {
                logger.LogInformation("Starting Product Service");

                using (var scope = host.Services.CreateScope())
                {
                    logger.LogInformation("Initializing service subscriptions");
                    var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
                    await serviceInitializer.Initialize();

                    logger.LogInformation("Ensuring database is created");
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await db.Database.EnsureCreatedAsync();
                    logger.LogInformation("Database initialization completed");
                }

                logger.LogInformation("Product Service started successfully");
                await host.RunAsync();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error starting Product Service");
                throw;
            }
        }
    }
}
