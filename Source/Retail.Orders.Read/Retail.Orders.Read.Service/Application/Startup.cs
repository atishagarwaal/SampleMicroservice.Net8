//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Orders.Read.Application
{
    using CommonLibrary.Middleware;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Asp.Versioning.ApiExplorer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Prometheus;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Interfaces;

    /// <summary>
    /// Configures web host.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration instance.</param>
        /// <param name="environment">Web host environment.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
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
                c.DocumentTitle = "Order Read Service";
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", 
                        description.GroupName.ToUpperInvariant());
                }
            });

            webApplicationBuilder.UseHttpsRedirection();
            webApplicationBuilder.UseRouting();
            
            // Collect HTTP request metrics for Prometheus
            webApplicationBuilder.UseHttpMetrics();
            
            webApplicationBuilder.UseAuthorization();

            webApplicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                // Prometheus metrics endpoint
                endpoints.MapMetrics();
                
                // Liveness endpoint - indicates the service is running
                endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
                {
                    Predicate = _ => false
                });
                
                // Readiness endpoint - indicates the service is ready to accept traffic
                endpoints.MapHealthChecks("/health/readiness", new HealthCheckOptions
                {
                    Predicate = _ => true
                });
            });
        }
    }
}

