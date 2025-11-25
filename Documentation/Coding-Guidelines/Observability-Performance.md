# Observability & Performance

## Overview

This document covers logging, metrics, distributed tracing, performance optimization, and caching strategies.

## 🧱 Logging Isn't Decoration — It's a Diagnostic Tool

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

✅ Propagate `trace_id` across services.
✅ Attach `span_id` to every request or message
✅ Correlate `trace_id` and `span_id` across logs and metrics.

---

## 📊 Observability Patterns

### OpenTelemetry Integration

All services configure OpenTelemetry for distributed tracing and metrics.

**Registration:**
```csharp
// In CompositionRoot.ConfigureServices - register early
serviceCollection.AddOpenTelemetry(
    context.Configuration,
    serviceName: "Retail.Customers",
    serviceVersion: "1.0.0");
```

**Key Points:**
- ✅ Register OpenTelemetry before other services
- ✅ Use service name and version consistently
- ✅ Configure exporters in `appsettings.json`

### Prometheus Metrics

Services expose Prometheus metrics for monitoring. Metrics collection is configurable via `MetricsConfiguration` to enable/disable metrics in different environments.

**Configuration:**
```json
// appsettings.json
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
serviceCollection.AddSingleton<IMetricsService>(services =>
{
    var metricsConfiguration = services.GetRequiredService<IOptions<MetricsConfiguration>>();
    if (metricsConfiguration.Value.Enabled)
    {
        return new MetricsService();
    }
    else
    {
        return new EmptyMetricsService();  // No-op implementation
    }
});
```

**Usage:**
```csharp
// In Startup.Configure (static method)
public static void Configure(IApplicationBuilder webApplicationBuilder, IWebHostEnvironment webEnvironment)
{
    var metricsConfig = webApplicationBuilder.ApplicationServices
        .GetRequiredService<IOptions<MetricsConfiguration>>().Value;

    // ... other middleware ...

    // Conditionally collect HTTP request metrics
    if (metricsConfig.Enabled)
    {
        webApplicationBuilder.UseHttpMetrics();
    }

    webApplicationBuilder.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        
        // Conditionally expose /metrics endpoint
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
- ✅ Conditionally call `UseHttpMetrics()` and `MapMetrics()` based on configuration
- ✅ Use consistent metric naming: `service_feature_metric`
- ✅ Allows disabling metrics in development or specific environments

---

## 🚀 Performance Starts with Measurement

Don't chase micro-optimizations. You can't optimize what you can't measure. Start by profiling. First, make it right → then make it fast → then measure it.

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

> 🧠 If it's CPU-bound, run it synchronously or move it to a background worker.

---

## 🧠 Caching: Pick the Right Cache and Design Keys Carefully

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
```

---

## ✅ Health checks: Liveness vs Readiness

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
```

---

## Related Documentation

- [Solution Patterns](./Solution-Patterns.md) - Health checks and observability configuration
- [Messaging Patterns](./Messaging-Patterns.md) - Message metrics and tracing
- [C# Coding Standards](./CSharp-Coding-Standards.md) - Performance optimization

