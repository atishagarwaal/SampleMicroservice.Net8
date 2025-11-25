//-----------------------------------------------------------------------
// <copyright file="OrderWriteApplication.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Orders.Write.Application
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using CommonLibrary.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Interfaces;
    using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;

    /// <summary>
    /// Represents the Order Write microservice application lifecycle.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OrderWriteApplication : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<OrderWriteApplication> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderWriteApplication"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="logger">The logger.</param>
        public OrderWriteApplication(
            IServiceProvider serviceProvider,
            ILogger<OrderWriteApplication> logger)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting Order Write Service");

            using (var scope = this.serviceProvider.CreateScope())
            {
                this.logger.LogInformation("Setting up RabbitMQ topology");
                var topologyManager = scope.ServiceProvider.GetRequiredService<IRabbitMQTopologyManager>();
                await topologyManager.SetupTopologyAsync(cancellationToken).ConfigureAwait(false);

                this.logger.LogInformation("Initializing service subscriptions");
                var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
                await serviceInitializer.Initialize().ConfigureAwait(false);

                this.logger.LogInformation("Ensuring database is created");
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Database initialization completed");
            }

            this.logger.LogInformation("Order Write Service started successfully");
        }

        /// <summary>
        /// Stops the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stopping Order Write Service");
            return Task.CompletedTask;
        }
    }
}

