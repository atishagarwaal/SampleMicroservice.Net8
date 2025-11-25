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

## 🏗️ Test Project Configuration

All test projects follow a standardized configuration pattern using centralized build properties. This ensures consistency across all test projects and simplifies maintenance.

### Centralized Configuration

Test projects inherit configuration from `Build/Tests.Common.props` via `Tests/Directory.Build.props`. This provides:

- **Comprehensive code analysis warning suppressions** - Standardized suppressions for test-specific scenarios
- **Code analysis packages** - Microsoft.CodeAnalysis.NetAnalyzers and StyleCop.Analyzers
- **Test project properties** - Standard settings for all test projects

### Configuration Structure

**Tests/Directory.Build.props:**
```xml
<Project>
  <!-- Imports Tests.Common.props which contains test-specific properties -->
  <Import Project="$(MSBuildThisFileDirectory)..\Build\Tests.Common.props" 
          Condition="Exists('$(MSBuildThisFileDirectory)..\Build\Tests.Common.props')" />
</Project>
```

**Build/Tests.Common.props:**
```xml
<Project>
  <PropertyGroup Label="Test Project Properties">
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup Label="Code Analysis Packages">
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <PropertyGroup Label="Code Analysis Warning Suppressions">
    <NoWarn Label="Underscores">$(NoWarn);CA1707</NoWarn>
    <NoWarn Label="Trailing spaces">$(NoWarn);SA1028</NoWarn>
    <NoWarn Label="Using statements order">$(NoWarn);SA1210</NoWarn>
    <NoWarn Label="Constant field location">$(NoWarn);SA1203</NoWarn>
    <NoWarn Label="Single type in a file">$(NoWarn);SA1402</NoWarn>
    <NoWarn Label="Missing or misformatted Documentation">$(NoWarn);CS1591;SA1600;SA1636;SA1633;SA1624</NoWarn>
    <NoWarn Label="Missing AttributeUsageAttribute">$(NoWarn);CA1018</NoWarn>
    <NoWarn Label="Tuple Parenthesis Spacing">$(NoWarn);SA1008;SA1009</NoWarn>
    <NoWarn Label="Closing Parenthesis should be on same line">$(NoWarn);SA1111</NoWarn>
    <NoWarn Label="Repeated statement">$(NoWarn);S3358</NoWarn>
    <NoWarn Label="NuGet restore with HTTP">$(NoWarn);NU1803</NoWarn>
    <NoWarn Label="Possible null reference">$(NoWarn);CS8602;CS8603;CS8604;CS8620;CS8625;CS8632</NoWarn>
  </PropertyGroup>
</Project>
```

### Test Project Structure

Individual test projects should **not** duplicate suppressions or code analysis packages. They inherit these from `Tests.Common.props` automatically.

**✅ Correct - Minimal test project:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <!-- Test framework packages (NUnit, SpecFlow, etc.) -->
  </ItemGroup>

  <!-- No duplicate NoWarn suppressions - inherited from Tests.Common.props -->
</Project>
```

**❌ Incorrect - Duplicate suppressions:**
```xml
<!-- Don't duplicate suppressions that are already in Tests.Common.props -->
<PropertyGroup Label="Code Analysis Warning Suppressions">
  <NoWarn>$(NoWarn);CS1591;SA1600</NoWarn>  <!-- Already in Tests.Common.props -->
</PropertyGroup>
```

### Code Coverage

All test projects should include coverlet packages for code coverage collection:

```xml
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.4">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <!-- Optional: coverlet.msbuild for MSBuild integration -->
  <PackageReference Include="coverlet.msbuild" Version="6.0.4">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

### Key Points

- ✅ **Use centralized configuration** - All suppressions in `Tests.Common.props`
- ✅ **Don't duplicate** - Individual projects inherit automatically
- ✅ **Include coverlet** - For code coverage collection
- ✅ **Consistent packages** - Code analysis packages configured centrally
- ✅ **Standard properties** - Test project properties set in `Tests.Common.props`

---

## Related Documentation

- [C# Coding Standards](./CSharp-Coding-Standards.md) - Code style and organization
- [Architecture Principles](./Architecture-Principles.md) - Design for testability
- [Solution Patterns](./Solution-Patterns.md) - Solution-specific patterns

