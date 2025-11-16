//-----------------------------------------------------------------------
// <copyright file="CustomerApplication.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Api.Customers.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommonLibrary.Application;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;

    /// <summary>
    /// Represents the Customer microservice application lifecycle.
    /// </summary>
    public class CustomerApplication : IApplication, IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CustomerApplication> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerApplication"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="logger">The logger.</param>
        public CustomerApplication(
            IServiceProvider serviceProvider,
            ILogger<CustomerApplication> logger)
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
            this._logger.LogInformation("Starting Customer Service");

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

            this._logger.LogInformation("Customer Service started successfully");
        }

        /// <summary>
        /// Stops the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Stopping Customer Service");
            return Task.CompletedTask;
        }
    }
}

