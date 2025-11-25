//-----------------------------------------------------------------------
// <copyright file="BFFApplication.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.BFFWeb.Api.Application
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents the BFF microservice application lifecycle.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BFFApplication : IHostedService
    {
        private readonly ILogger<BFFApplication> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BFFApplication"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public BFFApplication(ILogger<BFFApplication> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting BFF Service");
            this.logger.LogInformation("BFF Service started successfully");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stopping BFF Service");
            return Task.CompletedTask;
        }
    }
}

