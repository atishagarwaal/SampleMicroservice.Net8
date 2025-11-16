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
    using Microsoft.Extensions.Options;

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

            // Configure database connection
            serviceCollection.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var dbConfig = serviceProvider.GetRequiredService<IOptions<DatabaseConnectionConfiguration>>().Value;
                options.UseSqlServer(dbConfig.DefaultConnection);
            }, ServiceLifetime.Scoped);

            serviceCollection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            serviceCollection.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

            // Register MediatR with all relevant assemblies
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(CreateOrderCommand).Assembly,
                typeof(DeleteOrderCommand).Assembly,
                typeof(UpdateOrderCommand).Assembly)
            );

            serviceCollection.AddScoped<IEventHandler<InventoryErrorEvent>, InventoryErrorEventHandler>();
            serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

            // Register validators
            serviceCollection.AddScoped<IMessageValidator<OrderDto>, OrderDtoValidator>();
            serviceCollection.AddScoped<IMessageValidator<LineItemDto>, LineItemDtoValidator>();

            // Register converters
            serviceCollection.AddScoped<IConverter<LineItemDto, Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.LineItem>, LineItemConverter>();
            serviceCollection.AddScoped<IConverter<Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.LineItem, LineItemDto>, LineItemDtoConverter>();
            serviceCollection.AddScoped<IConverter<OrderDto, Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.Order>, OrderConverter>();
            serviceCollection.AddScoped<IConverter<Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.Order, OrderDto>, OrderDtoConverter>();

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
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Order", Version = "v1" });
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

