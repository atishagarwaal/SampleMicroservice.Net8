# Retail Microservices Documentation

## Overview

This documentation provides a comprehensive guide to the retail microservices architecture, focusing on event-driven communication, contract generation, and service integration patterns.

## Documentation Structure

### Getting Started
| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[Development-Guide.md](./Development-Guide.md)** | Development setup and workflow | Environment setup, building, running, debugging, common tasks |
| **[Tutorials/README.md](./Tutorials/README.md)** | Step-by-step tutorials | Practical guides for common development tasks |

### Architecture & Design
| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[Architecture/README.md](./Architecture/README.md)** | Architecture documentation | Architectural decisions and design patterns |
| **[Architecture/Architecture-Decision-Records.md](./Architecture/Architecture-Decision-Records.md)** | Architectural decisions | ADRs documenting design choices and rationale |
| **[Design/README.md](./Design/README.md)** | Design documentation | Communication patterns, data management, configuration |
| **[Design/Event-Driven-Communication.md](./Design/Event-Driven-Communication.md)** | Event-driven patterns | Event publishing, consumption, error handling |

### Requirements
| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[Requirements/README.md](./Requirements/README.md)** | Requirements documentation | Functional and non-functional requirements |
| **[Requirements/Functional-Requirements.md](./Requirements/Functional-Requirements.md)** | System functionality | Functional requirements, user stories, acceptance criteria |
| **[Requirements/Non-Functional-Requirements.md](./Requirements/Non-Functional-Requirements.md)** | Quality attributes | Performance, scalability, reliability, security requirements |

### Development Standards
| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[Coding Guidelines](./Coding-Guidelines/README.md)** | Coding standards | C# best practices, architecture principles, solution patterns, testing |
| **[Standards/README.md](./Standards/README.md)** | Development standards | Documentation and API standards |
| **[Standards/API-Documentation-Standards.md](./Standards/API-Documentation-Standards.md)** | API documentation | OpenAPI/Swagger, XML comments, AsyncAPI documentation |

### Contracts & Integration
| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[Contracts/README.md](./Contracts/README.md)** | Contracts documentation | Contract generation and integration patterns |
| **[Contracts/AsyncAPI-Contract-Generation.md](./Contracts/AsyncAPI-Contract-Generation.md)** | Contract generation process | Step-by-step contract generation, troubleshooting |

### Operations
| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[Operations/README.md](./Operations/README.md)** | Operations documentation | Deployment, service operations, CI/CD |
| **[Operations/ServiceManual.md](./Operations/ServiceManual.md)** | Operational guidance | Service operations, monitoring, troubleshooting |
| **[Operations/Deployment.md](./Operations/Deployment.md)** | Deployment instructions | Kubernetes deployment, Helm charts, local setup |
| **[Operations/Jenkinsfile.README.md](./Operations/Jenkinsfile.README.md)** | CI/CD pipeline | Jenkins pipeline configuration and usage |
| **[ReleaseNotes/README.md](./ReleaseNotes/README.md)** | Release notes | Version history, changes, migration guides |

## Architecture Summary

### Core Services
- **Retail.Customers**: Customer data management and inventory subscription
- **Retail.Orders.Read**: Order read models and querying  
- **Retail.Orders.Write**: Order creation and lifecycle management
- **Retail.Products**: Product inventory and order processing

### Key Principles
- **Event-Driven**: Services communicate through well-defined events
- **Contract-First**: AsyncAPI specifications define service interfaces
- **Service Independence**: Each service owns its contracts and data
- **Type Safety**: Generated C# contracts ensure compile-time validation

## Quick Reference

### For New Developers
1. Start with [Development-Guide.md](./Development-Guide.md) for setup
2. Follow [Tutorials/README.md](./Tutorials/README.md) for step-by-step guides
3. Read [Coding Guidelines](./Coding-Guidelines/README.md) for standards
4. Review [Architecture/Architecture-Decision-Records.md](./Architecture/Architecture-Decision-Records.md) for architecture
5. Check [CONTRIBUTING.md](../CONTRIBUTING.md) for contribution process

### For Architects
1. [Architecture/Architecture-Decision-Records.md](./Architecture/Architecture-Decision-Records.md) - Design decisions
2. [Design/README.md](./Design/README.md) - Design patterns and architecture
3. [Design/Event-Driven-Communication.md](./Design/Event-Driven-Communication.md) - Communication patterns
4. [Requirements/Non-Functional-Requirements.md](./Requirements/Non-Functional-Requirements.md) - Quality attributes

### For Developers
1. [Development-Guide.md](./Development-Guide.md) - Development workflow
2. [Tutorials/README.md](./Tutorials/README.md) - Step-by-step tutorials
3. [Coding Guidelines](./Coding-Guidelines/README.md) - Coding standards and patterns
4. [Standards/API-Documentation-Standards.md](./Standards/API-Documentation-Standards.md) - API docs
5. [Contracts/AsyncAPI-Contract-Generation.md](./Contracts/AsyncAPI-Contract-Generation.md) - Contracts

### For Operations
1. [Operations/ServiceManual.md](./Operations/ServiceManual.md) - Operations
2. [Operations/Deployment.md](./Operations/Deployment.md) - Deployment guide
3. [Operations/Jenkinsfile.README.md](./Operations/Jenkinsfile.README.md) - CI/CD pipeline
4. [ReleaseNotes/README.md](./ReleaseNotes/README.md) - Release history

## Getting Help

1. **Setup Issues**: See [Development-Guide.md](./Development-Guide.md)
2. **Contract Generation**: See [Contracts/AsyncAPI-Contract-Generation.md](./Contracts/AsyncAPI-Contract-Generation.md)
3. **Service Design**: See [Design/README.md](./Design/README.md)
4. **Coding Standards**: See [Coding Guidelines](./Coding-Guidelines/README.md)
5. **API Documentation**: See [Standards/API-Documentation-Standards.md](./Standards/API-Documentation-Standards.md)
6. **Contributing**: See [CONTRIBUTING.md](../CONTRIBUTING.md)
