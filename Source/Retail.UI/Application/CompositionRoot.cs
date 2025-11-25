//-----------------------------------------------------------------------
// <copyright file="CompositionRoot.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.UI.Application
{
    using CommonLibrary.Configuration;
    using CommonLibrary.Telemetry;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Configuration for this service.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class CompositionRoot
    {

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

            // Configure strongly-typed configuration classes
            serviceCollection.Configure<MetricsConfiguration>(
                context.Configuration.GetSection(nameof(MetricsConfiguration)));

            // Register metrics service conditionally based on configuration
            serviceCollection.AddSingleton<CommonLibrary.Telemetry.IMetricsService>(services =>
            {
                var metricsConfiguration = services.GetRequiredService<IOptions<MetricsConfiguration>>();
                if (metricsConfiguration.Value.Enabled)
                {
                    return new CommonLibrary.Telemetry.MetricsService();
                }
                else
                {
                    return new CommonLibrary.Telemetry.EmptyMetricsService();
                }
            });

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

