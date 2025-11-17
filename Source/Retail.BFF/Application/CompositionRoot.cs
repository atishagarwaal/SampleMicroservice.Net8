//-----------------------------------------------------------------------
// <copyright file="CompositionRoot.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.BFFWeb.Api.Application
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using CommonLibrary.Telemetry;
    using Retail.BFFWeb.Api.Configurations;
    using Retail.BFFWeb.Api.Interface;
    using Retail.BFFWeb.Api.Provider;

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
                serviceName: "Retail.BFF",
                serviceVersion: "1.0.0");

            serviceCollection.AddHttpClient();

            // Add services to the container.
            serviceCollection.AddSingleton<ICustomerProvider, CustomerProvider>();
            serviceCollection.AddSingleton<IOrderProvider, OrderProvider>();
            serviceCollection.AddSingleton<IProductProvider, ProductProvider>();

            // Register application lifecycle
            serviceCollection.AddSingleton<BFFApplication>();
            serviceCollection.AddSingleton<CommonLibrary.Application.IApplication>(sp => sp.GetRequiredService<BFFApplication>());
            serviceCollection.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<BFFApplication>());

            serviceCollection.Configure<CustomerServiceConfig>(context.Configuration.GetSection("CustomerServiceConfig"));
            serviceCollection.Configure<OrderServiceConfig>(context.Configuration.GetSection("OrderServiceConfig"));
            serviceCollection.Configure<ProductServiceConfig>(context.Configuration.GetSection("ProductServiceConfig"));

            serviceCollection.AddControllers();

            // Add API versioning
            serviceCollection.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            serviceCollection.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Aggregated Data", Version = "v1" });
            });

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

