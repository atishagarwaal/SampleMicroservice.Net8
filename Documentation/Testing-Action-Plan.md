# Testing Action Plan - Aligning with Symbotic Standards

## Overview
This document provides specific actions to align our testing approach with Symbotic microservice testing patterns based on analysis of reference repositories.

## Analysis Summary

### Current State
- ✅ **Well Tested**: Controllers, Services, Event Handlers, Service Integration
- ⚠️ **Overtested**: DTOs (simple POCOs), Entities (simple domain objects)
- ❌ **Undertested**: Converters (need expansion), Validators (need expansion), Infrastructure (missing)

## Specific Actions

### 1. Remove Overtesting (Low Priority Tests)

#### 1.1 Remove DTO Property Tests
**Files to Remove/Simplify:**
- `Tests/Retail.Products/Retail.Products.ComponentTests/SkuDtoTests.cs` - **REMOVE** (simple POCO)
- `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/OrderDtoTests.cs` - **REMOVE** (simple POCO)
- `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/LineItemDtoTests.cs` - **REMOVE** (simple POCO)
- `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/OrderDtoTests.cs` - **REMOVE** (simple POCO)
- `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/LineItemDtoTests.cs` - **REMOVE** (simple POCO)
- `Tests/Retail.Customers/Retail.Customers.ComponentTests/CustomerDtoTests.cs` - **REMOVE** (simple POCO)
- `Tests/Retail.BFF/Retail.BFF.UnitTests/SkuDtoTests.cs` - **REMOVE** (simple POCO)
- `Tests/Retail.BFF/Retail.BFF.UnitTests/OrderDtoTests.cs` - **REMOVE** (simple POCO)
- `Tests/Retail.BFF/Retail.BFF.UnitTests/CustomerDtoTests.cs` - **REMOVE** (simple POCO)
- `Tests/Retail.BFF/Retail.BFF.UnitTests/LineItemDtoTests.cs` - **REMOVE** (simple POCO)

**Rationale**: DTOs are simple data transfer objects with no business logic. Reference repositories don't test DTOs unless they contain validation logic.

#### 1.2 Simplify Entity Tests
**Files to Review/Simplify:**
- `Tests/Retail.Products/Retail.Products.ComponentTests/SkuEntityTests.cs` - **KEEP** only validation attribute tests
- `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/OrderEntityTests.cs` - **KEEP** only validation attribute tests
- `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/LineItemEntityTests.cs` - **KEEP** only validation attribute tests
- `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/OrderEntityTests.cs` - **KEEP** only validation attribute tests
- `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/LineItemEntityTests.cs` - **KEEP** only validation attribute tests
- `Tests/Retail.Customers/Retail.Customers.ComponentTests/CustomerEntityTests.cs` - **KEEP** only validation attribute tests
- `Tests/Retail.Customers/Retail.Customers.ComponentTests/NotificationEntityTests.cs` - **KEEP** only validation attribute tests

**Action**: Remove property getter/setter tests, keep only validation attribute tests if entities have Data Annotations.

### 2. Expand Converter Tests (High Priority)

#### 2.1 Existing Converter Tests - Enhance Coverage
**Files to Enhance:**
- ✅ `Tests/Retail.Products/Retail.Products.ComponentTests/SkuConverterTests.cs` - **ENHANCE** (add edge cases, null collections, complex mappings)
- ✅ `Tests/Retail.Products/Retail.Products.ComponentTests/SkuDtoConverterTests.cs` - **ENHANCE** (add edge cases, null collections)
- ✅ `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/OrderConverterTests.cs` - **ENHANCE** (add null line items, empty collections)
- ✅ `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/OrderDtoConverterTests.cs` - **ENHANCE** (add null line items, empty collections)
- ✅ `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/LineItemConverterTests.cs` - **ENHANCE** (add edge cases)
- ✅ `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/LineItemDtoConverterTests.cs` - **ENHANCE** (add edge cases)

#### 2.2 Missing Converter Tests - Add
**Files to Create:**
- ❌ `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/OrderConverterTests.cs` - **CREATE**
- ❌ `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/OrderDtoConverterTests.cs` - **CREATE**
- ❌ `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/LineItemConverterTests.cs` - **CREATE**
- ❌ `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/LineItemDtoConverterTests.cs` - **CREATE**
- ❌ `Tests/Retail.Customers/Retail.Customers.ComponentTests/CustomerConverterTests.cs` - **CREATE**
- ❌ `Tests/Retail.Customers/Retail.Customers.ComponentTests/CustomerDtoConverterTests.cs` - **CREATE**
- ❌ `Tests/Retail.Customers/Retail.Customers.ComponentTests/NotificationConverterTests.cs` - **CREATE**
- ❌ `Tests/Retail.Customers/Retail.Customers.ComponentTests/NotificationDtoConverterTests.cs` - **CREATE**

**Test Scenarios to Cover:**
- Null input handling
- Empty collections
- Null properties within objects
- Edge cases (zero values, max values)
- Complex nested object mappings
- Circular reference handling (if applicable)

### 3. Expand Validator Tests (High Priority)

#### 3.1 Existing Validator Tests - Enhance Coverage
**Files to Enhance:**
- ✅ `Tests/Retail.Products/Retail.Products.ComponentTests/SkuDtoValidatorTests.cs` - **ENHANCE** (add all validation rules, edge cases)
- ✅ `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/OrderDtoValidatorTests.cs` - **ENHANCE** (add all validation rules)
- ✅ `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/LineItemDtoValidatorTests.cs` - **ENHANCE** (add all validation rules)

#### 3.2 Missing Validator Tests - Add
**Files to Create:**
- ❌ `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/OrderDtoValidatorTests.cs` - **CREATE**
- ❌ `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/LineItemDtoValidatorTests.cs` - **CREATE**
- ❌ `Tests/Retail.Customers/Retail.Customers.ComponentTests/CustomerDtoValidatorTests.cs` - **CREATE**

**Test Scenarios to Cover:**
- All validation rules (one test per rule)
- Null input handling
- Empty string handling
- Boundary value testing (min/max values)
- Invalid data types
- Custom validation logic
- Error message verification

### 4. Add Infrastructure Tests (Medium Priority)

#### 4.1 Repository Tests - Add
**Files to Create:**
- ❌ `Tests/Retail.Products/Retail.Products.ComponentTests/GenericRepositoryTests.cs` - **CREATE**
- ❌ `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/GenericRepositoryTests.cs` - **CREATE**
- ❌ `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/GenericRepositoryTests.cs` - **CREATE**
- ❌ `Tests/Retail.Customers/Retail.Customers.ComponentTests/GenericRepositoryTests.cs` - **CREATE**

**Test Scenarios to Cover:**
- AddAsync operations
- GetAllAsync operations
- GetByIdAsync operations
- Update operations
- Remove operations
- ExecuteQueryAsync with various predicates
- Null handling
- Exception handling

#### 4.2 UnitOfWork Tests - Add
**Files to Create:**
- ❌ `Tests/Retail.Products/Retail.Products.ComponentTests/UnitOfWorkTests.cs` - **CREATE**
- ❌ `Tests/Retail.Orders.Write/Retail.Orders.Write.ComponentTests/UnitOfWorkTests.cs` - **CREATE**
- ❌ `Tests/Retail.Orders.Read/Retail.Orders.Read.ComponentTests/UnitOfWorkTests.cs` - **CREATE**
- ❌ `Tests/Retail.Customers/Retail.Customers.ComponentTests/UnitOfWorkTests.cs` - **CREATE**

**Test Scenarios to Cover:**
- Transaction management (Begin, Commit, Rollback)
- Repository access
- Multiple operations in transaction
- Rollback on exception
- Nested transactions (if supported)

### 5. Enhance Domain Service Tests (High Priority)

#### 5.1 Existing Service Tests - Review and Enhance
**Files to Review:**
- ✅ `Tests/Retail.Products/Retail.Products.ComponentTests/ProductServiceTests.cs` - **REVIEW** (ensure all business scenarios covered)
- ✅ `Tests/Retail.Customers/Retail.Customers.ComponentTests/CustomerServiceTests.cs` - **REVIEW** (ensure all business scenarios covered)

**Additional Scenarios to Add:**
- Edge cases
- Error handling scenarios
- Boundary conditions
- Complex business logic paths
- Integration between multiple operations

### 6. Add Event Handler Tests (Medium Priority)

#### 6.1 Missing Event Handler Tests - Add
**Files to Create (if event handlers exist in source):**
- Check for event handler classes in source code
- Create comprehensive tests for each event handler
- Test event processing logic
- Test error handling
- Test event publishing

## Implementation Priority

### Phase 1: Remove Overtesting (Quick Wins)
1. Remove DTO property tests
2. Simplify entity tests (keep only validation)

### Phase 2: Expand High Priority Tests
1. Enhance converter tests
2. Add missing converter tests
3. Enhance validator tests
4. Add missing validator tests

### Phase 3: Add Missing Infrastructure Tests
1. Add repository tests
2. Add UnitOfWork tests

### Phase 4: Enhance Domain Tests
1. Review and enhance service tests
2. Add event handler tests (if applicable)

## Test Coverage Goals

Based on reference repositories:
- **Converters**: 90%+ coverage (critical for data transformation)
- **Validators**: 95%+ coverage (critical for input validation)
- **Domain Services**: 85%+ coverage (critical for business logic)
- **Infrastructure**: 70%+ coverage (important for data access)
- **Controllers**: 80%+ coverage (already well tested)
- **Overall**: 75%+ coverage

## Notes

1. **DTO Tests**: Only keep if DTOs contain validation logic or complex initialization
2. **Entity Tests**: Only keep validation attribute tests, remove property tests
3. **Converter Tests**: Critical - ensure comprehensive coverage
4. **Validator Tests**: Critical - ensure all validation rules are tested
5. **Infrastructure Tests**: Use in-memory database for testing repositories
6. **Service Tests**: Focus on business logic, not implementation details

## Reference Patterns

Based on analysis of:
- `task-assignment-calibration-task-generator`
- `task-assignment-inbound-task-administration`
- `task-assignment-suspect-inventory-resolution-service`

**Key Patterns:**
- Extensive converter testing (11+ converter test files in inbound-task-administration)
- Extensive validator testing (11+ validator test files in inbound-task-administration)
- Infrastructure testing (database providers, repositories)
- Domain service testing (business logic scenarios)
- Minimal DTO/Entity property testing

