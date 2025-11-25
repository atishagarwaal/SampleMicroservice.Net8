//-----------------------------------------------------------------------
// <copyright file="IRabbitMQTopologyManager.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for managing RabbitMQ topology setup (exchanges, queues, and bindings).
    /// </summary>
    public interface IRabbitMQTopologyManager
    {
        /// <summary>
        /// Sets up the RabbitMQ topology by creating exchanges, queues, and bindings.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetupTopologyAsync(CancellationToken cancellationToken = default);
    }
}

