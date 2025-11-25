# Tutorial 001: Creating a New Service

This tutorial walks you through creating a new microservice from scratch, following the established patterns and architecture used in the Retail Microservices solution.

## Objectives

By the end of this tutorial, you will:
- Understand the Clean Architecture structure used in services
- Know how to set up a new service project
- Implement the Composition Root pattern
- Configure the Startup class
- Implement the Application Host pattern (IHostedService)
- Set up basic API controllers

## Prerequisites

- .NET 8 SDK installed
- Basic understanding of C# and ASP.NET Core
- IDE (Visual Studio, Rider, or VS Code)
- SQL Server (LocalDB or Docker)

## Step 1: Create Project Structure

### 1.1 Create Solution Folder

```bash
mkdir Retail.Inventory
cd Retail.Inventory
dotnet new sln -n Retail.Inventory
```

### 1.2 Create Clean Architecture Layers

Create the four layers following Clean Architecture:

```bash
# API Layer (Presentation)
dotnet new webapi -n Retail.Inventory.Service/src/CleanArchitecture.API -f net8.0

# Application Layer
dotnet new classlib -n Retail.Inventory.Service/src/CleanArchitecture.Application -f net8.0

# Domain Layer
dotnet new classlib -n Retail.Inventory.Service/src/CleanArchitecture.Domain -f net8.0

# Infrastructure Layer
dotnet new classlib -n Retail.Inventory.Service/src/CleanArchitecture.Infrastructure -f net8.0
```

### 1.3 Add Projects to Solution

```bash
dotnet sln add Retail.Inventory.Service/src/CleanArchitecture.API/CleanArchitecture.API.csproj
dotnet sln add Retail.Inventory.Service/src/CleanArchitecture.Application/CleanArchitecture.Application.csproj
dotnet sln add Retail.Inventory.Service/src/CleanArchitecture.Domain/CleanArchitecture.Domain.csproj
dotnet sln add Retail.Inventory.Service/src/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj
```

### 1.4 Set Up Project References

```bash
# API references Application
dotnet add Retail.Inventory.Service/src/CleanArchitecture.API/CleanArchitecture.API.csproj reference Retail.Inventory.Service/src/CleanArchitecture.Application/CleanArchitecture.Application.csproj

# Application references Domain
dotnet add Retail.Inventory.Service/src/CleanArchitecture.Application/CleanArchitecture.Application.csproj reference Retail.Inventory.Service/src/CleanArchitecture.Domain/CleanArchitecture.Domain.csproj

# Infrastructure references Application and Domain
dotnet add Retail.Inventory.Service/src/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj reference Retail.Inventory.Service/src/CleanArchitecture.Application/CleanArchitecture.Application.csproj
dotnet add Retail.Inventory.Service/src/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj reference Retail.Inventory.Service/src/CleanArchitecture.Domain/CleanArchitecture.Domain.csproj

# API references Infrastructure
dotnet add Retail.Inventory.Service/src/CleanArchitecture.API/CleanArchitecture.API.csproj reference Retail.Inventory.Service/src/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj
```

## Step 2: Create Application Layer Structure

### 2.1 Create Application Folder

```bash
mkdir Retail.Inventory.Service/Application
```

### 2.2 Create CompositionRoot.cs

Create `Retail.Inventory.Service/Application/CompositionRoot.cs`:

```csharp
namespace Retail.Inventory.Service.Application
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Retail.Inventory.Service.src.CleanArchitecture.Application.Services;
    using Retail.Inventory.Service.src.CleanArchitecture.Infrastructure.Data;
    using Retail.Inventory.Service.src.CleanArchitecture.Infrastructure.Repositories;

    /// <summary>
    /// Configures dependency injection for the service.
    /// </summary>
    public class CompositionRoot
    {
        /// <summary>
        /// Configures services for dependency injection.
        /// </summary>
        /// <param name="context">The host builder context.</param>
        /// <param name="services">The service collection.</param>
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            // Configuration
            services.Configure<DatabaseConnectionConfiguration>(
                context.Configuration.GetSection(nameof(DatabaseConnectionConfiguration)));

            // Database Context
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var config = context.Configuration.GetSection(nameof(DatabaseConnectionConfiguration))
                    .Get<DatabaseConnectionConfiguration>();
                options.UseSqlServer(config?.ConnectionString ?? throw new InvalidOperationException("Connection string not configured"));
            });

            // Repositories
            services.AddScoped<IInventoryRepository, InventoryRepository>();

            // Services
            services.AddScoped<IInventoryService, InventoryService>();

            // Application Infrastructure
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, InventoryApplication>();
        }

        /// <summary>
        /// Configures application configuration sources.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        public static void Configure(IConfigurationBuilder builder)
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
```

### 2.3 Create Startup.cs

Create `Retail.Inventory.Service/Application/Startup.cs`:

```csharp
namespace Retail.Inventory.Service.Application
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Retail.CommonLibrary.Middleware;

    /// <summary>
    /// Configures the application pipeline.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Configures services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHealthChecks();
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web host environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            
            // Global exception handling
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
            
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health/liveness");
                endpoints.MapHealthChecks("/health/readiness");
            });
        }
    }
}
```

### 2.4 Create Application Class

Create `Retail.Inventory.Service/Application/InventoryApplication.cs`:

```csharp
namespace Retail.Inventory.Service.Application
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Retail.CommonLibrary.Application;

    /// <summary>
    /// Application lifecycle management.
    /// </summary>
    public class InventoryApplication : IHostedService
    {
        private readonly ILogger<InventoryApplication> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryApplication"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public InventoryApplication(ILogger<InventoryApplication> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Inventory service starting...");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Inventory service stopping...");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes the background service.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
```

## Step 3: Create Program.cs

Update `Retail.Inventory.Service/src/CleanArchitecture.API/Program.cs`:

```csharp
namespace Retail.Inventory.Service.src.CleanArchitecture.API
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Retail.Inventory.Service.Application;

    /// <summary>
    /// Application entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(CompositionRoot.Configure)
                .ConfigureServices(CompositionRoot.ConfigureServices)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();

            host.Run();
        }
    }
}
```

## Step 4: Add Required Packages

Add packages to the appropriate projects:

```bash
# API Project
dotnet add Retail.Inventory.Service/src/CleanArchitecture.API/CleanArchitecture.API.csproj package Swashbuckle.AspNetCore
dotnet add Retail.Inventory.Service/src/CleanArchitecture.API/CleanArchitecture.API.csproj package Microsoft.AspNetCore.Diagnostics.HealthChecks

# Infrastructure Project
dotnet add Retail.Inventory.Service/src/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add Retail.Inventory.Service/src/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools

# Add CommonLibrary reference
dotnet add Retail.Inventory.Service/src/CleanArchitecture.API/CleanArchitecture.API.csproj reference ../../CommonLibrary/CommonLibrary.csproj
dotnet add Retail.Inventory.Service/Application/CompositionRoot.cs reference ../../CommonLibrary/CommonLibrary.csproj
```

## Step 5: Create Configuration

Create `Retail.Inventory.Service/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DatabaseConnectionConfiguration": {
    "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=RetailInventory;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

## Step 6: Create Basic Controller

Create `Retail.Inventory.Service/src/CleanArchitecture.API/Controllers/InventoryController.cs`:

```csharp
namespace Retail.Inventory.Service.src.CleanArchitecture.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Retail.Inventory.Service.src.CleanArchitecture.Application.Services;

    /// <summary>
    /// Inventory management controller.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryController"/> class.
        /// </summary>
        /// <param name="inventoryService">The inventory service.</param>
        /// <param name="logger">The logger.</param>
        public InventoryController(
            IInventoryService inventoryService,
            ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// Gets inventory by product ID.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <returns>Inventory information.</returns>
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetInventory(int productId)
        {
            var result = await _inventoryService.GetInventoryAsync(productId);
            
            if (result.IsFailure)
            {
                return Problem(
                    detail: result.Error,
                    statusCode: 404,
                    title: "Not Found");
            }

            return Ok(result.Value);
        }
    }
}
```

## Step 7: Build and Run

```bash
# Build the solution
dotnet build

# Run the service
dotnet run --project Retail.Inventory.Service/src/CleanArchitecture.API/CleanArchitecture.API.csproj
```

Visit `https://localhost:5001/swagger` to see the API documentation.

## Summary

You've successfully created a new microservice with:
- ✅ Clean Architecture structure (API, Application, Domain, Infrastructure)
- ✅ Composition Root pattern for dependency injection
- ✅ Startup class for middleware configuration
- ✅ Application Host pattern (IHostedService)
- ✅ Basic API controller
- ✅ Configuration management
- ✅ Health check endpoints

## Next Steps

- [Tutorial 002: Event-Driven Communication](002-Event-Driven-Communication.md) - Add messaging capabilities
- [Tutorial 003: Working with Contracts](003-Working-with-Contracts.md) - Generate and use AsyncAPI contracts
- [Tutorial 004: Health Checks and Observability](004-Health-Checks-Observability.md) - Add comprehensive observability

## Common Issues

**Issue**: Build errors about missing references
- **Solution**: Ensure all project references are added correctly

**Issue**: Configuration not loading
- **Solution**: Verify `appsettings.json` is in the correct location and `CompositionRoot.Configure` is called

**Issue**: Service won't start
- **Solution**: Check database connection string and ensure SQL Server is running

