//-----------------------------------------------------------------------
// <copyright file="DatabaseHealthCheck.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.HealthChecks
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    /// <summary>
    /// Health check for database connectivity.
    /// </summary>
    /// <typeparam name="TContext">The database context type.</typeparam>
    public class DatabaseHealthCheck<TContext> : IHealthCheck
        where TContext : DbContext
    {
        private readonly TContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseHealthCheck{TContext}"/> class.
        /// </summary>
        /// <param name="dbContext">The database context to check.</param>
        public DatabaseHealthCheck(TContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Checks the health of the database connection.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the health check result.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await this.dbContext.Database.CanConnectAsync(cancellationToken);

                if (canConnect)
                {
                    return HealthCheckResult.Healthy("Database connection is healthy");
                }

                return HealthCheckResult.Unhealthy("Database connection failed");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database health check failed", ex);
            }
        }
    }
}

