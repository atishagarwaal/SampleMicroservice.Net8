# Architecture Decision Records (ADRs)

This document explains why we made key architectural decisions. Each ADR includes the problem we solved, what we decided, and the trade-offs.

---

## ADR-001: Microservices Architecture

**Status**: Accepted

**Problem**: Need independent scaling, deployment, and team work.

**Solution**: Microservices with:
- Each service owns its database
- RabbitMQ for event communication
- CQRS for Orders (separate read/write)
- BFF service for frontend aggregation

**Trade-offs**:
- ✅ Independent scaling and deployment
- ✅ Teams can work independently
- ❌ More operational complexity
- ❌ Network latency between services
- ❌ Harder debugging

**How we handle it**: Logging, tracing, circuit breakers, centralized monitoring

---

## ADR-002: Clean Architecture Layers

**Status**: Accepted

**Problem**: Need to separate business logic from infrastructure, make code testable.

**Solution**: Four layers:
1. **Domain**: Business entities and rules (no dependencies)
2. **Application**: Use cases and handlers (depends on Domain)
3. **Infrastructure**: Data access, external services (depends on Domain/Application)
4. **API**: Controllers and middleware (depends on Application)

**Trade-offs**:
- ✅ Business logic independent of frameworks
- ✅ Easy to test
- ❌ More files and folders
- ❌ Need discipline to maintain boundaries

**How we handle it**: AutoMapper for mapping, code analysis rules, architecture reviews

---

## ADR-003: CQRS for Orders

**Status**: Accepted

**Problem**: Orders need different models for writes (complex rules) vs reads (fast queries).

**Solution**: Separate services:
- **Orders.Write**: Handles create, update, cancel, complete
- **Orders.Read**: Handles queries, reports, analytics
- Write publishes events → Read consumes to update read models
- Separate databases for each

**Trade-offs**:
- ✅ Better performance for both operations
- ✅ Independent scaling
- ❌ Eventual consistency (read models lag behind)
- ❌ More complex architecture

**How we handle it**: Document eventual consistency, monitor read model lag, event replay for recovery

---

## ADR-004: RabbitMQ for Event Communication

**Status**: Accepted

**Problem**: Need async communication to decouple services, handle high throughput.

**Solution**: RabbitMQ with:
- AsyncAPI contracts (contract-first)
- Services publish/subscribe to events
- Generated C# contracts from AsyncAPI specs
- Shared messaging library

**Trade-offs**:
- ✅ Loose coupling, high throughput
- ✅ Resilient to failures
- ❌ Eventual consistency challenges
- ❌ Harder debugging

**How we handle it**: Correlation IDs, idempotent handlers, dead letter queues, logging

---

## ADR-005: Contract-First with AsyncAPI

**Status**: Accepted

**Problem**: Manual contracts lead to breaking changes, type mismatches, poor docs.

**Solution**: Contract-first development:
- AsyncAPI YAML files define contracts
- Generate C# contracts from specs
- Version contracts independently
- Separate Contracts projects per service

**Trade-offs**:
- ✅ Type safety at compile time
- ✅ Clear boundaries, automatic docs
- ❌ Extra build step
- ❌ Need to regenerate when specs change

**How we handle it**: Automated generation in CI/CD, versioning strategy

---

## ADR-006: Result Pattern for Errors

**Status**: Accepted

**Problem**: Exceptions used for control flow, no compile-time enforcement, performance overhead.

**Solution**: Result<T> pattern:
- Success or failure with typed errors
- Errors part of type system
- Exceptions only for exceptional cases

**Trade-offs**:
- ✅ Compile-time error handling enforcement
- ✅ No exceptions for control flow
- ❌ More verbose code
- ❌ Learning curve

**How we handle it**: Examples, documentation, code reviews, helper methods

---

## ADR-007: Composition Root Pattern

**Status**: Accepted

**Problem**: DI configuration scattered in Program.cs, hard to test and understand.

**Solution**: CompositionRoot class:
- Centralized service registration
- All DI in one place
- Separate from middleware setup
- Testable independently

**Trade-offs**:
- ✅ Clear registration, easy to test
- ✅ Consistent pattern
- ❌ Extra abstraction layer

**How we handle it**: Template for new services, code analysis, documentation

---

## ADR-008: Global Exception Handling

**Status**: Accepted

**Problem**: Error handling scattered, inconsistent responses, duplicate code.

**Solution**: GlobalExceptionHandlerMiddleware:
- Catches all unhandled exceptions
- Consistent ProblemDetails responses (RFC 7807)
- Maps exceptions to HTTP status codes
- Logs with correlation IDs

**Trade-offs**:
- ✅ Consistent responses, centralized handling
- ✅ Less code duplication
- ❌ Less granular control per endpoint

**How we handle it**: Custom exception types, exception filters, logging

---

## ADR-009: Strongly-Typed Configuration

**Status**: Accepted

**Problem**: Direct IConfiguration access causes runtime errors, no IntelliSense, magic strings.

**Solution**: Strongly-typed classes:
- POCOs for each config section
- IOptions<T> pattern
- Validation at startup
- Compile-time checking

**Trade-offs**:
- ✅ Compile-time safety, IntelliSense
- ✅ Clear structure
- ❌ Extra classes to maintain

**How we handle it**: Classes in CommonLibrary, documentation, examples

---

## ADR-010: Application Host Pattern

**Status**: Accepted

**Problem**: Application logic mixed with host setup, hard to test and understand lifecycle.

**Solution**: IApplication interface:
- Defines lifecycle (StartAsync/StopAsync)
- Separates application logic from host
- Testable independently

**Trade-offs**:
- ✅ Clear lifecycle, testable
- ✅ Better organization
- ❌ Extra abstraction

**How we handle it**: Base implementation, template, documentation

---

## ADR-011: Centralized Build Configuration

**Status**: Accepted

**Problem**: Build properties duplicated, inconsistent, hard to maintain.

**Solution**: Centralized config:
- Directory.Build.props at root
- Common.props for shared properties
- Tests.Common.props for tests
- Automatic inheritance

**Trade-offs**:
- ✅ Single source of truth, consistent
- ✅ Easy to update
- ❌ Need to understand MSBuild import order

**How we handle it**: Documentation, code analysis rules, reviews

---

## ADR-012: Separate Contracts Projects

**Status**: Accepted

**Problem**: Contracts embedded in services cause tight coupling, hard versioning.

**Solution**: Separate Contracts projects:
- Each service has dedicated Contracts project
- Independent versioning
- Can publish as NuGet packages
- AsyncAPI YAML included

**Trade-offs**:
- ✅ Independent versioning, shareable
- ✅ Clear boundaries
- ❌ More projects to manage

**How we handle it**: Solution organization, versioning strategy, documentation

---

## Adding New ADRs

Document new architectural decisions here using the same format: Problem, Solution, Trade-offs, How we handle it.

