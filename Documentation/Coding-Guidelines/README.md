# Coding Guidelines

*A developer's field guide to writing maintainable, testable, and elegant .NET code*

## Overview

This directory contains comprehensive coding guidelines organized by topic. These guidelines reflect best practices for modern C# development and the specific patterns used in this solution.

## Documentation Structure

### Core Guidelines

| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[CSharp-Coding-Standards.md](./CSharp-Coding-Standards.md)** | C# language standards | Modern C# features, code style, naming conventions, member organization |
| **[Architecture-Principles.md](./Architecture-Principles.md)** | Architecture fundamentals | SOLID principles, code organization, dependency injection, exception handling |

### Solution-Specific Patterns

| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[Solution-Patterns.md](./Solution-Patterns.md)** | Solution patterns | Composition Root, Startup, Application Host (IHostedService), strongly-typed configuration, health checks |

### Domain-Specific Guidelines

| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[Testing-Guidelines.md](./Testing-Guidelines.md)** | Testing practices | Unit tests, integration tests, test patterns, anti-patterns |
| **[Messaging-Patterns.md](./Messaging-Patterns.md)** | Event-driven patterns | Messaging contracts, fault tolerance, idempotency, retry patterns |
| **[Observability-Performance.md](./Observability-Performance.md)** | Observability & performance | Logging, metrics, tracing, performance optimization, caching |
| **[Security-DevOps.md](./Security-DevOps.md)** | Security & DevOps | Security practices, secret management, CI/CD, Git hygiene |

## Quick Reference

### For New Developers
1. Start with [CSharp-Coding-Standards.md](./CSharp-Coding-Standards.md) for language basics
2. Read [Architecture-Principles.md](./Architecture-Principles.md) for design fundamentals
3. Review [Solution-Patterns.md](./Solution-Patterns.md) for solution-specific patterns
4. Follow [Testing-Guidelines.md](./Testing-Guidelines.md) for testing practices

### For Architects
1. [Architecture-Principles.md](./Architecture-Principles.md) - Design principles and patterns
2. [Solution-Patterns.md](./Solution-Patterns.md) - Solution architecture patterns
3. [Messaging-Patterns.md](./Messaging-Patterns.md) - Event-driven architecture

### For Developers
1. [CSharp-Coding-Standards.md](./CSharp-Coding-Standards.md) - C# coding standards
2. [Solution-Patterns.md](./Solution-Patterns.md) - Implementation patterns
3. [Testing-Guidelines.md](./Testing-Guidelines.md) - Testing strategies
4. [Observability-Performance.md](./Observability-Performance.md) - Performance and monitoring

### For Operations
1. [Observability-Performance.md](./Observability-Performance.md) - Monitoring and performance
2. [Security-DevOps.md](./Security-DevOps.md) - Security and deployment practices

## Guiding Principles

💡 **You write code for people, not compilers.**

Keep your design **predictable**, **observable**, and **replaceable**.

### The Golden Principles

* **Single responsibility:** Every class, function, or module should do only one job.
* **Explicit boundaries:** Depend on abstractions, not concretes. Consumers shouldn't know internals.
* **Framework agnosticism:** Your business logic should be framework agnostic.
* **Observability:** If you can't measure it, you can't maintain it. Don't forget to measure what is important.

## Related Documentation

- [Development Guide](../Development-Guide.md) - Development workflow and setup
- [Architecture Decision Records](../Architecture/Architecture-Decision-Records.md) - Architectural decisions
- [API Documentation Standards](../Standards/API-Documentation-Standards.md) - API documentation requirements

