//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Retail.Api.Customers.Data;
using Retail.Api.Customers.DefaultInterface;
using Retail.Api.Customers.DefaultRepositories;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Repositories;
using Retail.Api.Customers.Service;
using Retail.Api.Customers.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<DapperContext>();
builder.Services.AddTransient(typeof(IRepository<>), typeof(EntityRepository<>));
builder.Services.AddTransient(typeof(IUnitOfWork), typeof(EntityUnitOfWork));
////builder.Services.AddTransient(typeof(IUnitOfWork), typeof(DapperUnitOfWork));
builder.Services.AddTransient(typeof(ICustomerService), typeof(CustomerService));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Customer", Version= "v1" });
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