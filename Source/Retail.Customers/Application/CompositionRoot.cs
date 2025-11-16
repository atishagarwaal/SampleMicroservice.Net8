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

            // Configure services
            serviceCollection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
            serviceCollection.AddScoped<ICustomerService, CustomerService>();
            serviceCollection.AddScoped<INotificationRepository, NotificationRepository>();

            // Add RabbitMQ from the common project
            serviceCollection.AddRabbitMQServices(context.Configuration);

            serviceCollection.AddScoped<IEventHandler<InventoryUpdatedEvent>, InventoryUpdatedEventHandler>();
            serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

            // Register validators
            serviceCollection.AddScoped<IMessageValidator<CustomerDto>, CustomerDtoValidator>();

            // Register converters
            serviceCollection.AddScoped<IConverter<CustomerDto, Customer>, CustomerConverter>();
            serviceCollection.AddScoped<IConverter<Customer, CustomerDto>, CustomerDtoConverter>();
            serviceCollection.AddScoped<IConverter<NotificationDto, Notification>, NotificationConverter>();
            serviceCollection.AddScoped<IConverter<Notification, NotificationDto>, NotificationDtoConverter>();

            // Register application lifecycle
            serviceCollection.AddSingleton<CustomerApplication>();
            serviceCollection.AddSingleton<CommonLibrary.Application.IApplication>(sp => sp.GetRequiredService<CustomerApplication>());
            serviceCollection.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<CustomerApplication>());

            // Add API versioning
            serviceCollection.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            serviceCollection.AddControllers();

            serviceCollection.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Customer", Version = "v1" });
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

