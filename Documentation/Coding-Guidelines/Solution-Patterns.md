# Solution-Specific Patterns

## Overview

This document documents the standard patterns and practices used across all services in this solution. Follow these patterns consistently to maintain architectural integrity and developer experience.

## 🧱 Composition Root Pattern

All services use a `CompositionRoot` **static class** to centralize dependency injection configuration. This pattern separates service registration from web host configuration.

**Structure:**
```csharp
namespace Retail.Api.Customers.Application
{
    using System.Diagnostics.CodeAnalysis;
    using CommonLibrary.Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Configuration for this service.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class CompositionRoot
    {
        /// <summary>
        /// Configures application service with a dependency injection container.
        /// </summary>
        /// <param name="context">The application's builder context.</param>
        /// <param name="serviceCollection">Service collection to register services to.</param>
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
        {
            // Application Infrastructure
            serviceCollection.AddSingleton<CustomerApplication>();
            serviceCollection.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<CustomerApplication>());
            serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

            // General Configuration
            serviceCollection.Configure<DatabaseConnectionConfiguration>(
                context.Configuration.GetSection("ConnectionStrings"));
            serviceCollection.Configure<MetricsConfiguration>(
                context.Configuration.GetSection(nameof(MetricsConfiguration)));

            // DataStore
            serviceCollection.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var dbConfig = serviceProvider.GetRequiredService<IOptions<DatabaseConnectionConfiguration>>().Value;
                options.UseSqlServer(dbConfig.DefaultConnection);
            }, ServiceLifetime.Scoped);
            serviceCollection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();

            // Domain Services
            serviceCollection.AddScoped<ICustomerService, CustomerService>();

            // Converters (Singleton lifetime for stateless converters)
            serviceCollection.AddSingleton<IConverter<CustomerDto, Customer>, CustomerConverter>();
            serviceCollection.AddSingleton<IConverter<Customer, CustomerDto>, CustomerDtoConverter>();

            // Validators
            serviceCollection.AddScoped<IMessageValidator<CustomerDto>, CustomerDtoValidator>();

            // Messaging
            serviceCollection.AddRabbitMQServices(context.Configuration);
            serviceCollection.AddSingleton<IRabbitMQTopologyManager, RabbitMQTopologyManager>();
            serviceCollection.AddScoped<IEventHandler<InventoryUpdatedEvent>, InventoryUpdatedEventHandler>();

            // API Infrastructure
            serviceCollection.AddOpenTelemetry(
                context.Configuration,
                serviceName: "Retail.Customers",
                serviceVersion: "1.0.0");
            serviceCollection.AddSingleton<CommonLibrary.Telemetry.IMetricsService>(services =>
            {
                var metricsConfiguration = services.GetRequiredService<IOptions<MetricsConfiguration>>();
                if (metricsConfiguration.Value.Enabled)
                {
                    return new CommonLibrary.Telemetry.MetricsService();
                }
                else
                {
                    return new CommonLibrary.Telemetry.EmptyMetricsService();
                }
            });
            serviceCollection.AddApiVersioning(/* ... */);
            serviceCollection.AddEndpointsApiExplorer();
            serviceCollection.AddControllers();
            serviceCollection.AddSwaggerGen(/* ... */);
            serviceCollection.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>("database");
        }

        /// <summary>
        /// Configures the application configuration sources.
        /// </summary>
        /// <param name="configurationBuilder">Configuration builder.</param>
        public static void Configure(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            configurationBuilder.AddEnvironmentVariables();
        }
    }
}
```

**Key Points:**
- ✅ **Static class** - Cannot be instantiated, contains only static methods
- ✅ Use static methods for configuration
- ✅ Separate `Configure` for configuration sources and `ConfigureServices` for DI
- ✅ **Organized service registration** - Group registrations by category (see Service Registration Organization below)
- ✅ Use strongly-typed configuration classes (see below)
- ✅ Register OpenTelemetry and metrics early for observability
- ✅ Use `[ExcludeFromCodeCoverage]` attribute

---

## 🚀 Startup Class Pattern

Each service has a `Startup` class that configures the HTTP pipeline. Service registration is handled in `CompositionRoot`, while `Startup` focuses on middleware pipeline.

**Structure:**
```csharp
namespace Retail.Api.Customers.Application
{
    /// <summary>
    /// Configures web host.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }

        /// <summary>
        /// Register services into the <see cref="IServiceCollection" />.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // Services are configured in CompositionRoot.ConfigureServices
            // This method can be used for additional web-specific configurations if needed
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="webApplicationBuilder">The application builder.</param>
        /// <param name="webEnvironment">The web hosting environment.</param>
        public static void Configure(IApplicationBuilder webApplicationBuilder, IWebHostEnvironment webEnvironment)
        {
            webEnvironment.ApplicationName = typeof(Startup).Assembly.GetName().Name;

            // Get metrics configuration
            var metricsConfig = webApplicationBuilder.ApplicationServices.GetRequiredService<IOptions<MetricsConfiguration>>().Value;

            // Register global exception handling middleware early in the pipeline
            webApplicationBuilder.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            if (webEnvironment.IsDevelopment())
            {
                webApplicationBuilder.UseDeveloperExceptionPage();
                webApplicationBuilder.UseSwagger();
                webApplicationBuilder.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
            }

            webApplicationBuilder.UseHttpsRedirection();
            webApplicationBuilder.UseRouting();
            
            // Conditionally collect HTTP request metrics for Prometheus
            if (metricsConfig.Enabled)
            {
                webApplicationBuilder.UseHttpMetrics();
            }
            
            webApplicationBuilder.UseAuthorization();

            webApplicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                // Conditionally expose Prometheus metrics endpoint
                if (metricsConfig.Enabled)
                {
                    endpoints.MapMetrics();
                }
                
                // Liveness endpoint - indicates the service is running
                endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
                {
                    Predicate = _ => false
                });
                
                // Readiness endpoint - indicates the service is ready to accept traffic
                endpoints.MapHealthChecks("/health/readiness", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("database")
                });
            });
        }
    }
}
```

**Middleware Order:**
1. Global Exception Handler (first)
2. Developer Exception Page (Development only)
3. Swagger (Development only)
4. HTTPS Redirection
5. Routing
6. HTTP Metrics (Prometheus)
7. Authorization
8. Endpoints (Controllers, Metrics, Health Checks)

---

## 🎯 Application Host Pattern (IApplication)

All services implement `IApplication` interface for explicit lifecycle management. This pattern provides a consistent way to handle startup and shutdown logic.

**Interface:**
```csharp
namespace CommonLibrary.Application
{
    /// <summary>
    /// Defines the contract for application lifecycle management.
    /// </summary>
    public interface IApplication
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
```

**Implementation:**
```csharp
namespace Retail.Api.Customers.Application
{
    /// <summary>
    /// Represents the Customer microservice application lifecycle.
    /// </summary>
    public class CustomerApplication : IApplication, IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CustomerApplication> _logger;

        public CustomerApplication(
            IServiceProvider serviceProvider,
            ILogger<CustomerApplication> logger)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting Customer Service");

            using (var scope = this._serviceProvider.CreateScope())
            {
                // Initialize service subscriptions (messaging, etc.)
                var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
                await serviceInitializer.Initialize();

                // Ensure database is created
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.EnsureCreatedAsync(cancellationToken);
            }

            this._logger.LogInformation("Customer Service started successfully");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Stopping Customer Service");
            return Task.CompletedTask;
        }
    }
}
```

**Registration:**
```csharp
// In CompositionRoot.ConfigureServices
serviceCollection.AddSingleton<CustomerApplication>();
serviceCollection.AddSingleton<IApplication>(sp => sp.GetRequiredService<CustomerApplication>());
serviceCollection.AddSingleton<IHostedService>(sp => sp.GetRequiredService<CustomerApplication>());
```

**Usage in Program.cs:**
```csharp
var application = host.Services.GetRequiredService<IApplication>();
await application.StartAsync(CancellationToken.None);
await host.RunAsync();
```

**Key Points:**
- ✅ Implement both `IApplication` and `IHostedService`
- ✅ Use scoped services for initialization (create scope)
- ✅ Log lifecycle events
- ✅ Handle graceful shutdown in `StopAsync`

---

## ⚙️ Strongly-Typed Configuration

Use strongly-typed configuration classes instead of accessing `IConfiguration` directly. This provides compile-time safety and IntelliSense support.

**Configuration Class:**
```csharp
namespace CommonLibrary.Configuration
{
    /// <summary>
    /// Configuration for database connection strings.
    /// </summary>
    public class DatabaseConnectionConfiguration
    {
        /// <summary>
        /// Gets or sets the default connection string.
        /// </summary>
        public string DefaultConnection { get; set; } = string.Empty;
    }
}
```

**Registration:**
```csharp
// In CompositionRoot.ConfigureServices
serviceCollection.Configure<DatabaseConnectionConfiguration>(
    context.Configuration.GetSection("ConnectionStrings"));
```

**Usage:**
```csharp
public class MyService
{
    private readonly DatabaseConnectionConfiguration _dbConfig;

    public MyService(IOptions<DatabaseConnectionConfiguration> dbConfig)
    {
        _dbConfig = dbConfig.Value;
    }

    public void DoWork()
    {
        var connectionString = _dbConfig.DefaultConnection;
        // Use connection string...
    }
}
```

**Key Points:**
- ✅ Create configuration classes in `CommonLibrary.Configuration`
- ✅ Use `IOptions<T>` or `IOptionsSnapshot<T>` for injection
- ✅ Map configuration sections explicitly
- ✅ Provide default values where appropriate

---

## 🛡️ Global Exception Handling Middleware

All services use `GlobalExceptionHandlerMiddleware` to catch unhandled exceptions and return consistent error responses (ProblemDetails format).

**Registration:**
```csharp
// In Startup.Configure - register early in pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

**Behavior:**
- Catches all unhandled exceptions
- Returns ProblemDetails-compliant JSON responses
- Maps exception types to appropriate HTTP status codes
- Includes trace ID for correlation
- Logs exceptions with context

**Exception Mapping:**
- `ArgumentNullException` / `ArgumentException` → 400 Bad Request
- `UnauthorizedAccessException` → 401 Unauthorized
- `KeyNotFoundException` → 404 Not Found
- `NotImplementedException` → 501 Not Implemented
- All others → 500 Internal Server Error

**Key Points:**
- ✅ Register as first middleware in pipeline
- ✅ Never catch exceptions in middleware (let it bubble up)
- ✅ Use specific exception types for better error responses

---

## 📝 Program.cs Pattern

All services follow a consistent `Program.cs` structure that builds the host, configures services, and starts the application.

**Structure:**
```csharp
namespace Retail.Api.Customers
{
    /// <summary>
    /// Contains the main entry point of the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(CompositionRoot.Configure)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(CompositionRoot.ConfigureServices)
                .Build();

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Program");

            try
            {
                var application = host.Services.GetRequiredService<IApplication>();
                await application.StartAsync(CancellationToken.None);

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Application terminated unexpectedly");
                throw;
            }
        }
    }
}
```

**Key Points:**
- ✅ Use `Host.CreateDefaultBuilder` for standard configuration
- ✅ Configure host configuration before web host defaults
- ✅ Use `CompositionRoot` for all configuration
- ✅ Start `IHostedService` (Application class) before running host
- ✅ Log critical exceptions before termination

---

## 📋 Service Registration Organization

Service registrations in `CompositionRoot.ConfigureServices` should be organized into clear, logical categories with comments. This improves readability and maintainability.

**Organization Pattern:**
```csharp
public static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
{
    // 1. Application Infrastructure
    serviceCollection.AddSingleton<CustomerApplication>();
    serviceCollection.AddSingleton<IHostedService>(sp => sp.GetRequiredService<CustomerApplication>());
    serviceCollection.AddScoped<IServiceInitializer, ServiceInitializer>();

    // 2. General Configuration
    serviceCollection.Configure<DatabaseConnectionConfiguration>(
        context.Configuration.GetSection("ConnectionStrings"));
    serviceCollection.Configure<MetricsConfiguration>(
        context.Configuration.GetSection(nameof(MetricsConfiguration)));

    // 3. DataStore
    serviceCollection.AddDbContext<ApplicationDbContext>(/* ... */);
    serviceCollection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();

    // 4. Domain Services
    serviceCollection.AddScoped<ICustomerService, CustomerService>();

    // 5. Converters
    serviceCollection.AddSingleton<IConverter<CustomerDto, Customer>, CustomerConverter>();
    serviceCollection.AddSingleton<IConverter<Customer, CustomerDto>, CustomerDtoConverter>();

    // 6. Validators
    serviceCollection.AddScoped<IMessageValidator<CustomerDto>, CustomerDtoValidator>();

    // 7. Messaging
    serviceCollection.AddRabbitMQServices(context.Configuration);
    serviceCollection.AddSingleton<IRabbitMQTopologyManager, RabbitMQTopologyManager>();
    serviceCollection.AddScoped<IEventHandler<InventoryUpdatedEvent>, InventoryUpdatedEventHandler>();

    // 8. API Infrastructure
    serviceCollection.AddOpenTelemetry(/* ... */);
    serviceCollection.AddSingleton<IMetricsService>(/* ... */);
    serviceCollection.AddApiVersioning(/* ... */);
    serviceCollection.AddEndpointsApiExplorer();
    serviceCollection.AddControllers();
    serviceCollection.AddSwaggerGen(/* ... */);
    serviceCollection.AddHealthChecks()/* ... */;
}
```

**Key Points:**
- ✅ Group related registrations together
- ✅ Use clear section comments
- ✅ Follow consistent ordering across all services
- ✅ Register infrastructure services last (OpenTelemetry, Metrics, API versioning, Swagger)

---

## 🔄 IConverter<TFrom, TTo> Pattern

Services use a custom `IConverter<TFrom, TTo>` interface for type-safe data transformation. This pattern provides compile-time safety and better testability than reflection-based mapping libraries.

**Interface:**
```csharp
namespace CommonLibrary.Application
{
    /// <summary>
    /// Defines a converter that transforms objects from one type to another.
    /// </summary>
    /// <typeparam name="TFrom">The source type.</typeparam>
    /// <typeparam name="TTo">The target type.</typeparam>
    public interface IConverter<in TFrom, out TTo>
    {
        /// <summary>
        /// Converts the specified source object to the target type.
        /// </summary>
        /// <param name="source">The source object to convert.</param>
        /// <returns>The converted object.</returns>
        TTo Convert(TFrom source);
    }
}
```

**Implementation Example:**
```csharp
namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Dto = Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts CustomerDto to Customer domain entity.
    /// </summary>
    public class CustomerConverter : IConverter<Dto.CustomerDto, DomainEntities.Customer>
    {
        /// <summary>
        /// Converts CustomerDto to Customer.
        /// </summary>
        public DomainEntities.Customer Convert(Dto.CustomerDto source)
        {
            if (source == null)
            {
                return null;
            }

            return new DomainEntities.Customer
            {
                Id = source.Id,
                Name = source.Name,
                Email = source.Email
            };
        }
    }
}
```

**Registration:**
```csharp
// In CompositionRoot.ConfigureServices
// Converters (Singleton lifetime for stateless converters)
serviceCollection.AddSingleton<IConverter<CustomerDto, Customer>, CustomerConverter>();
serviceCollection.AddSingleton<IConverter<Customer, CustomerDto>, CustomerDtoConverter>();
```

**Key Points:**
- ✅ **Singleton lifetime** - Converters are stateless and thread-safe, register as Singleton for performance
- ✅ **Type safety** - Compile-time checking prevents runtime mapping errors
- ✅ **Testability** - Easy to mock and test converters independently
- ✅ **Using aliases** - Use namespace aliases for long namespace names (see Using Aliases Pattern below)
- ✅ **Explicit dependencies** - Clear what conversions are needed
- ✅ **Composability** - Converters can depend on other converters via DI

---

## 📝 Using Aliases Pattern

Use namespace aliases (`using Alias = Full.Namespace.Name;`) to improve readability when working with long namespace names, especially in converter files.

**Example:**
```csharp
namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Dto = Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts CustomerDto to Customer domain entity.
    /// </summary>
    public class CustomerConverter : IConverter<Dto.CustomerDto, DomainEntities.Customer>
    {
        public DomainEntities.Customer Convert(Dto.CustomerDto source)
        {
            return new DomainEntities.Customer
            {
                Id = source.Id,
                Name = source.Name
            };
        }
    }
}
```

**Key Points:**
- ✅ Use aliases for frequently used long namespaces
- ✅ Common aliases: `Dto` for DTO namespaces, `DomainEntities` for Domain entity namespaces
- ✅ Improves code readability and reduces line length
- ✅ Use consistently across all converter files

---

## 🗄️ Repository and Unit of Work Pattern

Services use a generic repository pattern with Unit of Work for data access abstraction.

**Registration:**
```csharp
// In CompositionRoot.ConfigureServices
serviceCollection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
```

**Usage:**
```csharp
public class CustomerService
{
    private readonly IGenericRepository<Customer> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(
        IGenericRepository<Customer> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        await _repository.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();
        return customer;
    }
}
```

**Key Points:**
- ✅ Use generic repository for common CRUD operations
- ✅ Use Unit of Work for transaction management
- ✅ Register as scoped services (one per request)
- ✅ Create specific repositories for complex queries

---

## 🏥 Health Checks Pattern

All services implement separate liveness and readiness health checks for Kubernetes orchestration.

**Registration:**
```csharp
// In CompositionRoot.ConfigureServices
serviceCollection.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");
```

**Endpoints:**
```csharp
// In Startup.Configure
// Liveness - service is running
endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
{
    Predicate = _ => false  // No checks, just returns 200 if service is up
});

// Readiness - service is ready to accept traffic
endpoints.MapHealthChecks("/health/readiness", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database")  // Check dependencies
});
```

**Key Points:**
- ✅ Liveness: Simple check that service is running (no dependencies)
- ✅ Readiness: Check critical dependencies (database, message broker)
- ✅ Use tags to group health checks
- ✅ Kubernetes uses these endpoints for pod lifecycle management

---

## 📦 Build Configuration Pattern

All projects inherit from `Directory.Build.props` and `Build/Common.props` for centralized build properties. Test projects additionally inherit from `Build/Tests.Common.props` for test-specific configuration.

### Production Projects

**Directory.Build.props:**
```xml
<Project>
  <Import Project="Build\Common.props" />
</Project>
```

**Build/Common.props:**
```xml
<Project>
  <PropertyGroup Label="Package Information Properties">
    <Copyright>Copyright (c) Your Company, LLC. All rights reserved.</Copyright>
    <Authors>Your Company</Authors>
  </PropertyGroup>

  <PropertyGroup Label="Build Properties">
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <PropertyGroup Label="Code Analysis Properties">
    <CodeAnalysisRuleSet>$(SolutionDir)\Build\Code Analysis\Code Analysis Rules.ruleset</CodeAnalysisRuleSet>
  </PropertyGroup>
</Project>
```

### Test Projects

**Tests/Directory.Build.props:**
```xml
<Project>
  <!-- Imports Tests.Common.props which contains test-specific properties -->
  <Import Project="$(MSBuildThisFileDirectory)..\Build\Tests.Common.props" 
          Condition="Exists('$(MSBuildThisFileDirectory)..\Build\Tests.Common.props')" />
</Project>
```

**Build/Tests.Common.props:**
```xml
<Project>
  <PropertyGroup Label="Test Project Properties">
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup Label="Code Analysis Packages">
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <PropertyGroup Label="Code Analysis Warning Suppressions">
    <!-- Comprehensive suppressions for test projects -->
    <NoWarn Label="Underscores">$(NoWarn);CA1707</NoWarn>
    <NoWarn Label="Trailing spaces">$(NoWarn);SA1028</NoWarn>
    <NoWarn Label="Using statements order">$(NoWarn);SA1210</NoWarn>
    <NoWarn Label="Constant field location">$(NoWarn);SA1203</NoWarn>
    <NoWarn Label="Single type in a file">$(NoWarn);SA1402</NoWarn>
    <NoWarn Label="Missing or misformatted Documentation">$(NoWarn);CS1591;SA1600;SA1636;SA1633;SA1624</NoWarn>
    <NoWarn Label="Missing AttributeUsageAttribute">$(NoWarn);CA1018</NoWarn>
    <NoWarn Label="Tuple Parenthesis Spacing">$(NoWarn);SA1008;SA1009</NoWarn>
    <NoWarn Label="Closing Parenthesis should be on same line">$(NoWarn);SA1111</NoWarn>
    <NoWarn Label="Repeated statement">$(NoWarn);S3358</NoWarn>
    <NoWarn Label="NuGet restore with HTTP">$(NoWarn);NU1803</NoWarn>
    <NoWarn Label="Possible null reference">$(NoWarn);CS8602;CS8603;CS8604;CS8620;CS8625;CS8632</NoWarn>
  </PropertyGroup>
</Project>
```

**Key Points:**
- ✅ Centralize build properties in `Common.props` for production projects
- ✅ Centralize test configuration in `Tests.Common.props` for test projects
- ✅ Enable XML documentation generation
- ✅ Use consistent code analysis rules
- ✅ Apply to all projects via `Directory.Build.props`
- ✅ Test projects inherit both `Common.props` and `Tests.Common.props`
- ✅ Don't duplicate suppressions in individual test projects

---

## 🐰 RabbitMQ Topology Manager Pattern

RabbitMQ topology initialization (exchanges, queues, bindings) is extracted into a dedicated, testable class `IRabbitMQTopologyManager`. This pattern improves modularity and testability.

**Interface:**
```csharp
namespace CommonLibrary.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages RabbitMQ topology setup (exchanges, queues, bindings).
    /// </summary>
    public interface IRabbitMQTopologyManager
    {
        /// <summary>
        /// Sets up the RabbitMQ topology asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SetupTopologyAsync(CancellationToken cancellationToken = default);
    }
}
```

**Usage in Application Class:**
```csharp
public async Task StartAsync(CancellationToken cancellationToken)
{
    this.logger.LogServiceStartup("Customer Service");

    using (var scope = this.serviceProvider.CreateScope())
    {
        this.logger.LogTopologySetup();
        var topologyManager = scope.ServiceProvider.GetRequiredService<IRabbitMQTopologyManager>();
        await topologyManager.SetupTopologyAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogServiceSubscriptionsInitialization();
        var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
        await serviceInitializer.Initialize().ConfigureAwait(false);
    }

    this.logger.LogServiceStartedSuccessfully("Customer Service");
}
```

**Registration:**
```csharp
// In CompositionRoot.ConfigureServices
// Messaging
serviceCollection.AddRabbitMQServices(context.Configuration);
serviceCollection.AddSingleton<IRabbitMQTopologyManager, RabbitMQTopologyManager>();
```

**Key Points:**
- ✅ Extract topology setup logic into dedicated class
- ✅ Register as Singleton
- ✅ Use in Application class startup sequence
- ✅ Improves testability and modularity

---

## 📊 Metrics Configuration Pattern

Metrics collection is configurable via `MetricsConfiguration` to enable/disable Prometheus metrics. This provides flexibility for different environments.

**Configuration Class:**
```csharp
namespace CommonLibrary.Configuration
{
    /// <summary>
    /// Configuration for metrics collection.
    /// </summary>
    public class MetricsConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether metrics are enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the service name for metrics.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;
    }
}
```

**appsettings.json:**
```json
{
  "MetricsConfiguration": {
    "Enabled": true,
    "ServiceName": "Retail.Customers"
  }
}
```

**Registration:**
```csharp
// In CompositionRoot.ConfigureServices
// General Configuration
serviceCollection.Configure<MetricsConfiguration>(
    context.Configuration.GetSection(nameof(MetricsConfiguration)));

// API Infrastructure
serviceCollection.AddSingleton<CommonLibrary.Telemetry.IMetricsService>(services =>
{
    var metricsConfiguration = services.GetRequiredService<IOptions<MetricsConfiguration>>();
    if (metricsConfiguration.Value.Enabled)
    {
        return new CommonLibrary.Telemetry.MetricsService();
    }
    else
    {
        return new CommonLibrary.Telemetry.EmptyMetricsService();
    }
});
```

**Usage in Startup:**
```csharp
public static void Configure(IApplicationBuilder webApplicationBuilder, IWebHostEnvironment webEnvironment)
{
    var metricsConfig = webApplicationBuilder.ApplicationServices
        .GetRequiredService<IOptions<MetricsConfiguration>>().Value;

    // ... middleware setup ...

    if (metricsConfig.Enabled)
    {
        webApplicationBuilder.UseHttpMetrics();
    }

    webApplicationBuilder.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        if (metricsConfig.Enabled)
        {
            endpoints.MapMetrics();
        }
        // ... health checks ...
    });
}
```

**Key Points:**
- ✅ Use `MetricsConfiguration` for conditional metrics
- ✅ Provide `EmptyMetricsService` (no-op) when disabled
- ✅ Conditionally register `UseHttpMetrics()` and `MapMetrics()` based on configuration
- ✅ Allows disabling metrics in development or specific environments

---

## 📝 Structured Logging Extensions Pattern

Application lifecycle logging uses structured logging extensions with `LoggerMessage.Define` for consistency and performance. Extensions are shared in `CommonLibrary.Logging`.

**Shared Extensions:**
```csharp
namespace CommonLibrary.Logging
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extension methods for application lifecycle logging.
    /// </summary>
    public static class ApplicationLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception?> LogServiceStartupAction =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1001, nameof(LogServiceStartup)),
                "Starting service instance: {ServiceName}");

        /// <summary>
        /// Logs service startup.
        /// </summary>
        public static void LogServiceStartup(this ILogger logger, string serviceName) =>
            LogServiceStartupAction(logger, serviceName, null);

        // ... other extension methods ...
    }
}
```

**Usage in Application Class:**
```csharp
public async Task StartAsync(CancellationToken cancellationToken)
{
    this.logger.LogServiceStartup("Customer Service");

    using (var scope = this.serviceProvider.CreateScope())
    {
        this.logger.LogTopologySetup();
        var topologyManager = scope.ServiceProvider.GetRequiredService<IRabbitMQTopologyManager>();
        await topologyManager.SetupTopologyAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogServiceSubscriptionsInitialization();
        var serviceInitializer = scope.ServiceProvider.GetRequiredService<IServiceInitializer>();
        await serviceInitializer.Initialize().ConfigureAwait(false);

        this.logger.LogDatabaseCreation();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
        this.logger.LogDatabaseInitializationCompleted();
    }

    this.logger.LogServiceStartedSuccessfully("Customer Service");
}
```

**Key Points:**
- ✅ Use `LoggerMessage.Define` for performance (compiled delegates)
- ✅ Shared extensions in `CommonLibrary.Logging`
- ✅ Consistent EventIds and log messages across all services
- ✅ Use extension methods instead of direct `LogInformation` calls

---

## 📋 Service Registration Checklist

When creating a new service, ensure:

- ✅ `CompositionRoot` **static class** with `Configure` and `ConfigureServices` methods
- ✅ `Startup` class with **static** `Configure` method
- ✅ `*Application` class implementing `IHostedService`
- ✅ `Program.cs` following standard pattern
- ✅ Strongly-typed configuration classes registered
- ✅ Global exception handler middleware registered
- ✅ Health checks configured (liveness and readiness)
- ✅ OpenTelemetry configured
- ✅ Metrics configuration with conditional Prometheus metrics
- ✅ Swagger configured (Development only)
- ✅ Repository and Unit of Work registered
- ✅ Converters registered as **Singleton**
- ✅ Service registration organized by category
- ✅ RabbitMQ topology manager registered
- ✅ Structured logging extensions used

---

## Related Documentation

- [Architecture Principles](./Architecture-Principles.md) - Design fundamentals
- [C# Coding Standards](./CSharp-Coding-Standards.md) - C# language features
- [Observability & Performance](./Observability-Performance.md) - Monitoring patterns

