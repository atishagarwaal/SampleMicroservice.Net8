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
    using CommonLibrary.Telemetry;
    using Asp.Versioning.ApiExplorer;
    using Asp.Versioning;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Configuration for this service.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
                serviceName: "Retail.Orders.Write",
                serviceVersion: "1.0.0");

            // Register metrics service
            serviceCollection.AddSingleton<CommonLibrary.Telemetry.IMetricsService, CommonLibrary.Telemetry.MetricsService>();

            // Configure strongly-typed configuration classes
            serviceCollection.Configure<DatabaseConnectionConfiguration>(
                context.Configuration.GetSection("ConnectionStrings"));

            // Configure database connection
            serviceCollection.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var dbConfig = serviceProvider.GetRequiredService<IOptions<DatabaseConnectionConfiguration>>().Value;
                options.UseSqlServer(dbConfig.DefaultConnection);
            }, ServiceLifetime.Scoped);

            serviceCollection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register MediatR with all relevant assemblies
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(CreateOrderCommand).Assembly,
                typeof(DeleteOrderCommand).Assembly,
                typeof(UpdateOrderCommand).Assembly)
            );

            serviceCollection.AddScoped<IEventHandler<InventoryErrorEvent>, InventoryErrorEventHandler>();
            serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

            // Register application lifecycle
            serviceCollection.AddSingleton<OrderWriteApplication>();
            serviceCollection.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<OrderWriteApplication>());

            // Register validators
            serviceCollection.AddScoped<IMessageValidator<OrderDto>, OrderDtoValidator>();
            serviceCollection.AddScoped<IMessageValidator<LineItemDto>, LineItemDtoValidator>();

            // Register converters
            serviceCollection.AddSingleton<IConverter<LineItemDto, Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.LineItem>, LineItemConverter>();
            serviceCollection.AddSingleton<IConverter<Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.LineItem, LineItemDto>, LineItemDtoConverter>();
            serviceCollection.AddSingleton<IConverter<OrderDto, Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.Order>, OrderConverter>();
            serviceCollection.AddSingleton<IConverter<Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.Order, OrderDto>, OrderDtoConverter>();

            // Add RabbitMQ from the common project
            serviceCollection.AddRabbitMQServices(context.Configuration);

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

