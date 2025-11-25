//-----------------------------------------------------------------------
// <copyright file="CompositionRoot.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Orders.Write.Application
{
    using CommonLibrary.Handlers;
    using InventoryErrorEventNameSpace;
    using MediatR;
    using MessagingInfrastructure.Service;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Converters;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
    using Retail.Orders.Write.src.CleanArchitecture.Application.EventHandlers;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Interfaces;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Service;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Validation;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;
    using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
    using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;
    using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
    using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Repositories;
    using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.UnitOfWork;
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
            // Application Infrastructure
            serviceCollection.AddSingleton<OrderWriteApplication>();
            serviceCollection.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<OrderWriteApplication>());
            serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

            // General Configuration
            serviceCollection.Configure<DatabaseConnectionConfiguration>(
                context.Configuration.GetSection("ConnectionStrings"));
            serviceCollection.Configure<MetricsConfiguration>(
                context.Configuration.GetSection(nameof(MetricsConfiguration)));

            // DataStore
            serviceCollection.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var dbConfig = serviceProvider.GetRequiredService<IOptions<DatabaseConnectionConfiguration>>().Value;
                options.UseSqlServer(dbConfig.DefaultConnection);
            }, ServiceLifetime.Scoped);
            serviceCollection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();

            // Domain Services
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(CreateOrderCommand).Assembly,
                typeof(DeleteOrderCommand).Assembly,
                typeof(UpdateOrderCommand).Assembly)
            );

            // Converters
            serviceCollection.AddSingleton<IConverter<LineItemDto, Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.LineItem>, LineItemConverter>();
            serviceCollection.AddSingleton<IConverter<Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.LineItem, LineItemDto>, LineItemDtoConverter>();
            serviceCollection.AddSingleton<IConverter<OrderDto, Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.Order>, OrderConverter>();
            serviceCollection.AddSingleton<IConverter<Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.Order, OrderDto>, OrderDtoConverter>();

            // Validators
            serviceCollection.AddScoped<IMessageValidator<OrderDto>, OrderDtoValidator>();
            serviceCollection.AddScoped<IMessageValidator<LineItemDto>, LineItemDtoValidator>();

            // Messaging
            serviceCollection.AddRabbitMQServices(context.Configuration);
            serviceCollection.AddSingleton<IRabbitMQTopologyManager, RabbitMQTopologyManager>();
            serviceCollection.AddScoped<IEventHandler<InventoryErrorEvent>, InventoryErrorEventHandler>();

            // API Infrastructure
            serviceCollection.AddOpenTelemetry(
                context.Configuration,
                serviceName: "Retail.Orders.Write",
                serviceVersion: "1.0.0");
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
                        Title = $"Order Write Service version {version} - OpenAPI {description.ApiVersion}",
                        Description = $"Order Write service API v{description.ApiVersion}",
                    });
                }

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });
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

