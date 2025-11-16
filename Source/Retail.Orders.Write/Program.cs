//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using MediatR;
using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingInfrastructure.Service;
using MessagingLibrary.Interface;
using MessagingLibrary.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Retail.Orders.Write.src.CleanArchitecture.Application.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Service;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Repositories;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.UnitOfWork;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.EventHandlers;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;
using InventoryErrorEventNameSpace;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

// Register MediatR with all relevant assemblies
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(CreateOrderCommand).Assembly,
    typeof(DeleteOrderCommand).Assembly,
    typeof(UpdateOrderCommand).Assembly)
);

builder.Services.AddScoped<IEventHandler<InventoryErrorEvent>, InventoryErrorEventHandler>();
builder.Services.AddScoped<IServiceInitializer, ServiceInitializer>();

// Register validators
builder.Services.AddScoped<IMessageValidator<OrderDto>, OrderDtoValidator>();
builder.Services.AddScoped<IMessageValidator<LineItemDto>, LineItemDtoValidator>();

// Register converters
builder.Services.AddScoped<IConverter<LineItemDto, Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.LineItem>, LineItemConverter>();
builder.Services.AddScoped<IConverter<Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.LineItem, LineItemDto>, LineItemDtoConverter>();
builder.Services.AddScoped<IConverter<OrderDto, Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.Order>, OrderConverter>();
builder.Services.AddScoped<IConverter<Retail.Orders.Write.src.CleanArchitecture.Domain.Entities.Order, OrderDto>, OrderDtoConverter>();

// Add RabbitMQ from the common project
builder.Services.AddRabbitMQServices(builder.Configuration);

builder.Services.AddControllers();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Order", Version = "v1" });
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting Order Write Service");

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

    logger.LogInformation("Order Write Service started successfully");
    app.Run();
}
catch (Exception ex)
{
    logger.LogError(ex, "Error starting Order Write Service");
    throw;
}
