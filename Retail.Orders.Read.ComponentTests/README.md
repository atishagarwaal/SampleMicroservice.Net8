# Retail.Orders.Read.ComponentTests

This project contains unit tests for the Retail.Orders.Read microservice.

## Overview

The ComponentTests project focuses on testing individual components and classes in isolation, including:

- **Entity Tests**: Testing domain entities (Order, LineItem)
- **DTO Tests**: Testing data transfer objects (OrderDto, LineItemDto)
- **Service Tests**: Testing application services and business logic

## Test Structure

### Entity Tests
- `OrderEntityTests.cs` - Tests for the Order domain entity
- `LineItemEntityTests.cs` - Tests for the LineItem domain entity

### DTO Tests
- `OrderDtoTests.cs` - Tests for the OrderDto class
- `LineItemDtoTests.cs` - Tests for the LineItemDto class

## Running Tests

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter TestCategory=OrderEntity
```

## Test Categories

Tests are organized into categories for easier filtering:

- `UnitTests` - All unit tests
- `OrderEntity` - Order entity specific tests
- `LineItemEntity` - LineItem entity specific tests
- `OrderDto` - OrderDto specific tests
- `LineItemDto` - LineItemDto specific tests

## Dependencies

- MSTest Framework
- FluentAssertions for readable assertions
- Moq for mocking (when needed)
- Coverlet for code coverage

## Best Practices

- Each test method tests one specific behavior
- Tests follow the Arrange-Act-Assert pattern
- Tests are independent and can run in any order
- Tests use descriptive names that explain the scenario being tested
