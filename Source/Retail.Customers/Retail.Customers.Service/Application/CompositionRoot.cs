//-----------------------------------------------------------------------
// <copyright file="CompositionRoot.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Api.Customers.Application
{
    using CommonLibrary.Handlers;
    using CommonLibrary.MessageContract;
    using InventoryUpdatedEventNameSpace;
    using MessagingInfrastructure.Service;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Customers.src.CleanArchitecture.Application.EventHandlers;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Service;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Validation;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Validation.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
    using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
    using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Repositories;
    using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.UnitOfWork;
    using CommonLibrary.Configuration;
    using CommonLibrary.Infrastructure;
    using CommonLibrary.Telemetry;
    using Microsoft.AspNetCore.Mvc;
    using RabbitMQ.Client;
    using Asp.Versioning.ApiExplorer;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using System;
    using System.IO;
    using System.Reflection;
    using Asp.Versioning;
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
                serviceName: "Retail.Customers",
                serviceVersion: "1.0.0");

            // Configure strongly-typed configuration classes
            serviceCollection.Configure<DatabaseConnectionConfiguration>(
                context.Configuration.GetSection("ConnectionStrings"));
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

            // Configure database connection
            serviceCollection.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var dbConfig = serviceProvider.GetRequiredService<IOptions<DatabaseConnectionConfiguration>>().Value;
                options.UseSqlServer(dbConfig.DefaultConnection);
            }, ServiceLifetime.Scoped);

            // Configure services
            serviceCollection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
            serviceCollection.AddScoped<ICustomerService, CustomerService>();
            serviceCollection.AddScoped<INotificationRepository, NotificationRepository>();

            // Add RabbitMQ from the common project
            serviceCollection.AddRabbitMQServices(context.Configuration);

            // Register RabbitMQ topology manager
            serviceCollection.AddSingleton<IRabbitMQTopologyManager, RabbitMQTopologyManager>();

            serviceCollection.AddScoped<IEventHandler<InventoryUpdatedEvent>, InventoryUpdatedEventHandler>();
            serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

            // Register validators
            serviceCollection.AddScoped<IMessageValidator<CustomerDto>, CustomerDtoValidator>();

            // Register converters
            serviceCollection.AddSingleton<IConverter<CustomerDto, Customer>, CustomerConverter>();
            serviceCollection.AddSingleton<IConverter<Customer, CustomerDto>, CustomerDtoConverter>();
            serviceCollection.AddSingleton<IConverter<NotificationDto, Notification>, NotificationConverter>();
            serviceCollection.AddSingleton<IConverter<Notification, NotificationDto>, NotificationDtoConverter>();

            // Register application lifecycle
            serviceCollection.AddSingleton<CustomerApplication>();
            serviceCollection.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<CustomerApplication>());

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
                var provider = serviceProvider.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
                var version = System.Reflection.Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion;

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = $"Customer Service version {version} - OpenAPI {description.ApiVersion}",
                        Description = $"Customer service API v{description.ApiVersion}",
                    });
                }

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
                if (System.IO.File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            // Add health checks
            serviceCollection.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>("database");
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

