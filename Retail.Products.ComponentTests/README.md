# Retail.Products Component Tests

This project contains unit tests for the Retail.Products microservice using MSTest framework.

## Test Structure

### Entity Tests
- **SkuEntityTests.cs** - Tests for the Sku domain entity
  - Constructor and property tests
  - Validation attribute tests
  - Edge case handling tests

### DTO Tests
- **SkuDtoTests.cs** - Tests for the SkuDto data transfer object
  - Property getter/setter tests
  - Object initialization tests
  - Equality and comparison tests

### Service Tests
- **ProductServiceTests.cs** - Tests for the ProductService business logic
  - CRUD operation tests
  - Event handling tests
  - Transaction management tests
  - Mock verification tests

## Test Categories

Tests are organized using MSTest categories:
- `UnitTests` - All unit tests
- `SkuEntity` - Entity-specific tests
- `SkuDto` - DTO-specific tests
- `ProductService` - Service-specific tests

## Running Tests

### Visual Studio
1. Open the solution in Visual Studio
2. Build the solution
3. Open Test Explorer (Test > Test Explorer)
4. Run all tests or specific test categories

### Command Line
```bash
# Navigate to the test project directory
cd Retail.Products.ComponentTests

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter "TestCategory=ProductService"
```

## Dependencies

- **MSTest** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **coverlet.collector** - Code coverage collection

## Test Data

Test data is created inline within each test method to ensure test isolation and clarity.

## Best Practices

- Each test method tests a single behavior
- Tests follow the Arrange-Act-Assert pattern
- Mock objects are used to isolate the system under test
- Tests are independent and can run in any order
- Clear naming conventions are used for test methods
