# Retail.BFF.UnitTests

This project contains unit tests for the Retail.BFF project using MSTest framework.

## Purpose

Unit tests verify the behavior of individual components in isolation, ensuring that each class and method works correctly without external dependencies.

## Test Structure

### DTO Tests
- **SkuDtoTests**: Tests for the SkuDto data transfer object
- **OrderDtoTests**: Tests for the OrderDto data transfer object  
- **CustomerDtoTests**: Tests for the CustomerDto data transfer object
- **LineItemDtoTests**: Tests for the LineItemDto data transfer object

### Controller Tests
- **BFFControllerTests**: Tests for the BFFController API controller

### Provider Tests
- **ProductProviderTests**: Tests for the ProductProvider service

## Test Categories

Each test is categorized using `[TestCategory]` attributes:
- **UnitTests**: General unit test category
- **SkuDto**: Tests specific to SkuDto
- **OrderDto**: Tests specific to OrderDto
- **CustomerDto**: Tests specific to CustomerDto
- **LineItemDto**: Tests specific to LineItemDto
- **BFFController**: Tests specific to BFFController
- **ProductProvider**: Tests specific to ProductProvider

## Dependencies

- **MSTest**: Testing framework
- **Moq**: Mocking framework for isolating dependencies
- **FluentAssertions**: Readable assertion library
- **Coverlet**: Code coverage collection

## Running Tests

```bash
dotnet test
```

## Code Coverage

Code coverage is collected using Coverlet and output in Cobertura format to the `TestResults\` directory.

## Test Patterns

All tests follow the Arrange-Act-Assert pattern:
1. **Arrange**: Set up test data and mocks
2. **Act**: Execute the method under test
3. **Assert**: Verify the expected behavior

## Mocking Strategy

- External dependencies are mocked using Moq
- HTTP clients are mocked to avoid actual network calls
- Configuration objects are mocked to provide test values
- Service interfaces are mocked to isolate the component under test
