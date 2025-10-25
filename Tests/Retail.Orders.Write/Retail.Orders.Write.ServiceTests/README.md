# Retail.Orders.Write Service Tests

This project contains integration tests for the `Retail.Orders.Write` microservice, focusing on testing service-level functionality and behavior using Behavior-Driven Development (BDD) with SpecFlow.

## Overview

The Service Tests project uses **SpecFlow** with **NUnit** framework to test the Order Write service at the integration level. These tests verify that all components work together correctly and handle real-world scenarios including database operations, message publishing, and error handling.

## Test Structure

### Feature Files
- **`F01.OrderWriteService.feature`** - Service startup and health checks
  - Service startup and health check
  - Service dependency injection verification
  - Service error handling with database unavailable

- **`F02.OrderWriteOperations.feature`** - Order Write operations
  - Create new order
  - Update existing order
  - Delete existing order
  - Create order with line items
  - Handle invalid order data
  - Handle duplicate order creation

- **`F03.OrderCommandHandling.feature`** - Order Command Handling
  - Handle CreateOrderCommand successfully
  - Handle UpdateOrderCommand successfully
  - Handle DeleteOrderCommand successfully
  - Handle command with database transaction failure
  - Handle command with message publishing failure
  - Handle command with invalid data

### Step Definitions
- **`OrderWriteServiceSteps.cs`** - Steps for service startup and health checks
- **`OrderWriteOperationsSteps.cs`** - Steps for order write operations
- **`OrderCommandHandlingSteps.cs`** - Steps for command handling scenarios

### Common Infrastructure
- **`TestBase.cs`** - Base class for all service tests
- **`TestData.cs`** - Factory methods for creating test data
- **`appsettings.test.json`** - Test-specific configuration

## Dependencies

- **SpecFlow** - BDD framework for .NET
- **NUnit** - Test framework for SpecFlow integration
- **FluentAssertions** - Fluent assertion library for readable tests
- **Microsoft.Extensions.DependencyInjection** - DI container for testing
- **Microsoft.Extensions.Configuration** - Configuration management
- **Microsoft.Extensions.Logging** - Logging infrastructure
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing

## Running Tests

### Run All Service Tests
```bash
dotnet test Retail.Orders.Write.ServiceTests
```

### Run Tests by Feature
```bash
dotnet test --filter "FullyQualifiedName~OrderWriteService"
dotnet test --filter "FullyQualifiedName~OrderWriteOperations"
dotnet test --filter "FullyQualifiedName~OrderCommandHandling"
```

### Run Specific Scenarios
```bash
dotnet test --filter "TestName~CreateNewOrder"
dotnet test --filter "TestName~HandleCreateOrderCommand"
```

### Run Tests with Verbose Output
```bash
dotnet test --verbosity normal
```

## Test Scenarios

### Service Health and Configuration
- **Service Startup** - Verify service initializes correctly
- **Health Checks** - Verify service responds to health check requests
- **Dependency Injection** - Verify all required services are properly registered
- **Error Handling** - Verify graceful handling of service failures

### Order Write Operations
- **Order Creation** - Test creating new orders with valid data
- **Order Updates** - Test updating existing orders
- **Order Deletion** - Test removing orders from the system
- **Line Item Management** - Test handling orders with multiple line items
- **Data Validation** - Test handling invalid or duplicate data
- **Error Scenarios** - Test graceful failure handling

### Command Pattern Implementation
- **Command Processing** - Test command handler execution
- **Event Publishing** - Test domain event publication after successful operations
- **Transaction Management** - Test database transaction handling
- **Rollback Scenarios** - Test transaction rollback on failures
- **Message Publishing** - Test integration with messaging infrastructure

## Test Data Management

The `TestData` class provides factory methods for creating consistent test data:
- **Sample Orders** - Realistic order data for testing
- **Sample Line Items** - Line item data with proper relationships
- **Sample Commands** - Command objects for different scenarios
- **ID Management** - Consistent ID generation for test isolation

## Configuration

### Test Settings
- **In-Memory Database** - Uses EF Core in-memory provider for fast, isolated testing
- **Mock External Services** - External dependencies are mocked or stubbed
- **Logging Configuration** - Debug-level logging for test debugging
- **Environment Variables** - Support for environment-specific configuration

### Service Registration
- **Logging Services** - Console logging with configurable levels
- **Configuration Services** - Test-specific configuration loading
- **Mock Services** - Stubbed implementations for external dependencies

## Best Practices

1. **Test Isolation** - Each test scenario is independent and doesn't affect others
2. **Realistic Data** - Test data represents actual business scenarios
3. **Error Handling** - Tests verify both success and failure scenarios
4. **Logging Integration** - All operations are logged for debugging
5. **Configuration Management** - Tests use dedicated configuration files

## Continuous Integration

These tests are designed for CI/CD pipelines:
- **Integration Testing** - Verifies component interactions
- **Behavioral Testing** - Tests business logic and workflows
- **Error Scenario Coverage** - Ensures robust error handling
- **Performance Considerations** - Fast execution with in-memory resources

## Troubleshooting

### Common Issues
1. **Test Discovery Issues** - Ensure SpecFlow tools are properly configured
2. **Configuration Errors** - Verify `appsettings.test.json` is accessible
3. **Dependency Resolution** - Check that all required services are registered
4. **Logging Issues** - Verify logger is properly initialized

### Debugging
- Use `dotnet test --logger "console;verbosity=detailed"` for detailed output
- Check test logs for specific failure details
- Verify test data generation is working correctly
- Ensure all step definitions are properly bound to feature files

## Test Execution Flow

1. **Feature File** - Defines test scenarios in Gherkin syntax
2. **Step Definitions** - Implements the behavior for each step
3. **Test Base** - Provides common setup and teardown functionality
4. **Test Data** - Generates consistent, realistic test data
5. **Assertions** - Verifies expected behavior using FluentAssertions
6. **Cleanup** - Ensures test isolation and resource cleanup
