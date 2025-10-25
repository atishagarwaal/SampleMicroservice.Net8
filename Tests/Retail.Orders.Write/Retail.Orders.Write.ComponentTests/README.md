# Retail.Orders.Write Component Tests

This project contains unit tests for the `Retail.Orders.Write` microservice, focusing on testing individual components in isolation.

## Overview

The Component Tests project uses **MSTest** framework to test the core components of the Order Write service, including domain entities, DTOs, and commands. These tests are designed to run quickly and provide fast feedback during development.

## Test Structure

### Entity Tests
- **`OrderEntityTests.cs`** - Tests for the `Order` domain entity
  - Validation attribute testing
  - Property setting and retrieval
  - Line item collection management
  - Data integrity validation

- **`LineItemEntityTests.cs`** - Tests for the `LineItem` domain entity
  - Validation attribute testing
  - Property setting and retrieval
  - Navigation property testing
  - Data validation

### DTO Tests
- **`OrderDtoTests.cs`** - Tests for the `OrderDto` class
  - Data transfer object validation
  - Property setting and retrieval
  - Line item collection handling
  - Edge case handling

- **`LineItemDtoTests.cs`** - Tests for the `LineItemDto` class
  - Data transfer object validation
  - Property setting and retrieval
  - Data type handling
  - Boundary value testing

### Command Tests
- **`CreateOrderCommandTests.cs`** - Tests for the `CreateOrderCommand` class
  - Command pattern implementation
  - Property validation
  - Interface implementation verification

## Dependencies

- **MSTest.TestFramework** - Unit testing framework
- **FluentAssertions** - Fluent assertion library for readable tests
- **Moq** - Mocking framework for isolating dependencies
- **Microsoft.CodeAnalysis.NetAnalyzers** - Code analysis and quality tools

## Running Tests

### Run All Tests
```bash
dotnet test Retail.Orders.Write.ComponentTests
```

### Run Specific Test Class
```bash
dotnet test --filter "TestClass~OrderEntityTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "TestName~Order_WithValidData_ShouldBeValid"
```

### Run Tests with Verbose Output
```bash
dotnet test --verbosity normal
```

## Test Categories

### Unit Tests
- **Entity Validation** - Tests domain entity validation attributes and business rules
- **DTO Operations** - Tests data transfer object functionality and data handling
- **Command Pattern** - Tests command objects and their properties
- **Data Integrity** - Tests data consistency and validation logic

### Test Patterns
- **Arrange-Act-Assert** - Standard unit test structure
- **Property Testing** - Testing getter/setter functionality
- **Collection Testing** - Testing list and collection operations
- **Validation Testing** - Testing data validation and constraints

## Best Practices

1. **Test Naming** - Use descriptive test names that explain the scenario and expected outcome
2. **Test Isolation** - Each test should be independent and not rely on other tests
3. **Assertion Clarity** - Use FluentAssertions for readable and expressive assertions
4. **Test Data** - Use realistic test data that represents actual usage scenarios
5. **Coverage** - Aim for comprehensive coverage of all public methods and properties

## Configuration

The project uses `MSTestSettings.cs` to configure test execution:
- Parallel test execution enabled
- Method-level test isolation
- Optimized for fast execution

## Continuous Integration

These tests are designed to run quickly in CI/CD pipelines:
- Fast execution time
- No external dependencies
- Reliable and repeatable results
- Clear pass/fail indicators

## Troubleshooting

### Common Issues
1. **Test Discovery Issues** - Ensure MSTest.TestAdapter is properly referenced
2. **Assertion Failures** - Check that FluentAssertions is properly imported
3. **Build Errors** - Verify all project references are correct

### Debugging
- Use `dotnet test --logger "console;verbosity=detailed"` for detailed output
- Run individual tests to isolate issues
- Check test output for specific failure details
