//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
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
using Retail.Orders.Read.src.CleanArchitecture.Application.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Application.Service;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Repositories;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.UnitOfWork;
using Retail.Orders.Read.src.CleanArchitecture.Application.Queries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

// Register MediatR with all relevant assemblies
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(GetAllOrdersQuery).Assembly,
    typeof(GetOrderByIdQuery).Assembly
));

builder.Services.AddScoped<IServiceInitializer, ServiceInitializer>();

// Add RabbitMQ from the common project
builder.Services.AddRabbitMQServices(builder.Configuration);

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
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Order", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
    await serviceInitializer.Initialize();

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreatedAsync();
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
