# Development Guide

## Table of Contents

1. [Overview](#overview)
2. [Development Environment Setup](#development-environment-setup)
3. [Project Structure](#project-structure)
4. [Development Workflow](#development-workflow)
5. [Building and Running](#building-and-running)
6. [Testing](#testing)
7. [Debugging](#debugging)
8. [Code Generation](#code-generation)
9. [Common Tasks](#common-tasks)
10. [Troubleshooting](#troubleshooting)

---

## Overview

This guide provides step-by-step instructions for setting up and working with the Retail Microservices .NET 8 solution. It covers everything from initial setup to common development tasks.

---

## Development Environment Setup

### Prerequisites

Install the following tools:

1. **.NET 8.0 SDK**
   - Download from [Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verify installation: `dotnet --version`

2. **IDE Options**
   - **Visual Studio 2022** (Recommended): Community, Professional, or Enterprise
   - **JetBrains Rider**: Full-featured IDE
   - **Visual Studio Code**: Lightweight editor with C# extension

3. **SQL Server**
   - **SQL Server Express** (Free) or **LocalDB**
   - Or use **Docker**: `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest`

4. **RabbitMQ**
   - **Docker** (Recommended): `docker run -d --hostname rabbitmq --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management`
   - Or install locally from [RabbitMQ Downloads](https://www.rabbitmq.com/download.html)

5. **Git**
   - Download from [Git Downloads](https://git-scm.com/downloads)

6. **Docker** (Optional)
   - For containerized development
   - Download from [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Initial Setup

1. **Clone Repository**
   ```bash
   git clone https://github.com/your-org/SampleMicroservice.Net8.git
   cd SampleMicroservice.Net8
   ```

2. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

3. **Build Solution**
   ```bash
   dotnet build
   ```

4. **Run Tests**
   ```bash
   dotnet test
   ```

5. **Set Up Databases**
   - Restore database backups from `Database/` folder:
     - `Customer.bak` → Customer database
     - `Product.bak` → Product database
     - `Order.bak` → Order database
   - Update connection strings in `appsettings.Development.json` files

6. **Configure RabbitMQ**
   - Access management UI: http://localhost:15672
   - Default credentials: `guest` / `guest`
   - Update connection strings in `appsettings.Development.json`

---

## Project Structure

### Solution Structure

```
SampleMicroservice.Net8/
├── Build/                          # Build configuration
│   ├── Code Analysis/             # Code analysis rules
│   ├── Common.props               # Common build properties
│   └── Tests.Common.props         # Test-specific properties
├── CommonLibrary/                  # Shared libraries
├── Contracts/                      # API contracts
│   ├── AsyncAPI/                  # AsyncAPI specifications
│   └── OpenAPI/                   # OpenAPI specifications
├── Database/                      # Database backups
├── Deployment/                    # Deployment configurations
│   ├── Charts/                    # Helm charts
│   └── Kubernetes/                # Kubernetes manifests
├── Documentation/                  # Project documentation
├── MessagingInfrastructure/       # Messaging framework
├── Source/                        # Source code
│   ├── Retail.BFF/               # BFF service
│   ├── Retail.Customers/         # Customers service
│   ├── Retail.Orders.Read/       # Orders read service
│   ├── Retail.Orders.Write/      # Orders write service
│   ├── Retail.Products/          # Products service
│   └── Retail.UI/                # UI application
└── Tests/                         # Test projects
```

### Service Structure

Each service follows Clean Architecture:

```
Retail.Customers.Service/
├── Application/                   # Application layer
│   ├── CompositionRoot.cs        # DI configuration
│   ├── Startup.cs                # Middleware configuration
│   └── CustomerApplication.cs    # Application lifecycle
├── src/
│   ├── CleanArchitecture.API/    # Presentation layer
│   ├── CleanArchitecture.Application/  # Application layer
│   ├── CleanArchitecture.Domain/ # Domain layer
│   └── CleanArchitecture.Infrastructure/  # Infrastructure layer
├── appsettings.json              # Configuration
└── Program.cs                    # Entry point
```

---

## Development Workflow

### 1. Create Feature Branch

```bash
git checkout -b feature/SS-123456_FeatureName
```

### 2. Make Changes

- Write code following [Coding Guidelines](./Coding-Guidelines/README.md)
- Add tests for new functionality
- Update documentation as needed

### 3. Build and Test

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run specific test project
dotnet test Tests/Retail.Customers/Retail.Customers.ComponentTests
```

### 4. Commit Changes

Follow [Commit Message Guidelines](../CONTRIBUTING.md#commit-message-guidelines):

```bash
git add .
git commit -m "feat(customers): Add email validation"
```

### 5. Push and Create Pull Request

```bash
git push origin feature/SS-123456_FeatureName
```

---

## Building and Running

### Build Solution

```bash
# Build all projects
dotnet build

# Build specific project
dotnet build Source/Retail.Customers/Retail.Customers.Service

# Build Release configuration
dotnet build -c Release
```

### Run Services

**Option 1: Run Individual Services**

```bash
# Customers Service (Port 7001)
dotnet run --project Source/Retail.Customers/Retail.Customers.Service

# Products Service (Port 7003)
dotnet run --project Source/Retail.Products/Retail.Products.Service

# Orders Write Service (Port 7002)
dotnet run --project Source/Retail.Orders.Write/Retail.Orders.Write.Service

# Orders Read Service (Port 7005)
dotnet run --project Source/Retail.Orders.Read/Retail.Orders.Read.Service

# BFF Service (Port 7004)
dotnet run --project Source/Retail.BFF

# UI Application (Port 7000)
dotnet run --project Source/Retail.UI
```

**Option 2: Use Docker Compose**

```bash
docker-compose up -d
```

**Option 3: Use Launch Settings**

In Visual Studio or Rider, use the launch configurations in `launchSettings.json`.

### Service URLs

- **Retail.UI**: http://localhost:7000
- **Retail.Customers**: http://localhost:7001/swagger
- **Retail.Orders.Write**: http://localhost:7002/swagger
- **Retail.Products**: http://localhost:7003/swagger
- **Retail.BFF**: http://localhost:7004/swagger
- **Retail.Orders.Read**: http://localhost:7005/swagger
- **RabbitMQ Management**: http://localhost:15672

---

## Testing

### Test Types

1. **Unit Tests**: Test individual components
   - Location: `Tests/{Service}/{Service}.ComponentTests`
   - Run: `dotnet test Tests/Retail.Customers/Retail.Customers.ComponentTests`

2. **Component Tests**: Test component integration
   - Location: `Tests/{Service}/{Service}.ComponentTests`
   - Run: `dotnet test Tests/Retail.Customers/Retail.Customers.ComponentTests`

3. **Service Tests**: End-to-end service testing
   - Location: `Tests/{Service}/{Service}.ServiceTests`
   - Run: `dotnet test Tests/Retail.Customers/Retail.Customers.ServiceTests`

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/Retail.Customers/Retail.Customers.ComponentTests

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~CreateCustomer_WhenEmailIsUnique_ReturnsSuccess"
```

### Test Naming Convention

```csharp
[Test]
[UnitTest]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    // Act
    // Assert
}
```

---

## Debugging

### Visual Studio

1. Set breakpoints in code
2. Press F5 to start debugging
3. Use Debug toolbar for step-through debugging

### Visual Studio Code

1. Install C# extension
2. Create `.vscode/launch.json`:
   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": ".NET Core Launch",
         "type": "coreclr",
         "request": "launch",
         "preLaunchTask": "build",
         "program": "${workspaceFolder}/Source/Retail.Customers/Retail.Customers.Service/bin/Debug/net8.0/Retail.Customers.Service.dll",
         "args": [],
         "cwd": "${workspaceFolder}/Source/Retail.Customers/Retail.Customers.Service",
         "stopAtEntry": false,
         "env": {
           "ASPNETCORE_ENVIRONMENT": "Development"
         }
       }
     ]
   }
   ```

### Rider

1. Set breakpoints
2. Right-click project → Debug
3. Use debugging tools in IDE

### Logging

Structured logging is configured using `ILogger<T>`. Logs include:
- Correlation IDs
- Trace context
- Structured JSON format

Example:
```csharp
_logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", orderId, customerId);
```

---

## Code Generation

### AsyncAPI Contract Generation

Contracts are generated from AsyncAPI YAML specifications. See the [AsyncAPI Contract Generation Guide](./Contracts/AsyncAPI-Contract-Generation.md) for complete instructions, prerequisites, and troubleshooting.

---

## Common Tasks

### Add New Service

1. Create service project structure
2. Add `CompositionRoot.cs` and `Startup.cs`
3. Implement `IHostedService` interface in Application class
4. Add to solution: `dotnet sln add Source/NewService/NewService.Service`
5. Create AsyncAPI contract
6. Generate contracts
7. Add tests

### Add New Endpoint

1. Create controller action
2. Add XML documentation
3. Add validation
4. Add unit tests
5. Update Swagger documentation

### Add New Event Handler

1. Create event handler class
2. Implement `IEventHandler<T>`
3. Register in `CompositionRoot`
4. Add tests
5. Update AsyncAPI contract if needed

### Database Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName --project Source/Retail.Customers/Retail.Customers.Service

# Update database
dotnet ef database update --project Source/Retail.Customers/Retail.Customers.Service
```

### Update Dependencies

```bash
# Update all packages
dotnet list package --outdated

# Update specific package
dotnet add package PackageName --version 1.0.0
```

---

## Troubleshooting

### Build Issues

**Problem**: Build fails with missing references

**Solution**:
```bash
dotnet restore
dotnet clean
dotnet build
```

### Database Connection Issues

**Problem**: Cannot connect to database

**Solution**:
1. Verify SQL Server is running
2. Check connection string in `appsettings.Development.json`
3. Verify database exists
4. Check firewall settings

### RabbitMQ Connection Issues

**Problem**: Cannot connect to RabbitMQ

**Solution**:
1. Verify RabbitMQ is running: `docker ps`
2. Check connection string in `appsettings.Development.json`
3. Verify ports 5672 and 15672 are accessible
4. Check RabbitMQ logs: `docker logs rabbitmq`

### Port Already in Use

**Problem**: Port already in use error

**Solution**:
1. Find process using port:
   ```bash
   # Windows
   netstat -ano | findstr :7001
   
   # Linux/Mac
   lsof -i :7001
   ```
2. Kill process or change port in `launchSettings.json`

### Test Failures

**Problem**: Tests failing

**Solution**:
1. Verify test database is set up
2. Check test configuration
3. Run tests individually to isolate issue
4. Check test logs for details

### Contract Generation Issues

**Problem**: Contract generation fails

**Solution**:
1. Verify Node.js is installed: `node --version`
2. Verify AsyncAPI generator is installed: `asyncapi-generator --version`
3. Check AsyncAPI YAML syntax
4. See [AsyncAPI Contract Generation](./Contracts/AsyncAPI-Contract-Generation.md)

---

## Additional Resources

- [Coding Guidelines](./Coding-Guidelines/README.md)
- [Architecture Decision Records](./Architecture/Architecture-Decision-Records.md)
- [Design Documentation](./Design/README.md)
- [Contributing Guide](../CONTRIBUTING.md)
- [Testing Guidelines](../.cursor/rules/testing-rules.mdc)

---

**Last Updated**: 2024

