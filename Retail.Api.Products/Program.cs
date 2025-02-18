//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingInfrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Service;
using Retail.Api.Products.src.CleanArchitecture.Application.EventHandlers;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Service;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

// Configure services
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
builder.Services.AddScoped(typeof(IProductService), typeof(ProductService));

// Add RabbitMQ from the common project
builder.Services.AddRabbitMQServices(builder.Configuration);

builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
builder.Services.AddScoped<IServiceInitializer, ServiceInitializer>();

builder.Services.AddAutoMapper(typeof(Program));

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
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Product", Version = "v1" });
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
