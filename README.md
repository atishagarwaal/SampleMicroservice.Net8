# Sample Microservice .NET 8

A comprehensive microservices-based retail application built using .NET 8.0, demonstrating modern microservices architecture patterns and best practices.

## Overview

This project showcases a complete microservices ecosystem for retail operations, including customer management, product catalog, order processing, and a Backend-for-Frontend (BFF) API. The system follows Clean Architecture principles and implements design patterns such as CQRS with MediatR, Repository Pattern, and Unit of Work.

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

- **Unit Tests**: Individual component testing
- **Component Tests**: Service integration testing
- **Service Tests**: End-to-end functionality testing

Run tests:
```bash
dotnet test
```

## Deployment

### Docker
```bash
docker-compose up -d
```

### Kubernetes
See `Documentation/DeploymentSetupGuide.md` for detailed deployment instructions.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## Documentation

- [Service Manual](Documentation/ServiceManual.md)
- [Deployment Guide](Documentation/DeploymentSetupGuide.md)
- [Test Plan](Documentation/TestPlan.md)
- [API Contracts](Contracts/README.md)

## License

Copyright (c) <Your Company>, LLC. All rights reserved.

## Support

For questions and support, refer to the project documentation and issue tracking system.
