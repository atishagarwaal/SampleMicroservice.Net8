# Contracts

This folder contains API contracts and specifications for the Sample Microservice .NET 8 system.

## Structure

### OpenAPI Specifications
- **Retail-BFF-OpenAPI-v1.0.0.yaml**: REST API specification for the BFF service (external API)

### AsyncAPI Specifications
- **Retail-Orders-Write-AsyncAPI-v1.0.0.yaml**: Messaging API specification for order write events (Orders.Write service)
- **Retail-Orders-Read-AsyncAPI-v1.0.0.yaml**: Messaging API specification for order read events (Orders.Read service)
- **Retail-Customers-AsyncAPI-v1.0.0.yaml**: Messaging API specification for customer events (Customers service)
- **Retail-Products-AsyncAPI-v1.0.0.yaml**: Messaging API specification for product/inventory events (Products service)

## Usage

### OpenAPI Specifications
The BFF OpenAPI specification defines the external REST API contract that clients (UI) use to interact with the system. It can be used to:
- Generate client SDKs for the UI
- Validate BFF API implementation
- Generate documentation for external consumers
- Set up API mocking for testing

### AsyncAPI Specifications
The AsyncAPI specifications define the messaging contracts for event-driven communication between the core microservices. Each service has its own AsyncAPI specification:

- **Orders.Write**: Publishes `OrderCreatedEvent`, `OrderUpdatedEvent`, `OrderCancelledEvent`, and `OrderCompletedEvent`
- **Products**: Subscribes to `OrderCreatedEvent`, publishes `InventoryUpdatedEvent` and `InventoryErrorEvent`
- **Customers**: Subscribes to `InventoryUpdatedEvent` for customer notifications
- **Orders.Read**: Subscribes to `InventoryUpdatedEvent` for read model updates

These specifications can be used to:
- Generate message schemas and DTOs
- Validate message formats between services
- Generate documentation for internal service communication
- Set up message mocking for testing
- Implement proper error handling and retry mechanisms

## Versioning

API contracts follow semantic versioning:
- **Major version**: Breaking changes to the API
- **Minor version**: New features that are backward compatible
- **Patch version**: Bug fixes and minor improvements

## Tools

Recommended tools for working with these specifications:
- **OpenAPI**: Swagger Editor, OpenAPI Generator, Postman
- **AsyncAPI**: AsyncAPI Studio, AsyncAPI Generator

## Validation

All API implementations should be validated against these contracts to ensure consistency and compatibility across the microservices ecosystem.
