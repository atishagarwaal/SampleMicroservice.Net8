//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Orders.Read
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Retail.Orders.Read.Application;

    /// <summary>
    /// Contains the main entry point of the application.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
                var application = host.Services.GetRequiredService<CommonLibrary.Application.IApplication>();
                await application.StartAsync(CancellationToken.None);

                await host.RunAsync();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error starting Order Read Service");
                throw;
            }
        }
    }
}
