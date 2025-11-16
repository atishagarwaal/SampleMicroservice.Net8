//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Orders.Write.Application
{
    using CommonLibrary.Middleware;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Interfaces;
    using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;

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
        /// <param name="app">An <see cref="IApplicationBuilder"/> for the application to configure.</param>
        public void Configure(IApplicationBuilder app)
        {
            this.environment.ApplicationName = "Retail.Orders.Write";

            // Register global exception handling middleware early in the pipeline
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            if (this.environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
                {
                    Predicate = _ => true
                });
            });
        }
    }
}

