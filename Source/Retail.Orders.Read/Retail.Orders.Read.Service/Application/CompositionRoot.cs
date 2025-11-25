//-----------------------------------------------------------------------
// <copyright file="CompositionRoot.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Orders.Read.Application
{
    using CommonLibrary.Handlers;
    using InventoryUpdatedEventNameSpace;
    using MediatR;
    using MessagingInfrastructure.Service;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Converters;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
    using Retail.Orders.Read.src.CleanArchitecture.Application.EventHandlers;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Interfaces;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Queries;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Service;
    using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
    using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
    using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
    using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.UnitOfWork;
    using CommonLibrary.Configuration;
    using CommonLibrary.Infrastructure;
    using CommonLibrary.Telemetry;
    using Asp.Versioning.ApiExplorer;
    using Asp.Versioning;
    using Microsoft.Extensions.Options;
    using RabbitMQ.Client;
    using Microsoft.OpenApi.Models;
    using System;
    using System.IO;
    using System.Reflection;
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
                serviceName: "Retail.Orders.Read",
                serviceVersion: "1.0.0");

            // Configure strongly-typed configuration classes
            serviceCollection.Configure<DatabaseConnectionConfiguration>(
                context.Configuration.GetSection("ConnectionStrings"));
            serviceCollection.Configure<MongoDBSettings>(
                context.Configuration.GetSection(nameof(MongoDBSettings)));
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

            // Configure MongoDB connection
            serviceCollection.AddScoped<ApplicationDbContext>();
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register MediatR with all relevant assemblies
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(GetAllOrdersQuery).Assembly,
                typeof(GetOrderByIdQuery).Assembly
            ));

            // Register event handlers with proper logging
            serviceCollection.AddScoped<IEventHandler<InventoryUpdatedEvent>, InventoryUpdatedEventHandler>();
            serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

            // Register converters
            serviceCollection.AddSingleton<IConverter<LineItemDto, Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.LineItem>, LineItemConverter>();
            serviceCollection.AddSingleton<IConverter<Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.LineItem, LineItemDto>, LineItemDtoConverter>();
            serviceCollection.AddSingleton<IConverter<OrderDto, Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.Order>, OrderConverter>();
            serviceCollection.AddSingleton<IConverter<Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.Order, OrderDto>, OrderDtoConverter>();

            // Register application lifecycle
            serviceCollection.AddSingleton<OrderReadApplication>();
            serviceCollection.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<OrderReadApplication>());

            // Add RabbitMQ from the common project
            serviceCollection.AddRabbitMQServices(context.Configuration);

            // Register RabbitMQ topology manager
            serviceCollection.AddSingleton<IRabbitMQTopologyManager, RabbitMQTopologyManager>();

            // Add API versioning
            serviceCollection.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"));
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            serviceCollection.AddEndpointsApiExplorer();
            serviceCollection.AddControllers();

            serviceCollection.AddSwaggerGen(c =>
            {
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
                using var serviceProvider = serviceCollection.BuildServiceProvider();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
                var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
                var version = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = $"Order Read Service version {version} - OpenAPI {description.ApiVersion}",
                        Description = $"Order Read service API v{description.ApiVersion}",
                    });
                }

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
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

