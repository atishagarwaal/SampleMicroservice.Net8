# 🧭 Recommended Practices for Modern C# Development

## *A developer's field guide to writing maintainable, testable, and elegant .NET code*

---

## 🧱 Build Architecture for Humans, Not Just Machines

Good architecture isn’t about how many layers you can name; it’s about how easily someone else can follow your logic months later. Code should feel predictable, observable, and replaceable — qualities that make it both robust and pleasant to work with.

💡 Guiding principle: You write code for people, not compilers.

Keep your design **predictable**, **observable**, and **replaceable**.

### 💡 The Golden Principles

* **Single responsibility:** Every class, function, or module should do only one job.
* **Explicit boundaries:** Depend on abstractions, not concretes. Consumers shouldn’t know internals
* **Framework agnosticism:** Your business logic should be framework agnostic.
* **Observability:** If you can’t measure it, you can’t maintain it. Don't forget to measure what is important.

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
        await _notifier.SendAsync(user.Email, "Welcome", "You’re in!");
    }
}
```

> 🧠 *A clean boundary is worth a dozen comments.*

---

## ⚙️ Organize Code by Intent, Not Just by Type

When someone opens your repository, they should immediately understand what it does, not just how it’s layered. Group related functionality together — by feature, not file type; otherwise keep a sane layered structure.

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

## 💬 Speak in Code That Explains Itself

Your code should explain itself. If it needs a long comment, it’s probably doing too much.

### ✅ Do this

```csharp
public decimal CalculateInvoiceTotal(IEnumerable<Item> items, decimal taxRate)
{
    var subtotal = items.Sum(i => i.Price * i.Quantity);
    return subtotal + (subtotal * taxRate);
}
```

### ❌ Not this

```csharp
public decimal DoWork(List<Item> x, decimal y)
{
    return x.Sum(i => i.Price * i.Quantity) * (1 + y);
}
```

> ✍️ **Rule of thumb:** ✍️ Write comments for *why*, not *what*. Your naming should do the rest.

---

## 🧩 Embrace Modern C# Features

⚡Use modern features to make your intent clear — not to show off.

```csharp
// Records for immutability
public record Address(string Street, string City, string Country);

// Pattern matching for clarity
if (customer is { IsActive: true, Orders.Count: > 0 })
    logger.LogInformation("Returning customer detected");

// Null-coalescing for sanity
return input ?? throw new ArgumentNullException(nameof(input));
```

---

## 🔍 Validate Inputs Like a Gatekeeper

Bugs love unvalidated data. Guard your public methods and API boundaries.

```csharp
public void ShipOrder(Guid orderId, string destination)
{
    ArgumentOutOfRangeException.ThrowIfEqual(orderId, Guid.Empty);
    ArgumentException.ThrowIfNullOrWhiteSpace(destination);
}
```

✅ Validate early **at the boundary** (controller, service entry).
✅ Fail fast with **clear error messages**.
✅ Never assume “it’ll never happen.”

⚠️ Defensive programming is the difference between resilient systems and fragile ones.

---

## 🧾 Keep Your Methods Small and Honest

A method should fit on one screen and describe one piece of logic.

```csharp
// ✅ Single-purpose, testable
public decimal ApplyDiscount(decimal total, decimal rate)
{
    if (rate is < 0 or > 1)
        throw new ArgumentOutOfRangeException(nameof(rate));

    return total * (1 - rate);
}
```

🧩 *When you scroll to understand, it’s time to extract another method.*

---

## 🕹️ Keep Your Queries Lean

### Entity Framework Example

```csharp
// ✅ Eager load only what you need
var orders = await _db.Orders
    .Include(o => o.Items)
    .Where(o => o.Status == OrderStatus.Pending)
    .ToListAsync();

// ❌ Bad: ToList() before filtering
var all = await _db.Orders.ToListAsync();
var pending = all.Where(o => o.Status == OrderStatus.Pending);
```

> 🧮 Push filters to the database, not the application.

---

## 🕹️ Code Style & Small Rules

### ILogger last parameter

Place `ILogger<T>` as the last constructor parameter — it keeps parameter ordering predictable across types.

```csharp
public MyService(IMyDep dep, ILogger<MyService> logger) { ... }
```

### Avoid static mutable state

Prefer DI-singletons or scoped stores; static mutable variables make tests brittle and concurrency tricky.

```csharp
// ❌ Avoid
public static List<string> GlobalCache = new List<string>();

// ✅ Prefer injected cache
public class CacheHolder { private readonly ConcurrentDictionary<string,string> _cache; }
```

### Add: `using` inside namespace

```csharp
namespace MyApp.Services;
using System.Text.Json;
```

---

## 🧾 Member & File Organization

A class should read like a story — declarations on top, helpers at the bottom.

```csharp
namespace MyApp.Services;

public class OrderService
{
    // 🧱 Constant (ALL_CAPS)
    private const int MAX_RETRY_COUNT = 3;

    // 📦 Field (_camelCase)
    private readonly IRepository<Order> _orderRepository;

    // ⚙️ Constructor (PascalCase for type, camelCase for parameter)
    public OrderService(IRepository<Order> orderRepository) => _orderRepository = orderRepository;

    // 🌐 Public API (PascalCase for methods, camelCase for parameter)
    public async Task<Order> CreateAsync(OrderDto orderDto)
    {
        var order = Map(orderDto);
        for (var i = 0; i < MAX_RETRY_COUNT; i++)
        {
            try
            {
                await _orderRepository.SaveAsync(order);
                return order;
            }
            catch (TransientDatabaseException) when (i < MAX_RETRY_COUNT - 1) { }
        }
        return order;
    }

    // 🛠️ Private Helper (PascalCase for method, camelCase for parameter)
    private static Order Map(OrderDto orderDto) => new(orderDto.Id, orderDto.Items);
}
```

🧱 One class per file keeps clarity.
🌐 Use PascalCase for types, camelCase for locals, and ALL_CAPS for constants.
📜 Use “this.” qualifier only for clarity — not decoration.

---

## 🧱 Dependency Injection & Configuration

Modern .NET thrives on dependency injection — it’s your secret weapon for modularity and testability.

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

## 🧱 Logging Isn’t Decoration — It’s a Diagnostic Tool

Logs are your time machine for production failures. Use structured, consistent formats.

```csharp
public class PaymentProcessor
{
    private readonly ILogger<PaymentProcessor> _logger;

    public PaymentProcessor(ILogger<PaymentProcessor> logger)
        => _logger = logger;

    public async Task ProcessAsync(Payment payment)
    {
        using (_logger.BeginScope(new { payment.Id, payment.UserId }))
        {
            _logger.LogInformation("Processing payment {Id}", payment.Id);
            await DoPaymentAsync(payment);
            _logger.LogInformation("Payment {Id} completed", payment.Id);
        }
    }
}
```

✅ Use placeholders (`{}`) for structured logs.
✅ Mask sensitive data.
✅ Add correlation IDs for tracing across services.

> 🧠 *A good log tells you what happened. A great one tells you why.*

---

## 🧮 Track Metrics for Behavior, Not Just Performance

Metrics turn invisible behavior into measurable truth.

```csharp
public class OrderMetrics
{
    private readonly Counter _created;
    private readonly Histogram _latency;

    public OrderMetrics(IMetricsFactory metrics)
    {
        _created = metrics.CreateCounter("orders_created_total", "Orders created");
        _latency = metrics.CreateHistogram("order_processing_seconds", "Order processing time");
    }

    public IDisposable TrackOrderProcessing()
    {
        _created.Increment();
        return _latency.NewTimer();
    }
}
```

🎯 Use **Counters** for counts, **Histograms** for durations, **Gauges** for current values.
📊 Keep metric names consistent: `service.feature.metric`.

Metrics for messages.

```csharp
public class MessageMetrics
{
    private readonly Counter _consumed;
    private readonly Histogram _latency;

    public MessageMetrics(IMetricsFactory factory)
    {
        _consumed = factory.CreateCounter("messages_consumed_total", "Messages processed");
        _latency = factory.CreateHistogram("message_latency_seconds", "Message processing time");
    }

    public IDisposable Track() => _latency.NewTimer();

    public void Increment() => _consumed.Increment();
}
```

✅ Count processed messages
✅ Track processing latency
✅ Observe retry counts

> 💡 *A dashboard that shows retries per consumer saves hours during incidents.*

**Metric cardinality** — avoid using high-cardinality labels like userId or orderId. Instead, use coarse labels such as `region`, `instance_type`, or `error_type`. If you must capture per-entity data, use traces/logs or sampling

---

## 🧪 Design Code That Wants to Be Tested

Good tests start with good design, not mocking libraries.

### ✅ Loosely coupled design

```csharp
public class InvoiceService
{
    private readonly IInvoiceRepository _repo;
    private readonly IClock _clock;

    public InvoiceService(IInvoiceRepository repo, IClock clock)
    {
        _repo = repo;
        _clock = clock;
    }

    public async Task<Invoice> CreateAsync(Customer c, decimal amount)
    {
        var invoice = new Invoice(c.Id, amount, _clock.UtcNow);
        await _repo.SaveAsync(invoice);
        return invoice;
    }
}
```

🧠 *Inject dependencies, don’t hide them.  If you can swap it for a fake in tests, you’re doing it right.*

---

## 🧾 Write Tests That Read Like Stories

A test should describe behavior, not implementation.

```csharp
[Fact]
public async Task CreateAsync_NewCustomer_SavesInvoice()
{
    // Arrange
    var repo = new InMemoryInvoiceRepo();
    var clock = new FakeClock(DateTime.Parse("2025-01-01"));
    var sut = new InvoiceService(repo, clock);

    // Act
    var result = await sut.CreateAsync(new Customer("A1"), 100);

    // Assert
    result.Amount.Should().Be(100);
    repo.Items.Should().ContainSingle(i => i.CustomerId == "A1");
}
```

> 💡 **Naming rule:**
> `Method_Scenario_ExpectedResult`.
> Example: `Withdraw_InsufficientFunds_ThrowsException`.

## 🚫 Avoid These Test Anti-Patterns

| Anti-Pattern                              | Why It Hurts                             |
| ----------------------------------------- | ---------------------------------------- |
| **Testing test helpers**                  | Adds noise, not confidence               |
| **Mocking your own fakes**                | Tests fakes, not behavior                |
| **Complex mock setups**                   | When setup > Act + Assert, refactor code |

### ❌ Example

```csharp
// BAD
mock.Setup(x => x.Save(It.IsAny<Order>()))
    .Returns(Task.FromResult(CreateFakeOrder()));
```

### ✅ Better

```csharp
var repo = new InMemoryOrderRepo();
await sut.SubmitAsync(order);
repo.Items.Should().Contain(order);
```

✅ Prefer lightweight fakes and in-memory implementations.
✅ Mock behavior only for external dependencies.

## 🔄 Integration & E2E Tests

Keep a clear separation:

| Layer           | Goal                     | Framework                      |
| --------------- | ------------------------ | ------------------------------ |
| **Unit**        | Logic correctness        | xUnit / NUnit                  |
| **Integration** | Components work together | Docker / TestContainers        |
| **E2E**         | Real flows & contracts   | Playwright / REST client tests |

> ⚙️ Run fast unit tests on each commit, slower integration tests in CI, full E2E in staging.

---

## ✉️ Messaging Is a Contract, Not a Conversation

When services talk to each other, the way they *communicate* defines how they *fail*.
Good messaging design treats every message like a contract: explicit, versioned, and self-describing.

Each message should:

1. Be **self-contained** — enough data to process independently.
2. Be **versioned** — backwards compatible where possible.
3. Include **metadata** — IDs, timestamps, correlation.

```csharp
public record OrderCreatedEvent(
    Guid OrderId,
    string CustomerEmail,
    decimal Amount,
    DateTimeOffset Timestamp);
```

> 💡 Treat events as immutable facts, not instructions. They describe what has happened, not what should happen.

---

## ⚙️ Choosing the Right Message Transport

Each transport mechanism has trade-offs — pick the right tool for the communication pattern.

| Use Case                | Transport        | Why                               |
| ----------------------- | ---------------- | --------------------------------- |
| Reliable async events   | RabbitMQ / Kafka | Durable & fault-tolerant          |
| Simple request/response | HTTP / gRPC      | Low latency, easier debugging     |
| Fire-and-forget         | Message Queues   | Decouple producers from consumers |

---

## 🔁 Message Handling and Fault Tolerance

Message consumers are the most failure-prone parts of your system. Always prepare for retries, and partial failures.

```csharp
public class OrderCreatedConsumer : IMessageConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public async Task HandleAsync(OrderCreatedEvent msg, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Processing order {Id}", msg.OrderId);
            await ProcessOrderAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process {Id}", msg.OrderId);
            throw; // Let the broker retry
        }
    }
}
```

✅ Allow retries through broker policies
✅ Redirect poison messages to a dead-letter queue
✅ Log every failure with rich context

---

## 🔄 Idempotency — Reliability’s Best Friend

When the same message arrives twice, the system should act as if it ran once.

```csharp
public async Task ProcessPaymentAsync(PaymentRequest req)
{
    if (await _repo.ExistsAsync(req.TransactionId))
        return; // Already processed

    await _repo.SaveAsync(req);
    await _gateway.ChargeAsync(req);
}
```

> 💬 *Stateless services are easy to scale; idempotent services are easy to trust.*

---

## 🧰 Retry, Timeout, and Circuit Breaker Patterns

Distributed systems fail in unpredictable ways. Resilience patterns keep your services healthy.

```csharp
var policy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutRejectedException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(attempt));

await policy.ExecuteAsync(() => _httpClient.SendAsync(request));
```

✅ Use Polly for declarative resilience
✅ Add timeouts to all external calls
✅ Use circuit breakers to isolate failure cascades

⚡ A retry without backoff is just a denial-of-service attack against yourself.

---

## 🧱 Integration and Orchestration

You can think of modern .NET services as “composable units” — Logging, Metrics, Messaging, and Hosting.

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

## 🔍 Observability in Distributed Systems

A distributed system without tracing is like flying blind. Instrument every step of your workflow with spans, metrics, and logs.

### Trace Everything

```csharp
using var activity = _activitySource.StartActivity("ConsumeOrderCreated");
activity?.SetTag("message.id", msg.OrderId);
activity?.SetTag("operation", "consume");

await _handler.HandleAsync(msg);

activity?.SetStatus(ActivityStatusCode.Ok);
```

✅Propagate `trace_id` across services.
✅Attach `span_id` to every request or message
✅Correlate `trace_id` and `span_id` across logs and metrics.

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

## ✅ Health checks: liveness vs readiness

Implement `IHealthCheck` for key dependencies and expose both liveness and readiness endpoints.
Liveness = "Is the process alive?" Readiness = "Is the process ready to accept traffic?"

```csharp
public class DbHealthCheck : IHealthCheck
{
    private readonly DbContext _db;
    public DbHealthCheck(DbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await _db.Database.CanConnectAsync(cancellationToken);
        return canConnect ? HealthCheckResult.Healthy("DB OK") 
                          : HealthCheckResult.Unhealthy("DB unavailable");
    }
}

---

## 🚦 Security for Message Brokers

```csharp
services.AddRabbitMQMessaging(builder =>
{
    builder.Configure(o =>
    {
        o.HostName = "secure-broker.internal";
        o.UseSsl = true;
        o.UserName = Environment.GetEnvironmentVariable("BROKER_USER");
        o.Password = Environment.GetEnvironmentVariable("BROKER_PASS");
    });
});
```

✅ Use TLS for transport security
✅ Rotate credentials regularly
✅ Limit broker permissions per queue

---

## 🚀 Performance Starts with Measurement

Don’t chase micro-optimizations. You can’t optimize what you can’t measure. Start by profiling. First, make it right → then make it fast → then measure it.

### Use **BenchmarkDotNet** for micro-benchmarks

```csharp
[MemoryDiagnoser]
public class StringBenchmarks
{
    [Benchmark]
    public string Interpolation() => $"Hello {42}";

    [Benchmark]
    public string Concat() => "Hello " + 42;
}
```

Run via CLI:

```bash
dotnet run -c Release --project Benchmarks
```

### ✅ Prefer Async for I/O-Bound Work

Async improves scalability, not speed. Use it only where it makes sense.

```csharp
// Good: non-blocking network call
public async Task<string> FetchAsync(Uri url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
}
```

### ⚠️ Avoid Fake Async for CPU Tasks

```csharp
// ❌ Blocking thread pool unnecessarily
await Task.Run(() => HeavyComputation());
```

> 🧠 If it’s CPU-bound, run it synchronously or move it to a background worker.

---

## 🧱 Choosing the Right Collection

Choose collections based on mutation pattern:

| Use Case             | Best Type                 | Why                        |
| -------------------- | ------------------------- | -------------------------- |
| Append-only          | `List<T>`                 | Compact and cache-friendly |
| Key lookup           | `Dictionary<TKey,TValue>` | O(1) access                |
| Ordered unique items | `SortedSet<T>`            | Automatic sorting          |
| Thread-safe reads    | `ConcurrentDictionary`    | Lock-free concurrency      |

```csharp
var cache = new ConcurrentDictionary<Guid, Order>();
cache.TryAdd(order.Id, order);
```

> 💡 Use `ReadOnlyCollection<T>` for safety when exposing lists publicly.

---

## 🧮 Data Access That Scales

Avoid pulling entire tables into memory.

```csharp
// ✅ Filter at source
var orders = await _db.Orders
    .Where(o => o.Status == OrderStatus.Pending)
    .Include(o => o.Items)
    .ToListAsync();

// ❌ Don’t filter after loading
var all = await _db.Orders.ToListAsync();
var pending = all.Where(o => o.Status == OrderStatus.Pending);
```

⚙️ Push computation to the database, not the application.

---

## 🧠 Caching: pick the right cache and design keys carefully

Use `IMemoryCache` for fast, in-process caches and `IDistributedCache` (Redis) for cross-instance caches. Prefer cache-aside and design cache keys to include a version and tenant where applicable.

```csharp
// IMemoryCache (cache-aside)
if (!_memoryCache.TryGetValue(key, out OrderDto dto))
{
    dto = await LoadOrderFromDbAsync(orderId);
    _memoryCache.Set(key, dto, TimeSpan.FromMinutes(5));
}

// IDistributedCache (Redis)
var cached = await _distributedCache.GetStringAsync(key);
if (cached is null)
{
    var payload = JsonSerializer.Serialize(await LoadOrderFromDbAsync(orderId));
    await _distributedCache.SetStringAsync(key, payload, new DistributedCacheEntryOptions
    { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
}

---

## 🔐 Security by Default

Security should be an instinct, not a checklist.

```csharp
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false;
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
    });
```

✅ Validate all input
✅ Use parameterized queries
✅ Enforce HTTPS
✅ Principle of least privilege for services

## 🔐 Secret Management

* Store secrets in **Azure Key Vault**, **AWS Secrets Manager**, or environment variables.
* Never commit `.json` files with passwords or tokens.
* Rotate credentials regularly.

---

## 🧮 Rate Limiting & API Protection

Add rate limiting and proper status codes.

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("default", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
    });
});
```

✅ Return `429 Too Many Requests` when throttled.
✅ Log client IP and API key on rate limit events.

---

## 🧩 CI/CD Pipelines and DevOps Discipline

### Git Hygiene

✅ One logical change per commit
✅ Keep PRs < 400 lines
✅ Rebase > merge
✅ Always write tests
✅ Integrate with SonarQube or CodeQL

---
