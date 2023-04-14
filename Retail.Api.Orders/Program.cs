//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Retail.Api.Orders.Data;
using Retail.Api.Orders.DefaultInterface;
using Retail.Api.Orders.DefaultRepositories;
using Retail.Api.Orders.Interface;
using Retail.Api.Orders.Repositories;
using Retail.Api.Orders.Service;
using Retail.Api.Orders.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<DapperContext>();
builder.Services.AddTransient(typeof(IEntityRepository<>), typeof(EntityRepository<>));
builder.Services.AddTransient(typeof(IDapperRepository), typeof(DapperRepository));
builder.Services.AddTransient(typeof(IEntityUnitOfWork), typeof(EntityUnitOfWork));
builder.Services.AddTransient(typeof(IDapperUnitOfWork), typeof(DapperUnitOfWork));

builder.Services.AddTransient(typeof(IOrderEntityRepository), typeof(OrderEntityRepository));
builder.Services.AddTransient(typeof(IOrderDapperRepository), typeof(OrderDapperRepository));
builder.Services.AddTransient(typeof(ILineItemEntityRepository), typeof(LineItemEntityRepository));
builder.Services.AddTransient(typeof(ILineItemDapperRepository), typeof(LineItemDapperRepository));
builder.Services.AddTransient(typeof(IOrderService), typeof(OrderService));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Order", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
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
