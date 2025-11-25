//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.UI.Application
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Prometheus;

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

            // Configure the HTTP request pipeline.
            if (!webEnvironment.IsDevelopment())
            {
                webApplicationBuilder.UseExceptionHandler("/Error", createScopeForErrors: true);
                webApplicationBuilder.UseHsts();
            }

            webApplicationBuilder.UseHttpsRedirection();
            webApplicationBuilder.UseStaticFiles();
            webApplicationBuilder.UseAntiforgery();
            webApplicationBuilder.UseRouting();
            
            // Collect HTTP request metrics for Prometheus
            webApplicationBuilder.UseHttpMetrics();

            webApplicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                
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

