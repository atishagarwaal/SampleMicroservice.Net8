# Testing Guidelines

## Overview

This document covers testing patterns, best practices, and anti-patterns for writing maintainable and effective tests.

## 🧪 Design Code That Wants to Be Tested

Good tests start with good design, not mocking libraries.

### ✅ Loosely coupled design

```csharp
public class InvoiceService
{
    private readonly IInvoiceRepository _repo;
    private readonly IClock _clock;

    public InvoiceService(IInvoiceRepository repo, IClock clock)
    {
        _repo = repo;
        _clock = clock;
    }

    public async Task<Invoice> CreateAsync(Customer c, decimal amount)
    {
        var invoice = new Invoice(c.Id, amount, _clock.UtcNow);
        await _repo.SaveAsync(invoice);
        return invoice;
    }
}
```

🧠 *Inject dependencies, don't hide them. If you can swap it for a fake in tests, you're doing it right.*

---

## 🧾 Write Tests That Read Like Stories

A test should describe behavior, not implementation.

```csharp
[Fact]
public async Task CreateAsync_NewCustomer_SavesInvoice()
{
    // Arrange
    var repo = new InMemoryInvoiceRepo();
    var clock = new FakeClock(DateTime.Parse("2025-01-01"));
    var sut = new InvoiceService(repo, clock);

    // Act
    var result = await sut.CreateAsync(new Customer("A1"), 100);

    // Assert
    result.Amount.Should().Be(100);
    repo.Items.Should().ContainSingle(i => i.CustomerId == "A1");
}
```

> 💡 **Naming rule:**
> `Method_Scenario_ExpectedResult`.
> Example: `Withdraw_InsufficientFunds_ThrowsException`.

---

## 🚫 Avoid These Test Anti-Patterns

| Anti-Pattern                              | Why It Hurts                             |
| ----------------------------------------- | ---------------------------------------- |
| **Testing test helpers**                  | Adds noise, not confidence               |
| **Mocking your own fakes**                | Tests fakes, not behavior                |
| **Complex mock setups**                   | When setup > Act + Assert, refactor code |

### ❌ Example

```csharp
// BAD
mock.Setup(x => x.Save(It.IsAny<Order>()))
    .Returns(Task.FromResult(CreateFakeOrder()));
```

### ✅ Better

```csharp
var repo = new InMemoryOrderRepo();
await sut.SubmitAsync(order);
repo.Items.Should().Contain(order);
```

✅ Prefer lightweight fakes and in-memory implementations.
✅ Mock behavior only for external dependencies.

---

## 🔄 Integration & E2E Tests

Keep a clear separation:

| Layer           | Goal                     | Framework                      |
| --------------- | ------------------------ | ------------------------------ |
| **Unit**        | Logic correctness        | xUnit / NUnit                  |
| **Integration** | Components work together | Docker / TestContainers        |
| **E2E**         | Real flows & contracts   | Playwright / REST client tests |

> ⚙️ Run fast unit tests on each commit, slower integration tests in CI, full E2E in staging.

---

## Testing Requirements

### Coverage Requirements
- **Required on all public methods** - Enforced by developer working on feature, and validated by reviewers.
- If missing unit test coverage is discovered for a file you're already working on, fix it.

### Test Naming Convention
- Test name pattern: `NameOfMethodBeingTested_ConditionBeingTested_ExpectedResult`
- Tests should follow the "Arrange, Act, Assert" pattern

```csharp
/// <summary>
/// Example unit test.
/// </summary>
[Test]
[UnitTest]
public void NameOfMethodBeingTested_ConditionBeingTested_ExpectedResult()
{
    // Arrange
    // Set the environment up here.
      
    // Act
    // Trigger the test case.
  
    // Assert
    // Verify the expected outcome.
}
```

### Constructor Testing
- Test constructor parameters for null validation **only if the constructor actually validates them**
- Use descriptive test names: `Constructor_WhenParameterIsNull_ThrowsArgumentNullException`
- Test each parameter validation separately
- **DO NOT test constructor behavior of external dependencies** (e.g., configuration default value handling, logger initialization)
- Focus on testing the service instance creation and its readiness to handle method calls

### Method Testing
- Test all public methods
- Test success and failure scenarios
- Test edge cases (null, empty, invalid inputs)
- Focus on method behavior, not implementation details
- **Test return values, data transformation, and business logic validation**
- **Do not test internal dependency interactions**

### Mock Guidelines
- Mock external dependencies only (loggers, configuration, external services)
- Keep mock setups simple and focused
- **DO NOT verify mock behavior** (e.g., `loggerMock.Verify()` calls)
- Focus on testing the service's return values and behavior, not its interactions with dependencies
- Mocks should enable testing, not become the subject of testing
- Use mocks to provide necessary dependencies, but don't assert on mock interactions

### External Dependencies Testing
- **DO NOT test external dependencies** (loggers, configuration providers, infrastructure)
- **DO NOT verify logging calls** - logging is an implementation detail, not business logic
- **DO NOT test configuration behavior** - test the service's behavior with different configurations, not the configuration itself
- **DO test the service's functionality** - focus on return values, data transformation, business logic validation

**Example:**
```csharp
// BAD: Testing logger behavior
loggerMock.Verify(x => x.Log(LogLevel.Error, ...), Times.Once);

// BAD: Testing configuration default value handling
[TestMethod]
public void Constructor_WhenConfigurationHasZero_UsesDefaultValue()
{
    // This tests configuration behavior, not service behavior
}

// GOOD: Testing service return value
var result = await service.ProcessDataAsync(input);
Assert.AreEqual(0, result.Count);
Assert.IsNotNull(result);
```

---

## Related Documentation

- [C# Coding Standards](./CSharp-Coding-Standards.md) - Code style and organization
- [Architecture Principles](./Architecture-Principles.md) - Design for testability
- [Solution Patterns](./Solution-Patterns.md) - Solution-specific patterns

