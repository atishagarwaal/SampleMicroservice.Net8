//-----------------------------------------------------------------------
// <copyright file="ProductApplication.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Api.Products.Application
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using CommonLibrary.Infrastructure;
    using CommonLibrary.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
    using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;

    /// <summary>
    /// Represents the Product microservice application lifecycle.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProductApplication : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ProductApplication> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductApplication"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="logger">The logger.</param>
        public ProductApplication(
            IServiceProvider serviceProvider,
            ILogger<ProductApplication> logger)
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
            this.logger.LogServiceStartup("Product Service");

            using (var scope = this.serviceProvider.CreateScope())
            {
                this.logger.LogTopologySetup();
                var topologyManager = scope.ServiceProvider.GetRequiredService<IRabbitMQTopologyManager>();
                await topologyManager.SetupTopologyAsync(cancellationToken).ConfigureAwait(false);

                this.logger.LogServiceSubscriptionsInitialization();
                var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
                await serviceInitializer.Initialize().ConfigureAwait(false);

                this.logger.LogDatabaseCreation();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                this.logger.LogDatabaseInitializationCompleted();
            }

            this.logger.LogServiceStartedSuccessfully("Product Service");
        }

        /// <summary>
        /// Stops the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogServiceStopping("Product Service");
            return Task.CompletedTask;
        }
    }
}

