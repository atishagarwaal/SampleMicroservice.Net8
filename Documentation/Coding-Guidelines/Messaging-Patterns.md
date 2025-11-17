# Messaging Patterns

## Overview

This document covers event-driven communication patterns, messaging contracts, fault tolerance, and reliability patterns for distributed systems.

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

## 🔄 Idempotency — Reliability's Best Friend

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

## Related Documentation

- [Architecture Principles](./Architecture-Principles.md) - Design fundamentals
- [Observability & Performance](./Observability-Performance.md) - Monitoring messaging
- [Solution Patterns](./Solution-Patterns.md) - Service configuration patterns

