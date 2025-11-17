//-----------------------------------------------------------------------
// <copyright file="CompositionRoot.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Retail.Api.Products.Application
{
    using CommonLibrary.Handlers;
    using MessagingInfrastructure.Service;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using OrderCreatedEventNameSpace;
    using Retail.Api.Products.src.CleanArchitecture.Application.Converters;
    using Retail.Api.Products.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Products.src.CleanArchitecture.Application.EventHandlers;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Service;
    using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
    using Retail.Api.Products.src.CleanArchitecture.Application.Service;
    using Retail.Api.Products.src.CleanArchitecture.Application.Validation;
    using Retail.Api.Products.src.CleanArchitecture.Application.Validation.Interfaces;
    using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;
    using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
    using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories;
    using Retail.Api.Products.src.CleanArchitecture.Infrastructure.UnitOfWork;
    using CommonLibrary.Configuration;
    using CommonLibrary.Telemetry;
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
            // Configure OpenTelemetry for observability
            serviceCollection.AddOpenTelemetry(
                context.Configuration,
                serviceName: "Retail.Products",
                serviceVersion: "1.0.0");

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
            serviceCollection.AddScoped<IProductService, ProductService>();

            // Add RabbitMQ from the common project
            serviceCollection.AddRabbitMQServices(context.Configuration);

            serviceCollection.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
            serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

            // Register validators
            serviceCollection.AddScoped<IMessageValidator<SkuDto>, SkuDtoValidator>();

            // Register converters
            serviceCollection.AddScoped<IConverter<SkuDto, Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>, SkuConverter>();
            serviceCollection.AddScoped<IConverter<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku, SkuDto>, SkuDtoConverter>();

            // Register application lifecycle
            serviceCollection.AddSingleton<ProductApplication>();
            serviceCollection.AddSingleton<CommonLibrary.Application.IApplication>(sp => sp.GetRequiredService<ProductApplication>());
            serviceCollection.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<ProductApplication>());

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
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Product", Version = "v1" });
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

