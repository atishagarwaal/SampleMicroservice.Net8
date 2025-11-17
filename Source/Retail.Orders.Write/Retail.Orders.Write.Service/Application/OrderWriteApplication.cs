//-----------------------------------------------------------------------
// <copyright file="OrderWriteApplication.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Orders.Write.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommonLibrary.Application;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Interfaces;
    using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;

    /// <summary>
    /// Represents the Order Write microservice application lifecycle.
    /// </summary>
    public class OrderWriteApplication : IApplication, IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderWriteApplication> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderWriteApplication"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="logger">The logger.</param>
        public OrderWriteApplication(
            IServiceProvider serviceProvider,
            ILogger<OrderWriteApplication> logger)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting Order Write Service");

            using (var scope = this._serviceProvider.CreateScope())
            {
                this._logger.LogInformation("Initializing service subscriptions");
                var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
                await serviceInitializer.Initialize();

                this._logger.LogInformation("Ensuring database is created");
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.EnsureCreatedAsync(cancellationToken);
                this._logger.LogInformation("Database initialization completed");
            }

            this._logger.LogInformation("Order Write Service started successfully");
        }

        /// <summary>
        /// Stops the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Stopping Order Write Service");
            return Task.CompletedTask;
        }
    }
}

