//-----------------------------------------------------------------------
// <copyright file="OrderReadApplication.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Orders.Read.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommonLibrary.Application;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Interfaces;

    /// <summary>
    /// Represents the Order Read microservice application lifecycle.
    /// </summary>
    public class OrderReadApplication : IApplication, IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderReadApplication> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderReadApplication"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="logger">The logger.</param>
        public OrderReadApplication(
            IServiceProvider serviceProvider,
            ILogger<OrderReadApplication> logger)
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
            this._logger.LogInformation("Starting Order Read Service");

            using (var scope = this._serviceProvider.CreateScope())
            {
                this._logger.LogInformation("Initializing service subscriptions");
                var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
                await serviceInitializer.Initialize();
                this._logger.LogInformation("Service subscriptions initialized successfully");
            }

            this._logger.LogInformation("Order Read Service started successfully");
        }

        /// <summary>
        /// Stops the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Stopping Order Read Service");
            return Task.CompletedTask;
        }
    }
}

