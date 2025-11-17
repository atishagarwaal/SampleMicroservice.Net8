# Retail Microservices Service Manual

Operational guide for running and maintaining the retail microservices system.

## Services

1. **Retail.Customers** (7001): Customer management
2. **Retail.Products** (7003): Product catalog and inventory
3. **Retail.Orders.Write** (7002): Order creation (CQRS write side)
4. **Retail.Orders.Read** (7005): Order queries (CQRS read side)
5. **Retail.BFF** (7004): API gateway for frontend
6. **Retail.UI** (7000): Web interface

**Infrastructure**: CommonLibrary, MessagingInfrastructure (RabbitMQ), Contracts

## System Context Diagram

```mermaid
graph TB
    Customer["👤 Customer<br/>End user of the retail system"]
    Admin["👨‍💼 Administrator<br/>System administrator"]
    
    RetailSystem["🏪 Retail Microservices System<br/>Complete retail ecosystem with<br/>customer management, product catalog,<br/>and order processing"]
    
    SQLServer["🗄️ SQL Server<br/>Primary database for data persistence"]
    RabbitMQ["🐰 RabbitMQ<br/>Message broker for inter-service communication"]
    Swagger["📚 Swagger UI<br/>API documentation and testing interface"]
    
    Customer -->|Uses| RetailSystem
    Admin -->|Manages| RetailSystem
    RetailSystem -->|Stores data| SQLServer
    RetailSystem -->|Publishes/Consumes messages| RabbitMQ
    Admin -->|Tests APIs| Swagger
    
    classDef person fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef system fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef external fill:#fff3e0,stroke:#e65100,stroke-width:2px
    
    class Customer,Admin person
    class RetailSystem system
    class SQLServer,RabbitMQ,Swagger external
```

## Container Diagram

```mermaid
graph TB
    subgraph "External Systems"
        Customer["👤 Customer<br/>End user"]
        Admin["👨‍💼 Administrator<br/>System admin"]
        SQLServer["🗄️ SQL Server<br/>Database"]
        RabbitMQ["🐰 RabbitMQ<br/>Message Broker"]
    end
    
    subgraph "Retail Microservices System"
        UI["🖥️ Retail.UI<br/>Blazor Server<br/>User interface and dashboard"]
        BFF["🌐 Retail.BFF<br/>.NET 8 Web API<br/>Backend-for-Frontend API gateway"]
        Customers["👥 Retail.Customers<br/>.NET 8 Web API<br/>Customer management service"]
        Products["📦 Retail.Products<br/>.NET 8 Web API<br/>Product catalog service"]
        OrdersWrite["✍️ Retail.Orders.Write<br/>.NET 8 Web API<br/>Order creation service"]
        OrdersRead["📖 Retail.Orders.Read<br/>.NET 8 Web API<br/>Order query service"]
        Messaging["📨 MessagingInfrastructure<br/>.NET 8 Library<br/>RabbitMQ messaging framework"]
        Common["🔧 CommonLibrary<br/>.NET 8 Library<br/>Shared utilities and contracts"]
    end
    
    Customer -->|Uses| UI
    Admin -->|Manages| UI
    UI -->|Calls| BFF
    BFF -->|Calls| Customers
    BFF -->|Calls| Products
    BFF -->|Calls| OrdersWrite
    BFF -->|Calls| OrdersRead
    
    OrdersWrite -->|Uses| Messaging
    Products -->|Uses| Messaging
    Customers -->|Uses| Messaging
    OrdersRead -->|Uses| Messaging
    Messaging -->|Publishes/Consumes| RabbitMQ
    
    Customers -->|Reads/Writes| SQLServer
    Products -->|Reads/Writes| SQLServer
    OrdersWrite -->|Reads/Writes| SQLServer
    OrdersRead -->|Reads/Writes| SQLServer
    
    classDef external fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef microservice fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    classDef infrastructure fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    
    class Customer,Admin,SQLServer,RabbitMQ external
    class UI,BFF,Customers,Products,OrdersWrite,OrdersRead microservice
    class Messaging,Common infrastructure
```

## Service Details

### Retail.Customers (7001)
**Purpose**: Customer CRUD, profile management, notifications  
**Database**: `Retail.Customer`  
**API**: `/api/v1/customer` (CRUD), `/api/v1/notification`  
**Events**: Subscribes to `InventoryUpdatedEvent`

### Retail.Products (7003)
**Purpose**: Product CRUD, inventory management  
**Database**: `Retail.Product`  
**API**: `/api/v1/product` (CRUD), `/api/v1/product/{id}/inventory`  
**Events**: Subscribes to `OrderCreatedEvent`, publishes `InventoryUpdatedEvent`, `InventoryErrorEvent`

### Retail.Orders.Write (7002)
**Purpose**: Order creation and updates (CQRS write)  
**Database**: `Retail.Order`  
**API**: `/api/v1/orderwrite` (POST, PUT, DELETE)  
**Events**: Publishes `OrderCreatedEvent`, `OrderUpdatedEvent`, `OrderCancelledEvent`

### Retail.Orders.Read (7005)
**Purpose**: Order queries (CQRS read)  
**Database**: `Retail.Order` (read model)  
**API**: `/api/v1/orderread` (GET all/by ID/by customer)  
**Events**: Subscribes to `InventoryUpdatedEvent`

### Retail.BFF (7004)
**Purpose**: API gateway aggregating data from all services  
**API**: `/api/v1/bff/orders`, `/api/v1/bff/customers`, `/api/v1/bff/products`  
**Dependencies**: HTTP calls to all microservices

## Event Flow

**Order Creation**: Orders.Write → OrderCreatedEvent → Products → InventoryUpdatedEvent → Customers/Orders.Read

**Error Handling**: Products → InventoryErrorEvent → Orders.Write → OrderCancelledEvent

See [Design Documentation](../Design/README.md) for detailed diagrams.

```mermaid
sequenceDiagram
    participant UI as 🖥️ Retail.UI
    participant BFF as 🌐 Retail.BFF
    participant OW as ✍️ Orders.Write
    participant P as 📦 Products
    participant C as 👥 Customers
    participant OR as 📖 Orders.Read
    participant RMQ as 🐰 RabbitMQ

    Note over UI,OR: Order Creation and Processing Flow
    
    UI->>+BFF: Create Order Request
    BFF->>+OW: POST /api/v1/orderwrite
    OW->>OW: Process Order
    OW->>RMQ: Publish OrderCreatedEvent
    OW-->>-BFF: Order Created Response
    BFF-->>-UI: Order Created

    Note over RMQ,OR: Asynchronous Event Processing
    
    RMQ->>+P: OrderCreatedEvent
    P->>P: Process Inventory Update
    P->>RMQ: Publish InventoryUpdatedEvent
    P-->>-RMQ: Inventory Updated

    par Customer Notification
        RMQ->>+C: InventoryUpdatedEvent
        C->>C: Process Notification
        C-->>-RMQ: Notification Processed
    and Read Model Update
        RMQ->>+OR: InventoryUpdatedEvent
        OR->>OR: Update Read Model
        OR-->>-RMQ: Read Model Updated
    end
```

## Dependencies

**External**: SQL Server, RabbitMQ, .NET 8 Runtime  
**Internal**: CommonLibrary, MessagingInfrastructure, Entity Framework Core, AutoMapper, MediatR

## Health Checks

- `/health` - Service health status
- `/swagger` - API documentation

## Configuration

**Database**: Connection strings in `appsettings.json`  
**RabbitMQ**: HostName, Port, Username, Password in `TopologyConfiguration`  
**Ports**: 7000-7005 (see service list above)

**Deployment**: See [Deployment Guide](./Deployment.md) for detailed deployment instructions.

## Troubleshooting

**Service won't start**: Check database connection, RabbitMQ status, port conflicts, config files

**Messages not processing**: Check RabbitMQ queues, verify contracts, ensure services running

**Database errors**: Verify SQL Server status, check connection strings, review connection pool

**API failures**: Check service endpoints, network connectivity, API versions

## Performance & Security

**Scaling**: Services scale independently, use load balancing  
**Optimization**: Connection pooling, async processing  
**Security**: API keys for service-to-service, JWT for users, HTTPS/TLS, input validation

## Maintenance

**Regular**: Database backups, log rotation, security updates  
**Disaster Recovery**: Service redundancy, database backups, message queue persistence
