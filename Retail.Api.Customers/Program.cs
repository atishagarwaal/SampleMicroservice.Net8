//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure services
builder.Services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
builder.Services.AddScoped(typeof(IUnitOfWork), typeof(EntityUnitOfWork));
builder.Services.AddScoped(typeof(ICustomerService), typeof(CustomerService));

// Add RabbitMQ from the common project
builder.Services.AddRabbitMQServices(builder.Configuration);

builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
builder.Services.AddScoped<IServiceInitializer, ServiceInitializer>();

builder.Services.AddAutoMapper(typeof(Program));

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

using (var scope = app.Services.CreateScope())
{
    var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
    await serviceInitializer.Initialize();

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
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

app.Run();