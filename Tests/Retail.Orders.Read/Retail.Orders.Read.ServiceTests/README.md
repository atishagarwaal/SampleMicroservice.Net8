# Retail.Orders.Read.ServiceTests

This project contains integration and service tests for the Retail.Orders.Read microservice using SpecFlow for behavior-driven development (BDD).

## Overview

The ServiceTests project focuses on testing the service layer and integration scenarios, including:

- **Service Startup Tests**: Testing service initialization and health checks
- **Order Read Operations**: Testing order retrieval functionality
- **Event Handling**: Testing order-related event processing

## Test Structure

### Feature Files
- `F01.OrderService.feature` - Service startup and health check scenarios
- `F02.OrderRead.feature` - Order read operation scenarios
- `F03.OrderEventHandling.feature` - Order event handling scenarios

### Step Definitions
- `OrderServiceSteps.cs` - Steps for service startup and health check tests
- `OrderReadSteps.cs` - Steps for order read operation tests
- `OrderEventHandlingSteps.cs` - Steps for order event handling tests

### Common Classes
- `TestBase.cs` - Base class for all service tests
- `TestData.cs` - Test data factory methods

## Running Tests

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- SpecFlow extension (for Visual Studio)

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=OrderServiceTest
```

## Test Categories

Tests are organized into categories for easier filtering:

- `IntegrationTest` - All integration tests
- `OrderServiceTest` - Service startup and health check tests
- `OrderReadTest` - Order read operation tests
- `OrderEventHandlingTest` - Order event handling tests

## Test Scenarios

### Service Startup Tests
- Service startup and health check
- Dependency injection verification
- Error handling with database unavailable

### Order Read Tests
- Read all orders
- Read order by ID
- Read orders by customer ID
- Read order with line items
- Handle non-existent order

### Event Handling Tests
- Handle order created event
- Handle order updated event
- Handle order cancelled event
- Handle line item added event
- Handle invalid event

## Dependencies

- SpecFlow for BDD testing
- NUnit test framework
- FluentAssertions for readable assertions
- Microsoft.Extensions.DependencyInjection for DI testing
- Microsoft.EntityFrameworkCore.InMemory for in-memory database testing

## Configuration

The project uses `appsettings.test.json` for test-specific configuration, including:
- In-memory database connection
- Test-specific logging levels
- Mock external services settings

## Best Practices

- Tests follow the Given-When-Then BDD pattern
- Each scenario tests one specific behavior
- Tests use dependency injection for service resolution
- Tests are independent and can run in any order
- Test data is created using factory methods for consistency
