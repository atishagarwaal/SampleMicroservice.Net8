# Retail Microservices Documentation

## Overview

This documentation provides a comprehensive guide to the retail microservices architecture, focusing on event-driven communication, contract generation, and service integration patterns.

## Documentation Structure

| Document | Purpose | Content Focus |
|----------|---------|---------------|
| **[README.md](./README.md)** | Navigation and overview | Quick start, architecture summary, getting started |
| **[AsyncAPI-Contract-Generation.md](./AsyncAPI-Contract-Generation.md)** | Contract generation process | Step-by-step contract generation, troubleshooting |
| **[Service-Architecture.md](./Service-Architecture.md)** | Service design and patterns | Service responsibilities, communication patterns, technology stack |
| **[Contract-Specifications.md](./Contract-Specifications.md)** | Contract details | AsyncAPI specifications, message schemas, versioning |

## Quick Start

### 1. Prerequisites
- .NET 8.0 SDK
- Node.js 18+ (for contract generation)
- Message broker (NATS/RabbitMQ)
- Database (SQL Server/PostgreSQL)

### 2. Generate Contracts
```bash
# Install AsyncAPI Generator
npm install -g @asyncapi/generator@2.0.0

# Generate all service contracts
asyncapi-generator "Contracts/AsyncAPI/Retail-Customers-AsyncAPI-v1.0.0.yaml" @lagoni/asyncapi-quicktype-template@1.0.2 -o "Retail.Customers/src/CleanArchitecture.Contracts" -p quicktypeLanguage=csharp

asyncapi-generator "Contracts/AsyncAPI/Retail-Orders-Read-AsyncAPI-v1.0.0.yaml" @lagoni/asyncapi-quicktype-template@1.0.2 -o "Retail.Orders.Read/src/CleanArchitecture.Contracts" -p quicktypeLanguage=csharp

asyncapi-generator "Contracts/AsyncAPI/Retail-Orders-Write-AsyncAPI-v1.0.0.yaml" @lagoni/asyncapi-quicktype-template@1.0.2 -o "Retail.Orders.Write/src/CleanArchitecture.Contracts" -p quicktypeLanguage=csharp

asyncapi-generator "Contracts/AsyncAPI/Retail-Products-AsyncAPI-v1.0.0.yaml" @lagoni/asyncapi-quicktype-template@1.0.2 -o "Retail.Products/src/CleanArchitecture.Contracts" -p quicktypeLanguage=csharp
```

### 3. Build and Run
```bash
dotnet build
dotnet test
```

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

## Getting Help

1. **Contract Generation Issues**: See [AsyncAPI-Contract-Generation.md](./AsyncAPI-Contract-Generation.md)
2. **Service Design Questions**: See [Service-Architecture.md](./Service-Architecture.md)
3. **Contract Details**: See [Contract-Specifications.md](./Contract-Specifications.md)
4. **General Architecture**: See [Service-Architecture.md](./Service-Architecture.md)
