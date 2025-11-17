# Retail.Orders.Write.Contracts

This project contains the contracts (events, messages, and schemas) for the Retail.Orders.Write microservice.

## Version

**Current Version**: 1.0.0

## Contents

### Events
- `OrderCreatedEvent` - Published when a new order is created
- `OrderUpdatedEvent` - Published when an order is updated
- `OrderCancelledEvent` - Published when an order is cancelled
- `OrderCompletedEvent` - Published when an order is completed

### Subscribed Events
- `InventoryErrorEvent` - Consumed from Products service when inventory processing fails

### AsyncAPI Specification
- `Retail-Orders-Write-AsyncAPI-v1.0.0.yaml` - Complete AsyncAPI specification for this service

## Usage

Reference this project in any service that needs to publish or consume Orders.Write events:

```xml
<ProjectReference Include="..\..\Contracts\Retail.Orders.Write.Contracts\Retail.Orders.Write.Contracts.csproj" />
```

## Versioning

This project follows semantic versioning:
- **Major**: Breaking changes to contracts
- **Minor**: New events or non-breaking changes
- **Patch**: Bug fixes

## Publishing as NuGet Package

This project can be published as a NuGet package for distribution:

```bash
dotnet pack -c Release
dotnet nuget push bin/Release/Retail.Orders.Write.Contracts.1.0.0.nupkg --source <your-nuget-source>
```


