# Architecture Principles

## Overview

This document covers fundamental architecture principles, code organization patterns, and design guidelines for building maintainable microservices.

## 🧱 Build Architecture for Humans, Not Just Machines

Good architecture isn't about how many layers you can name; it's about how easily someone else can follow your logic months later. Code should feel predictable, observable, and replaceable — qualities that make it both robust and pleasant to work with.

💡 Guiding principle: You write code for people, not compilers.

Keep your design **predictable**, **observable**, and **replaceable**.

### 💡 The Golden Principles

* **Single responsibility:** Every class, function, or module should do only one job.
* **Explicit boundaries:** Depend on abstractions, not concretes. Consumers shouldn't know internals
* **Framework agnosticism:** Your business logic should be framework agnostic.
* **Observability:** If you can't measure it, you can't maintain it. Don't forget to measure what is important.

```csharp
// Decoupled and testable by design
public interface INotificationSender
{
    Task SendAsync(string to, string subject, string body);
}

public class EmailNotifier : INotificationSender
{
    public Task SendAsync(string to, string subject, string body)
    {
        // Implementation detail...
        return Task.CompletedTask;
    }
}

public class AccountService
{
    private readonly INotificationSender _notifier;

    public AccountService(INotificationSender notifier) => _notifier = notifier;

    public async Task RegisterUserAsync(User user)
    {
        await _notifier.SendAsync(user.Email, "Welcome", "You're in!");
    }
}
```

> 🧠 *A clean boundary is worth a dozen comments.*

---

## ⚙️ Organize Code by Intent, Not Just by Type

When someone opens your repository, they should immediately understand what it does, not just how it's layered. Group related functionality together — by feature, not file type; otherwise keep a sane layered structure.

### Folder structure example

```
/Orders
  - OrderController.cs
  - OrderService.cs
  - OrderRepository.cs
  - OrderDto.cs
```

💬 A developer working on orders should find everything under /Orders, not scattered between Controllers, Services, and Repos.

---

## 🧱 Dependency Injection & Configuration

Modern .NET thrives on dependency injection — it's your secret weapon for modularity and testability.

```csharp
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.AddLogging(b => b.AddConsole());
                services.AddScoped<IClock, SystemClock>();
                services.AddScoped<IOrderRepository, SqlOrderRepository>();
                services.AddScoped<OrderService>();
            });

        await builder.RunConsoleAsync();
    }
}
```

✅ Keep constructors small.
✅ Register dependencies once, not everywhere.
✅ Never hide logic behind ServiceLocator or static helpers.

---

## 🧠 Exception Handling with Context

Silence is deadly. Always explain what failed and why.

```csharp
try
{
    await repository.SaveAsync(order);
}
catch (SqlException ex)
{
    throw new OrderPersistenceException($"Failed saving Order {order.Id}", ex);
}
```

✅ Wrap low-level exceptions with meaningful context.
✅ Avoid `catch (Exception)` unless enriching.
✅ Always include the original `InnerException`.

---

## 🧩 Graceful Startup & Shutdown

Your app should *always* shut down gracefully — finish inflight work, release connections.

```csharp
public class OrderWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _consumer.PollAsync(stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken token)
    {
        await _consumer.FlushAsync();
        await base.StopAsync(token);
    }
}
```

> 🧠 *A graceful shutdown is the difference between stability and data loss.*

---

## 🧭 Configuration & Composition

Treat configuration as code — versioned, validated, and environment-aware.

```csharp
public class CompositionRoot : ICompositionRoot
{
    public void Configure(IConfigurationBuilder builder)
    {
        builder.AddJsonFile("appsettings.json")
               .AddEnvironmentVariables();
    }

    public void ConfigureServices(IConfiguration config, IServiceCollection services)
    {
        services.AddLogging();
        services.AddMetrics();
        services.AddMessaging();
    }
}
```

✅ Use environment variables, not hardcoded values
✅ Keep secrets in vaults
✅ Validate configuration on startup

---

## 🧱 Integration and Orchestration

You can think of modern .NET services as "composable units" — Logging, Metrics, Messaging, and Hosting.

### Example: Bootstrapping Everything in One Host

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                // Logging
                services.AddLogging(b => b.AddConsole());

                // Metrics
                services.AddSingleton<IMetricsFactory, PrometheusMetricsFactory>();

                // Messaging
                services.AddRabbitMQMessaging(b =>
                {
                    b.Configure(o =>
                    {
                        o.HostName = "localhost";
                        o.Port = 5672;
                    });
                });

                // Application
                services.AddHostedService<OrderWorker>();
            })
            .Build();

        await host.RunAsync();
    }
}
```

> 🧩 *Order matters: logging → metrics → messaging → app logic.*

---

## Related Documentation

- [C# Coding Standards](./CSharp-Coding-Standards.md) - C# language features and code style
- [Solution Patterns](./Solution-Patterns.md) - Solution-specific implementation patterns
- [Messaging Patterns](./Messaging-Patterns.md) - Event-driven architecture patterns

