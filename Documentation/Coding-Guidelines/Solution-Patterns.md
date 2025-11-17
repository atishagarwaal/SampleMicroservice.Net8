# Solution-Specific Patterns

## Overview

This document documents the standard patterns and practices used across all services in this solution. Follow these patterns consistently to maintain architectural integrity and developer experience.

## 🧱 Composition Root Pattern

All services use a `CompositionRoot` class to centralize dependency injection configuration. This pattern separates service registration from web host configuration.

**Structure:**
```csharp
namespace Retail.Api.Customers.Application
{
    /// <summary>
    /// Configuration for this service.
    /// </summary>
    public class CompositionRoot
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="CompositionRoot"/> class from being created.
        /// </summary>
        protected CompositionRoot()
        {
        }

        /// <summary>
        /// Configures application service with a dependency injection container.
        /// </summary>
        /// <param name="context">The application's builder context.</param>
        /// <param name="serviceCollection">Service collection to register services to.</param>
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
        {
            // Configure OpenTelemetry for observability
            serviceCollection.AddOpenTelemetry(
                context.Configuration,
                serviceName: "Retail.Customers",
                serviceVersion: "1.0.0");

            // Configure strongly-typed configuration classes
            serviceCollection.Configure<DatabaseConnectionConfiguration>(
                context.Configuration.GetSection("ConnectionStrings"));

            // Configure database connection
            serviceCollection.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var dbConfig = serviceProvider.GetRequiredService<IOptions<DatabaseConnectionConfiguration>>().Value;
                options.UseSqlServer(dbConfig.DefaultConnection);
            }, ServiceLifetime.Scoped);

            // Register services, repositories, and application lifecycle
            // ...
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
- ✅ Use static methods for configuration
- ✅ Protected constructor prevents instantiation
- ✅ Separate `Configure` for configuration sources and `ConfigureServices` for DI
- ✅ Use strongly-typed configuration classes (see below)
- ✅ Register OpenTelemetry early for observability

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
        public void Configure(IApplicationBuilder app)
        {
            this.environment.ApplicationName = "Retail.Customers";

            // Register global exception handling middleware early in the pipeline
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            if (this.environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            
            // Collect HTTP request metrics for Prometheus
            app.UseHttpMetrics();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                // Prometheus metrics endpoint
                endpoints.MapMetrics();
                
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
- ✅ Start `IApplication` before running host
- ✅ Log critical exceptions before termination

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

All projects inherit from `Directory.Build.props` and `Build/Common.props` for centralized build properties.

**Directory.Build.props:**
```xml
<Project>
  <Import Project="Build\Common.props" />
</Project>
```

**Common.props:**
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

**Key Points:**
- ✅ Centralize build properties in `Common.props`
- ✅ Enable XML documentation generation
- ✅ Use consistent code analysis rules
- ✅ Apply to all projects via `Directory.Build.props`

---

## 📋 Service Registration Checklist

When creating a new service, ensure:

- ✅ `CompositionRoot` class with `Configure` and `ConfigureServices` methods
- ✅ `Startup` class with `Configure` method
- ✅ `*Application` class implementing `IApplication` and `IHostedService`
- ✅ `Program.cs` following standard pattern
- ✅ Strongly-typed configuration classes registered
- ✅ Global exception handler middleware registered
- ✅ Health checks configured (liveness and readiness)
- ✅ OpenTelemetry configured
- ✅ Prometheus metrics exposed
- ✅ Swagger configured (Development only)
- ✅ Repository and Unit of Work registered
- ✅ Service registered as singleton with multiple interfaces

---

## Related Documentation

- [Architecture Principles](./Architecture-Principles.md) - Design fundamentals
- [C# Coding Standards](./CSharp-Coding-Standards.md) - C# language features
- [Observability & Performance](./Observability-Performance.md) - Monitoring patterns

