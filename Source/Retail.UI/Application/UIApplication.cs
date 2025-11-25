//-----------------------------------------------------------------------
// <copyright file="UIApplication.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.UI.Application
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents the Retail UI application lifecycle.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UIApplication : IHostedService
    {
        private readonly ILogger<UIApplication> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIApplication"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public UIApplication(ILogger<UIApplication> logger)
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
            this.logger.LogInformation("Starting Retail UI application");
            this.logger.LogInformation("Retail UI application started successfully");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the application asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stopping Retail UI application");
            return Task.CompletedTask;
        }
    }
}

