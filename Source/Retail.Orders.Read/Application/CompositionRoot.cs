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
            // Configure strongly-typed configuration classes
            serviceCollection.Configure<DatabaseConnectionConfiguration>(
                context.Configuration.GetSection("ConnectionStrings"));
            serviceCollection.Configure<MongoDBSettings>(
                context.Configuration.GetSection(nameof(MongoDBSettings)));

            // Configure MongoDB connection
            serviceCollection.AddScoped<ApplicationDbContext>();
            serviceCollection.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

            // Register MediatR with all relevant assemblies
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(GetAllOrdersQuery).Assembly,
                typeof(GetOrderByIdQuery).Assembly
            ));

            // Register event handlers with proper logging
            serviceCollection.AddScoped<IEventHandler<InventoryUpdatedEvent>, InventoryUpdatedEventHandler>();
            serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

            // Register converters
            serviceCollection.AddScoped<IConverter<LineItemDto, Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.LineItem>, LineItemConverter>();
            serviceCollection.AddScoped<IConverter<Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.LineItem, LineItemDto>, LineItemDtoConverter>();
            serviceCollection.AddScoped<IConverter<OrderDto, Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.Order>, OrderConverter>();
            serviceCollection.AddScoped<IConverter<Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.Order, OrderDto>, OrderDtoConverter>();

            // Add RabbitMQ from the common project
            serviceCollection.AddRabbitMQServices(context.Configuration);

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
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Order Read Service", Version = "v1" });
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

