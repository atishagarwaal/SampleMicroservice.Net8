# C# Coding Standards

## Overview

This document covers C# language features, code style, naming conventions, and best practices for writing maintainable C# code.

## 🧩 Embrace Modern C# Features

⚡ Use modern features to make your intent clear — not to show off.

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
✅ Never assume "it'll never happen."

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

🧩 *When you scroll to understand, it's time to extract another method.*

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
📜 Use "this." qualifier only for clarity — not decoration.

---

## 💬 Speak in Code That Explains Itself

Your code should explain itself. If it needs a long comment, it's probably doing too much.

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

> ✍️ **Rule of thumb:** Write comments for *why*, not *what*. Your naming should do the rest.

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

// ❌ Don't filter after loading
var all = await _db.Orders.ToListAsync();
var pending = all.Where(o => o.Status == OrderStatus.Pending);
```

⚙️ Push computation to the database, not the application.

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

## Related Documentation

- [Architecture Principles](./Architecture-Principles.md) - Design principles and patterns
- [Solution Patterns](./Solution-Patterns.md) - Solution-specific implementation patterns
- [Testing Guidelines](./Testing-Guidelines.md) - Testing best practices

