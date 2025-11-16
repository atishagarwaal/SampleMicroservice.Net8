# Logging Standards and Best Practices

## Overview
This document defines the logging standards and best practices for the Retail Microservices solution, following patterns similar to Symbotic reference repositories while using standard .NET `ILogger<T>` instead of custom frameworks.

## Core Principles

### 1. Use Structured Logging
Always use structured logging with placeholders instead of string concatenation.

```csharp
// ✅ Good - Structured logging
_logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", orderId, customerId);

// ❌ Bad - String concatenation
_logger.LogInformation($"Processing order {orderId} for customer {customerId}");
```

### 2. Logger Parameter Position
Place `ILogger<T>` as the **last parameter** in constructors for consistency.

```csharp
// ✅ Good
public ProductService(
    IUnitOfWork unitOfWork,
    IConverter<SkuDto, Sku> skuConverter,
    ILogger<ProductService> logger)
{
    // ...
}

// ❌ Bad - Logger not last
public ProductService(
    ILogger<ProductService> logger,
    IUnitOfWork unitOfWork)
{
    // ...
}
```

### 3. Log Levels

- **LogError**: For exceptions and errors that require immediate attention
- **LogWarning**: For potentially problematic situations that don't prevent operation
- **LogInformation**: For general operational information (entry/exit points, key operations)
- **LogDebug**: For detailed debugging information (not typically used in production)
- **LogTrace**: For very detailed tracing (rarely used)

### 4. Exception Logging
Always include the exception object when logging errors.

```csharp
// ✅ Good - Exception included
_logger.LogError(ex, "Error creating order. OrderId: {OrderId}, CustomerId: {CustomerId}", 
    orderId, customerId);

// ❌ Bad - Exception not included
_logger.LogError("Error creating order: {Message}", ex.Message);
```

### 5. Correlation IDs and Scopes
Use logging scopes for correlation IDs and operation context.

```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["OrderId"] = orderId,
    ["CustomerId"] = customerId,
    ["CorrelationId"] = correlationId
}))
{
    _logger.LogInformation("Processing order");
    // All logs within this scope will include OrderId, CustomerId, and CorrelationId
}
```

### 6. Never Use Console.WriteLine
Always use `ILogger<T>` instead of `Console.WriteLine`.

```csharp
// ✅ Good
_logger.LogInformation("Received OrderCreatedEvent. OrderId: {OrderId}", orderId);

// ❌ Bad
Console.WriteLine($"Received OrderCreatedEvent. OrderId: {orderId}");
```

### 7. Log Entry and Exit Points
Log at Information level for method entry/exit in important operations.

```csharp
public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto)
{
    _logger.LogInformation("Creating order for customer {CustomerId}", orderDto.CustomerId);
    
    try
    {
        // ... operation logic ...
        
        _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerId}", 
            result.Id, orderDto.CustomerId);
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create order for customer {CustomerId}", orderDto.CustomerId);
        throw;
    }
}
```

### 8. Validation Failures
Log validation failures at Warning level (not Error, as they're expected).

```csharp
var validationResult = _validator.Validate(dto);
if (!validationResult.IsValid)
{
    _logger.LogWarning("Validation failed for {EntityType}. Validator: {ValidatorName}, Reason: {FailureReason}",
        nameof(OrderDto), validationResult.ValidatorName, validationResult.FailureReason);
    return BadRequest(validationResult.FailureReason);
}
```

### 9. Sensitive Data
Never log sensitive information (passwords, credit card numbers, SSNs, etc.).

```csharp
// ✅ Good - Masked sensitive data
_logger.LogInformation("User {UserId} logged in", userId);

// ❌ Bad - Sensitive data exposed
_logger.LogInformation("User {UserId} logged in with password {Password}", userId, password);
```

### 10. Performance-Critical Operations
Log performance metrics for critical operations.

```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
try
{
    // ... operation ...
}
finally
{
    stopwatch.Stop();
    _logger.LogInformation("Order processing completed in {DurationMs}ms for OrderId {OrderId}",
        stopwatch.ElapsedMilliseconds, orderId);
}
```

## Implementation Checklist

- [ ] All services have `ILogger<T>` injected as last parameter
- [ ] All controllers have `ILogger<T>` injected as last parameter
- [ ] All event handlers have `ILogger<T>` injected as last parameter
- [ ] No `Console.WriteLine` statements exist
- [ ] All logging uses structured logging with placeholders
- [ ] Exceptions are logged with the exception object
- [ ] Correlation IDs are included in scopes where applicable
- [ ] Log levels are appropriate (Error for exceptions, Warning for validation failures, Information for operations)
- [ ] Entry/exit points of important operations are logged

## Examples

### Service Method
```csharp
public class ProductService : IProductService
{
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(
        IUnitOfWork unitOfWork,
        IConverter<SkuDto, Sku> skuConverter,
        ILogger<ProductService> logger)
    {
        _logger = logger;
        // ...
    }
    
    public async Task<SkuDto> AddProductAsync(SkuDto skuDto)
    {
        _logger.LogInformation("Adding product. Name: {ProductName}", skuDto.Name);
        
        try
        {
            var validationResult = _validator.Validate(skuDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Product validation failed. Validator: {ValidatorName}, Reason: {Reason}",
                    validationResult.ValidatorName, validationResult.FailureReason);
                throw new ArgumentException(validationResult.FailureReason);
            }
            
            var sku = _converter.Convert(skuDto);
            var result = await _unitOfWork.Skus.AddAsync(sku);
            
            _logger.LogInformation("Product added successfully. ProductId: {ProductId}, Name: {ProductName}",
                result.Id, result.Name);
            
            return _dtoConverter.Convert(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product. Name: {ProductName}", skuDto.Name);
            throw;
        }
    }
}
```

### Event Handler
```csharp
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    
    public OrderCreatedEventHandler(
        IProductService productService,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
        // ...
    }
    
    public async Task HandleAsync(OrderCreatedEvent orderCreatedEvent)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["OrderId"] = orderCreatedEvent.OrderId,
            ["CustomerId"] = orderCreatedEvent.CustomerId
        }))
        {
            _logger.LogInformation("Processing OrderCreatedEvent. LineItemsCount: {LineItemsCount}",
                orderCreatedEvent.LineItems?.Length ?? 0);
            
            try
            {
                await _productService.HandleOrderCreatedEvent(orderCreatedEvent);
                _logger.LogInformation("OrderCreatedEvent processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderCreatedEvent");
                throw;
            }
        }
    }
}
```

### Controller Method
```csharp
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    
    public ProductController(
        IProductService productService,
        IMessageValidator<SkuDto> validator,
        ILogger<ProductController> logger)
    {
        _logger = logger;
        // ...
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] SkuDto value)
    {
        if (value == null)
        {
            _logger.LogWarning("Received null product DTO in POST request");
            return BadRequest(MessageConstants.InvalidParameter);
        }
        
        _logger.LogInformation("Creating product. Name: {ProductName}", value.Name);
        
        try
        {
            var validationResult = _validator.Validate(value);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Product validation failed. Validator: {ValidatorName}, Reason: {Reason}",
                    validationResult.ValidatorName, validationResult.FailureReason);
                return BadRequest(new { error = validationResult.FailureReason });
            }
            
            var result = await _productService.AddProductAsync(value);
            
            _logger.LogInformation("Product created successfully. ProductId: {ProductId}", result.Id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product. Name: {ProductName}", value.Name);
            return StatusCode(500, MessageConstants.InternalServerError);
        }
    }
}
```

