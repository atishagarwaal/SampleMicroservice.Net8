using Microsoft.AspNetCore.Mvc;
using Retail.BFFWeb.Api.Configurations;
using Retail.BFFWeb.Api.Interface;
using Retail.BFFWeb.Api.Provider;
using System.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddSingleton(typeof(ICustomerProvider), typeof(CustomerProvider));
builder.Services.AddSingleton(typeof(IOrderProvider), typeof(OrderProvider));
builder.Services.AddSingleton(typeof(IProductProvider), typeof(ProductProvider));

builder.Services.Configure<CustomerServiceConfig>(builder.Configuration.GetSection("CustomerServiceConfig"));
builder.Services.Configure<OrderServiceConfig>(builder.Configuration.GetSection("OrderServiceConfig"));
builder.Services.Configure<ProductServiceConfig>(builder.Configuration.GetSection("ProductServiceConfig"));

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
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Aggregated Data", Version = "v1" });
});

var app = builder.Build();

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
