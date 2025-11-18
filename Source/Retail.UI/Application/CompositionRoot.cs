//-----------------------------------------------------------------------
// <copyright file="CompositionRoot.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.UI.Application
{
    using CommonLibrary.Telemetry;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Configuration for this service.
    /// </summary>
    public class CompositionRoot
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="CompositionRoot"/> class from being created.
        /// </summary>
        protected CompositionRoot()
        {
        }

        /// <summary>
        /// Configures application service with a dependency injection container.
        /// </summary>
        /// <param name="context">The application's builder context.</param>
        /// <param name="serviceCollection">Service collection to register services to.</param>
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
        {
            // Configure OpenTelemetry for observability
            serviceCollection.AddOpenTelemetry(
                context.Configuration,
                serviceName: "Retail.UI",
                serviceVersion: "1.0.0");

            // Register metrics service
            serviceCollection.AddSingleton<CommonLibrary.Telemetry.IMetricsService, CommonLibrary.Telemetry.MetricsService>();

            // Add services to the container.
            serviceCollection.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Optionally keep factory for named clients too
            serviceCollection.AddHttpClient();

            // Register HttpClient (default for @inject HttpClient)
            serviceCollection.AddScoped<System.Net.Http.HttpClient>(sp =>
            {
                var navigationManager = sp.GetRequiredService<NavigationManager>();
                return new System.Net.Http.HttpClient { BaseAddress = new System.Uri(navigationManager.BaseUri) };
            });

            serviceCollection.AddRazorPages();

            // Register application lifecycle
            serviceCollection.AddSingleton<UIApplication>();
            serviceCollection.AddSingleton<CommonLibrary.Application.IApplication>(sp => sp.GetRequiredService<UIApplication>());
            serviceCollection.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<UIApplication>());

            // Add health checks
            serviceCollection.AddHealthChecks();
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="configurationBuilder">Configuration builder.</param>
        public static void Configure(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            configurationBuilder.AddEnvironmentVariables();
        }
    }
}

