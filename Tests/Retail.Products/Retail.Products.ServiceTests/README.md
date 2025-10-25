# Retail.Products Service Tests

This project contains integration and behavior-driven tests for the Retail.Products microservice using SpecFlow and NUnit frameworks.

## Test Structure

### Feature Files
- **F01.ProductService.feature** - Basic product service operations
  - Get all products
  - Get product by ID
  - Handle empty product lists

- **F02.ProductCRUD.feature** - CRUD operations testing
  - Add new products
  - Update existing products
  - Delete products
  - Handle invalid data

- **F03.ProductEventHandling.feature** - Event handling scenarios
  - Order created event processing
  - Inventory management
  - Error handling and rollbacks

### Step Definitions
- **ProductServiceStepDefinitions.cs** - Implementation of all feature steps
  - Service setup and teardown
  - Mock data configuration
  - Assertion and verification logic

### Common Infrastructure
- **TestBase.cs** - Base class for service tests
  - Dependency injection setup
  - Mock object configuration
  - Service provider management

### Test Data
- **sample-products.json** - Sample product data for testing

## Test Categories

Tests are organized using SpecFlow tags:
- `@ProductService` - Basic service operations
- `@ProductCRUD` - CRUD operations
- `@ProductEventHandling` - Event processing

## Running Tests

### Visual Studio
1. Open the solution in Visual Studio
2. Build the solution
3. Open Test Explorer (Test > Test Explorer)
4. Run all tests or specific feature files

### Command Line
```bash
# Navigate to the test project directory
cd Retail.Products.ServiceTests

# Run all tests
dotnet test

# Run specific feature
dotnet test --filter "FullyQualifiedName~F01.ProductService"

# Run tests with specific tag
dotnet test --filter "TestCategory=ProductService"
```

## Dependencies

- **SpecFlow** - Behavior-driven development framework
- **NUnit** - Testing framework
- **FluentAssertions** - Assertion library
- **Moq** - Mocking framework
- **Microsoft.Extensions.DependencyInjection** - DI container
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database

## Test Configuration

The `appsettings.test.json` file contains test-specific configuration:
- In-memory database settings
- Test connection strings
- Logging configuration

## Test Lifecycle

1. **BeforeScenario** - Sets up services and mocks
2. **Test Execution** - Runs the feature steps
3. **AfterScenario** - Cleans up resources

## Best Practices

- Tests follow the Given-When-Then pattern
- Mock objects isolate external dependencies
- Test data is configured in step definitions
- Clear separation between setup, execution, and verification
- Proper cleanup ensures test isolation

## Integration with CI/CD

These tests can be integrated into your CI/CD pipeline to ensure:
- Code quality and reliability
- Regression testing
- Automated validation of business requirements
