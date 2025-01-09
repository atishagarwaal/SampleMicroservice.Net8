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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Product", Version = "v1" });
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
