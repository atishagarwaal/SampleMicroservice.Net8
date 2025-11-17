# Event-Driven Communication

This document describes how services in the Retail Microservices solution communicate using event-driven patterns with RabbitMQ.

## Overview

Services communicate asynchronously through events published to RabbitMQ. This enables:
- **Loose Coupling**: Services don't need to know about each other
- **Scalability**: Services can scale independently
- **Resilience**: Services can handle failures gracefully
- **Eventual Consistency**: Services update their state based on events

## Communication Patterns

### 1. Event Publishing

Services publish events when their state changes:

```csharp
public class OrderService
{
    private readonly IMessagingBroker _messagingBroker;

    public async Task<Result<OrderDto>> CreateOrderAsync(OrderDto order)
    {
        // Create order in database
        var orderEntity = await _repository.AddAsync(order);
        
        // Publish event
        var event = new OrderCreatedEvent
        {
            OrderId = orderEntity.Id,
            CustomerId = orderEntity.CustomerId,
            TotalAmount = orderEntity.TotalAmount,
            Timestamp = DateTime.UtcNow
        };
        
        await _messagingBroker.PublishAsync(event);
        
        return Result<OrderDto>.Success(orderDto);
    }
}
```

### 2. Event Consumption

Services subscribe to events they need:

```csharp
public class InventoryUpdatedEventHandler : IEventHandler<InventoryUpdatedEvent>
{
    private readonly IProductService _productService;
    private readonly ILogger<InventoryUpdatedEventHandler> _logger;

    public async Task HandleAsync(InventoryUpdatedEvent @event)
    {
        _logger.LogInformation("Processing inventory update for ProductId: {ProductId}", @event.ProductId);
        
        await _productService.UpdateInventoryAsync(
            @event.ProductId, 
            @event.Quantity);
    }
}
```

## Event Contracts

### Contract Structure

Events are defined in AsyncAPI specifications and generated as C# classes:

```yaml
# AsyncAPI Specification
components:
  messages:
    OrderCreatedEvent:
      payload:
        type: object
        properties:
          orderId:
            type: string
          customerId:
            type: string
          totalAmount:
            type: number
          timestamp:
            type: string
            format: date-time
```

### Contract Versioning

- Contracts are versioned independently
- Services can subscribe to specific versions
- Breaking changes require new versions

## Message Broker Configuration

### RabbitMQ Setup

```csharp
services.AddRabbitMQMessaging(builder =>
{
    builder.Configure(options =>
    {
        options.HostName = configuration["RabbitMQ:HostName"];
        options.Port = int.Parse(configuration["RabbitMQ:Port"]);
        options.UserName = configuration["RabbitMQ:UserName"];
        options.Password = configuration["RabbitMQ:Password"];
    });
});
```

### Exchange and Queue Configuration

```csharp
// Exchange configuration
var exchangeConfig = new ExchangeConfig
{
    Name = "retail.orders",
    Type = ExchangeType.Topic,
    Durable = true
};

// Queue configuration
var queueConfig = new QueueConfig
{
    Name = "orders.created",
    Durable = true,
    Exclusive = false,
    AutoDelete = false
};
```

## Error Handling

### Retry Strategy

Failed message processing is retried with exponential backoff:

```csharp
services.AddRabbitMQMessaging(builder =>
{
    builder.Configure(options =>
    {
        options.RetryCount = 3;
        options.RetryDelay = TimeSpan.FromSeconds(5);
        options.ExponentialBackoff = true;
    });
});
```

### Dead Letter Queue

Messages that fail after all retries are sent to a dead letter queue:

```csharp
var queueConfig = new QueueConfig
{
    Name = "orders.created",
    DeadLetterExchange = "retail.dlx",
    DeadLetterRoutingKey = "orders.created.failed"
};
```

## Best Practices

### 1. Idempotency

Event handlers should be idempotent:

```csharp
public async Task HandleAsync(OrderCreatedEvent @event)
{
    // Check if already processed
    if (await _repository.ExistsAsync(@event.OrderId))
    {
        _logger.LogWarning("Order {OrderId} already processed", @event.OrderId);
        return;
    }
    
    // Process event
    await ProcessOrderAsync(@event);
}
```

### 2. Event Ordering

- Use correlation IDs to track related events
- Don't assume event ordering unless using ordered queues
- Design handlers to handle out-of-order events

### 3. Event Sourcing

Consider event sourcing for audit trails:

```csharp
public class OrderEventStore
{
    public async Task AppendEventAsync(string orderId, IEvent @event)
    {
        await _eventStore.AddAsync(new EventRecord
        {
            AggregateId = orderId,
            EventType = @event.GetType().Name,
            EventData = JsonSerializer.Serialize(@event),
            Timestamp = DateTime.UtcNow
        });
    }
}
```

## Monitoring

### Message Metrics

Track message processing metrics:

```csharp
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly Counter _processedCounter;
    private readonly Histogram _processingTime;

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await ProcessEventAsync(@event);
            _processedCounter.Increment();
        }
        finally
        {
            sw.Stop();
            _processingTime.Record(sw.Elapsed.TotalSeconds);
        }
    }
}
```

## Related Documentation

- [Message Contracts](Message-Contracts.md) - Contract design patterns
- [Error Handling Patterns](Error-Handling-Patterns.md) - Error handling strategies
- [AsyncAPI Contract Generation](../Contracts/AsyncAPI-Contract-Generation.md) - Contract generation

