//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Retail.Api.Orders.Data;
using Retail.Api.Orders.Interface;
using Retail.Api.Orders.Repositories;
using Retail.Api.Orders.Service;
using Retail.Api.Orders.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient(typeof(IEntityRepository<>), typeof(EntityRepository<>));
builder.Services.AddTransient<IDapperRepository, DapperRepository>();
builder.Services.AddTransient<IEntityUnitOfWork, EntityUnitOfWork>();
builder.Services.AddTransient<IDapperUnitOfWork, DapperUnitOfWork>();

builder.Services.AddTransient<IOrderEntityRepository, OrderEntityRepository>();
builder.Services.AddTransient<IOrderDapperRepository, OrderDapperRepository>();
builder.Services.AddTransient<ILineItemEntityRepository, LineItemEntityRepository>();
builder.Services.AddTransient<ILineItemDapperRepository, LineItemDapperRepository>();
builder.Services.AddTransient<IOrderService, OrderService>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
