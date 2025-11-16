//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using CommonLibrary.Handlers;
using MessagingInfrastructure.Service;
using MessagingLibrary.Interface;
using MessagingLibrary.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Retail.Api.Customers.src.CleanArchitecture.Application.EventHandlers;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Service;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Repositories;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.UnitOfWork;
using CommonLibrary.MessageContract;
using InventoryUpdatedEventNameSpace;
using Retail.Api.Customers.src.CleanArchitecture.Application.Validation;
using Retail.Api.Customers.src.CleanArchitecture.Application.Validation.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Application.Converters;
using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

// Configure services
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
builder.Services.AddScoped(typeof(ICustomerService), typeof(CustomerService));
builder.Services.AddScoped(typeof(INotificationRepository), typeof(NotificationRepository));

// Add RabbitMQ from the common project
builder.Services.AddRabbitMQServices(builder.Configuration);

builder.Services.AddScoped<IEventHandler<InventoryUpdatedEvent>, InventoryUpdatedEventHandler>();
builder.Services.AddScoped<IServiceInitializer, ServiceInitializer>();

// Register validators
builder.Services.AddScoped<IMessageValidator<CustomerDto>, CustomerDtoValidator>();

// Register converters
builder.Services.AddScoped<IConverter<CustomerDto, Customer>, CustomerConverter>();
builder.Services.AddScoped<IConverter<Customer, CustomerDto>, CustomerDtoConverter>();
builder.Services.AddScoped<IConverter<NotificationDto, Notification>, NotificationConverter>();
builder.Services.AddScoped<IConverter<Notification, NotificationDto>, NotificationDtoConverter>();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Customer", Version= "v1" });
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting Customer Service");

    using (var scope = app.Services.CreateScope())
    {
        logger.LogInformation("Initializing service subscriptions");
        var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
        await serviceInitializer.Initialize();

        logger.LogInformation("Ensuring database is created");
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("Database initialization completed");
    }

    if (app.Environment.IsDevelopment())
    {
        logger.LogInformation("Configuring Swagger for development environment");
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        });
    }

    // Configure the HTTP request pipeline.
    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    logger.LogInformation("Customer Service started successfully");
    app.Run();
}
catch (Exception ex)
{
    logger.LogError(ex, "Error starting Customer Service");
    throw;
}