//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Api.Customers.Application
{
    using System.Diagnostics.CodeAnalysis;
    using CommonLibrary.Middleware;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Asp.Versioning.ApiExplorer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Prometheus;
    using CommonLibrary.Configuration;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;

    /// <summary>
    /// Configures web host.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration instance.</param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Register services into the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Services are configured in CompositionRoot.ConfigureServices
            // This method can be used for additional web-specific configurations if needed
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="webApplicationBuilder">An <see cref="IApplicationBuilder"/> for the applicationBuilder to configure.</param>
        /// <param name="webEnvironment">An <see cref="IWebHostEnvironment"/> for the applicationBuilder to configure.</param>
        public static void Configure(IApplicationBuilder webApplicationBuilder, IWebHostEnvironment webEnvironment)
        {
            webEnvironment.ApplicationName = typeof(Startup).Assembly.GetName().Name;

            // Register global exception handling middleware early in the pipeline
            webApplicationBuilder.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            if (webEnvironment.IsDevelopment())
            {
                webApplicationBuilder.UseDeveloperExceptionPage();
            }

            webApplicationBuilder.UseSwagger();
            webApplicationBuilder.UseSwaggerUI(c =>
            {
                var provider = webApplicationBuilder.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                c.DocumentTitle = "Customer Service";
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", 
                        description.GroupName.ToUpperInvariant());
                }
            });

            webApplicationBuilder.UseHttpsRedirection();
            webApplicationBuilder.UseRouting();
            
            // Get metrics configuration
            var metricsConfiguration = webApplicationBuilder.ApplicationServices.GetRequiredService<IOptions<MetricsConfiguration>>().Value;
            
            // Collect HTTP request metrics for Prometheus (conditional)
            if (metricsConfiguration.Enabled)
            {
                webApplicationBuilder.UseHttpMetrics();
            }
            
            webApplicationBuilder.UseAuthorization();

            webApplicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                // Prometheus metrics endpoint (conditional)
                if (metricsConfiguration.Enabled)
                {
                    endpoints.MapMetrics();
                }
                
                // Liveness endpoint - indicates the service is running
                endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
                {
                    Predicate = _ => false
                });
                
                // Readiness endpoint - indicates the service is ready to accept traffic
                endpoints.MapHealthChecks("/health/readiness", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("database")
                });
            });
        }
    }
}

