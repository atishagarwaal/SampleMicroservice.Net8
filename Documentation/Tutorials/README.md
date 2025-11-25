# Tutorials

This section provides step-by-step tutorials for common development tasks and patterns used in the Retail Microservices solution. These tutorials build on each other and are designed to help developers understand key concepts and patterns.

## Tutorial Overview

These tutorials demonstrate practical implementation of microservices patterns. They focus on standard .NET 8 patterns and best practices.

### Recommended Learning Path

1. **[Getting Started with a New Service](001-Creating-New-Service.md)** - Create a new microservice from scratch
1. **[Getting Started with a New Service](001-Creating-New-Service.md)** - Create a new microservice from scratch
2. **[Implementing Event-Driven Communication](002-Event-Driven-Communication.md)** - Set up RabbitMQ messaging between services
3. **[Working with Contracts](003-Working-with-Contracts.md)** - Generate and use AsyncAPI contracts
4. **[Adding Health Checks and Observability](004-Health-Checks-Observability.md)** - Implement health checks, metrics, and tracing
5. **[Testing Strategies](005-Testing-Strategies.md)** - Write unit, component, and service tests
6. **[Deployment and CI/CD](006-Deployment-CICD.md)** - Deploy services using Docker and Kubernetes

## Tutorial List

### 001: Creating a New Service
**Duration**: 30-45 minutes  
**Prerequisites**: .NET 8 SDK, basic C# knowledge

Learn how to create a new microservice following the established patterns:
- Project structure and Clean Architecture layers
- Composition Root and Startup patterns
- Application Host pattern (IHostedService)
- Configuration management
- Basic API controller setup

[Read Tutorial →](001-Creating-New-Service.md)

---

### 002: Event-Driven Communication
**Duration**: 45-60 minutes  
**Prerequisites**: Tutorial 001, RabbitMQ knowledge

Implement event-driven communication between services:
- Setting up RabbitMQ messaging
- Creating event contracts
- Publishing events
- Consuming events with handlers
- Error handling and retries

[Read Tutorial →](002-Event-Driven-Communication.md)

---

### 003: Working with Contracts
**Duration**: 30-45 minutes  
**Prerequisites**: Tutorial 001, AsyncAPI knowledge

Generate and use type-safe contracts from AsyncAPI specifications:
- Writing AsyncAPI specifications
- Generating C# contracts
- Versioning contracts
- Using contracts in services
- Contract testing

[Read Tutorial →](003-Working-with-Contracts.md)

---

### 004: Health Checks and Observability
**Duration**: 45-60 minutes  
**Prerequisites**: Tutorial 001

Implement comprehensive observability:
- Health check endpoints (liveness/readiness)
- Prometheus metrics
- OpenTelemetry distributed tracing
- Structured logging
- Monitoring dashboards

[Read Tutorial →](004-Health-Checks-Observability.md)

---

### 005: Testing Strategies
**Duration**: 60-90 minutes  
**Prerequisites**: Tutorial 001, xUnit knowledge

Write comprehensive tests at all levels:
- Unit tests with mocking
- Component tests for service integration
- Service tests with SpecFlow BDD
- Test data management
- Code coverage

[Read Tutorial →](005-Testing-Strategies.md)

---

### 006: Deployment and CI/CD
**Duration**: 45-60 minutes  
**Prerequisites**: Tutorial 001, Docker/Kubernetes basics

Deploy services and set up CI/CD:
- Docker containerization
- Kubernetes deployment with Helm
- CI/CD pipeline setup
- Environment configuration
- Monitoring deployment

[Read Tutorial →](006-Deployment-CICD.md)

---

## Additional Resources

- [Development Guide](../Development-Guide.md) - Comprehensive development setup
- [Coding Guidelines](../Coding-Guidelines/README.md) - Coding standards and best practices
- [Architecture Decision Records](../Architecture/Architecture-Decision-Records.md) - Design decisions and rationale
- [Service Manual](../Operations/ServiceManual.md) - Operational guidance

## Getting Help

If you encounter issues while following these tutorials:

1. Check the [Troubleshooting](../Development-Guide.md#troubleshooting) section
2. Review the [Architecture Decision Records](../Architecture/Architecture-Decision-Records.md)
3. Consult the [Service Manual](../Operations/ServiceManual.md)
4. Review existing service implementations for examples

