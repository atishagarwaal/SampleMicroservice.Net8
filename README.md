# Retail Microservices .NET 8

A comprehensive microservices-based retail application built using .NET 8.0, demonstrating modern microservices architecture patterns and best practices.

## Overview

This project showcases a complete microservices ecosystem for retail operations, including customer management, product catalog, order processing, and a Backend-for-Frontend (BFF) API. The system follows Clean Architecture principles and implements design patterns such as CQRS, Event-Driven Architecture, Repository Pattern, and Unit of Work.

### Key Features

- **Microservices Architecture**: Independent, scalable services
- **Event-Driven Communication**: RabbitMQ-based messaging with AsyncAPI contracts
- **CQRS Pattern**: Separate read and write models for optimal performance
- **Clean Architecture**: Layered architecture with clear boundaries
- **Contract-First Development**: Type-safe contracts generated from AsyncAPI specs
- **Comprehensive Testing**: Unit, component, and service tests
- **Containerization**: Docker and Kubernetes support
- **Observability**: Structured logging, metrics, and distributed tracing

## Architecture

The application consists of the following microservices:

### Core Services
- **Retail.Customers**: Customer management and profile services
- **Retail.Products**: Product catalog and inventory management
- **Retail.Orders.Write**: Order creation and processing (Write side)
- **Retail.Orders.Read**: Order querying and reporting (Read side)
- **Retail.BFF**: Backend-for-Frontend API gateway
- **Retail.UI**: User interface application

### Supporting Infrastructure
- **CommonLibrary**: Shared libraries and utilities
- **MessagingInfrastructure**: RabbitMQ messaging framework
- **Contracts**: API contracts and specifications

## Technology Stack

- **.NET 8.0**: Core framework
- **SQL Server**: Primary database
- **RabbitMQ**: Message broker for inter-service communication
- **Entity Framework Core**: ORM for data access
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation
- **Docker**: Containerization support

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- RabbitMQ Server
- Docker (optional)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SampleMicroservice.Net8
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Set up databases**
   - Restore database backups from the `Database/` folder
   - Update connection strings in `appsettings.json` files

4. **Configure RabbitMQ**
   - Start RabbitMQ service
   - Update connection settings in configuration files

5. **Run the application**
   ```bash
   dotnet build
   dotnet run --project Retail.Customers
   dotnet run --project Retail.Products
   dotnet run --project Retail.Orders.Write
   dotnet run --project Retail.Orders.Read
   dotnet run --project Retail.BFF
   ```

## Project Structure

```
SampleMicroservice.Net8/
├── Build/                          # Build configuration and code analysis
├── Contracts/                      # API contracts and specifications
│   ├── OpenAPI/                   # BFF REST API specification (external)
│   └── AsyncAPI/                  # Messaging API specifications (internal)
├── Database/                       # Database backup files
├── Documentation/                  # Project documentation
│   ├── Design Documents/          # Architectural documentation
│   └── Spikes/                    # Research and POC documentation
├── Retail.Customers/              # Customer service
├── Retail.Products/               # Product service
├── Retail.Orders.Write/           # Order write service
├── Retail.Orders.Read/            # Order read service
├── Retail.BFF/                    # Backend-for-Frontend
├── Retail.UI/                     # User interface
├── CommonLibrary/                 # Shared libraries
├── MessagingInfrastructure/       # Messaging framework
└── Tests/                         # Test projects
```

## API Documentation

- **BFF API**: `/swagger` endpoint on BFF service (external API for UI)
- **Internal Services**: `/swagger` endpoints on individual services (for development/testing only)

## Testing

The project includes comprehensive testing at multiple levels:

- **Unit Tests**: Individual component testing with >80% code coverage
- **Component Tests**: Service integration testing
- **Service Tests**: End-to-end functionality testing using SpecFlow

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test project
dotnet test Tests/Retail.Customers/Retail.Customers.ComponentTests
```

See [Testing Guidelines](.cursor/rules/testing-rules.mdc) for detailed testing requirements.

## Deployment

### Docker
```bash
docker-compose up -d
```

### Kubernetes
See [Deployment Guide](Documentation/Operations/Deployment.md) for detailed deployment instructions including:
- Helm chart deployment
- Kubernetes manifests
- Local development setup
- Production deployment considerations

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## Documentation

For comprehensive documentation, see the [Documentation Index](Documentation/Index.md), which provides:

- **Getting Started**: Development setup, tutorials, and quick start guides
- **Architecture & Design**: ADRs, design patterns, and architectural decisions
- **Requirements**: Functional and non-functional requirements
- **Development Standards**: Coding guidelines and API documentation standards
- **Contracts & Integration**: AsyncAPI specifications and contract generation
- **Operations**: Service manual, deployment guides, and CI/CD documentation

### Quick Links
- [Development Guide](Documentation/Development-Guide.md) - Setup and development workflow
- [Architecture Decision Records](Documentation/Architecture/Architecture-Decision-Records.md) - Architectural decisions
- [Coding Guidelines](Documentation/Coding-Guidelines/README.md) - Coding standards and patterns
- [Deployment Guide](Documentation/Operations/Deployment.md) - Deployment instructions


## License

Copyright (c) <Your Company>, LLC. All rights reserved.

## Support

For questions and support, refer to the project documentation and issue tracking system.
