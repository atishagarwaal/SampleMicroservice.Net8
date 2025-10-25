# Retail.BFF.ServiceTests

This project contains service/integration tests for the Retail.BFF project using NUnit and SpecFlow for BDD-style testing.

## Purpose

Service tests verify the behavior of the BFF service as a whole, testing the integration between components and ensuring that the service behaves correctly from an external perspective.

## Test Structure

### Feature Files
- **F01.BFFService.feature**: Main BFF service operations including order retrieval, error handling, and performance scenarios

### Step Definitions
- **BFFServiceStepDefinitions.cs**: Implementation of the Gherkin scenarios using NUnit and SpecFlow

### Common Infrastructure
- **TestBase.cs**: Abstract base class providing common test setup and dependency injection

## Test Scenarios

### Core Functionality
- Retrieve all order details successfully
- Handle empty orders gracefully
- Handle missing customer gracefully
- Handle missing product gracefully

### Error Handling
- Handle service errors gracefully
- Validate error response structure

### Data Validation
- Validate response structure
- Validate line item structure

### Performance
- Performance under load with multiple concurrent requests

## Dependencies

- **NUnit**: Testing framework
- **SpecFlow**: BDD framework for Gherkin scenarios
- **Moq**: Mocking framework for isolating dependencies
- **FluentAssertions**: Readable assertion library
- **Microsoft.Extensions.DependencyInjection**: Dependency injection container
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Logging**: Logging infrastructure
- **Microsoft.Extensions.Http**: HTTP client factory

## Running Tests

```bash
dotnet test
```

## Code Coverage

Code coverage is collected using Coverlet and output in Cobertura format to the `TestResults\` directory.

## Test Patterns

All tests follow the Given-When-Then pattern:
1. **Given**: Set up the test context and data
2. **When**: Execute the action being tested
3. **Then**: Verify the expected behavior and outcomes

## Mocking Strategy

- External dependencies are mocked using Moq
- Service interfaces are mocked to provide controlled test data
- Configuration is loaded from `appsettings.test.json`
- Dependency injection container is configured with mocked services

## Configuration

The `appsettings.test.json` file provides test-specific configuration for:
- Logging levels
- External service URLs and timeouts
- Test environment settings

## BDD Approach

Tests are written in Gherkin syntax, making them readable by both technical and non-technical stakeholders:
- **Feature**: Describes the functionality being tested
- **Scenario**: Describes a specific test case
- **Background**: Sets up common test data
- **Given/When/Then**: Defines the test steps and assertions
