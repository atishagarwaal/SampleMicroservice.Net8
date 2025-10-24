# AsyncAPI Contract Specifications

## Overview

This document provides detailed information about the AsyncAPI contract specifications for each microservice in the retail system. These contracts define the messaging interfaces and ensure type-safe communication between services.

## Contract Files

### 1. Retail-Customers-AsyncAPI-v1.0.0.yaml

**Service**: Retail.Customers  
**Purpose**: Customer data management and inventory event subscription  
**Version**: 1.0.0  
**AsyncAPI Version**: 2.0.0  

#### Channels
- `inventorydomain.topic.exchange/inventory.updated.event` (subscribe)

#### Messages
- **InventoryUpdatedEvent**: Received when inventory changes affect customer orders

#### Schema
```yaml
InventoryUpdatedEvent:
  payload:
    type: object
    required:
      - customerId
      - orderId
      - orderDate
      - totalAmount
      - lineItems
    properties:
      customerId:
        type: string
        format: uuid
        description: Unique identifier of the customer who placed the order
      orderId:
        type: string
        format: uuid
        description: Unique identifier of the order
      orderDate:
        type: string
        format: date-time
        description: Date and time when the order was created
      totalAmount:
        type: number
        description: Total amount of the order
        minimum: 0
      lineItems:
        type: array
        description: List of items in the order
        items:
          $ref: "#/components/schemas/LineItem"
        minItems: 1

LineItem:
  type: object
  required:
    - skuId
    - qty
    - unitPrice
    - totalPrice
  properties:
    skuId:
      type: string
      format: uuid
      description: Unique identifier of the product SKU
    qty:
      type: integer
      description: Quantity of the product ordered
      minimum: 1
    unitPrice:
      type: number
      description: Price per unit of the product
      minimum: 0
    totalPrice:
      type: number
      description: Total price for this line item (qty * unitPrice)
      minimum: 0
```

#### Generated C# Models
- `InventoryUpdatedEvent`
- `LineItem`

---

### 2. Retail-Orders-Read-AsyncAPI-v1.0.0.yaml

**Service**: Retail.Orders.Read  
**Purpose**: Order read model updates and querying  
**Version**: 1.0.0  
**AsyncAPI Version**: 2.0.0  

#### Channels
- `inventorydomain.topic.exchange/inventory.updated.event` (subscribe)

#### Messages
- **InventoryUpdatedEvent**: Received to update order read models

#### Schema
```yaml
InventoryUpdatedEvent:
  payload:
    type: object
    required:
      - customerId
      - orderId
      - orderDate
      - totalAmount
      - lineItems
    properties:
      customerId:
        type: string
        format: uuid
        description: Unique identifier of the customer who placed the order
      orderId:
        type: string
        format: uuid
        description: Unique identifier of the order
      orderDate:
        type: string
        format: date-time
        description: Date and time when the order was created
      totalAmount:
        type: number
        description: Total amount of the order
        minimum: 0
      lineItems:
        type: array
        description: List of items in the order
        items:
          $ref: "#/components/schemas/LineItem"
        minItems: 1

LineItem:
  type: object
  required:
    - skuId
    - qty
    - unitPrice
    - totalPrice
  properties:
    skuId:
      type: string
      format: uuid
      description: Unique identifier of the product SKU
    qty:
      type: integer
      description: Quantity of the product
      minimum: 1
    unitPrice:
      type: number
      description: Price per unit of the product
      minimum: 0
    totalPrice:
      type: number
      description: Total price for this line item (qty * unitPrice)
      minimum: 0
```

#### Generated C# Models
- `InventoryUpdatedEvent`
- `LineItem`

---

### 3. Retail-Orders-Write-AsyncAPI-v1.0.0.yaml

**Service**: Retail.Orders.Write  
**Purpose**: Order creation and lifecycle management  
**Version**: 1.0.0  
**AsyncAPI Version**: 2.0.0  

#### Channels
- `orderdomain.topic.exchange/order.created.event` (publish)
- `orderdomain.topic.exchange/order.updated.event` (publish)
- `orderdomain.topic.exchange/order.cancelled.event` (publish)
- `orderdomain.topic.exchange/order.completed.event` (publish)

#### Messages
- **OrderCreatedEvent**: Published when a new order is created
- **OrderUpdatedEvent**: Published when an order is modified
- **OrderCancelledEvent**: Published when an order is cancelled
- **OrderCompletedEvent**: Published when an order is completed

#### Schema
```yaml
OrderCreatedEvent:
  payload:
    type: object
    required:
      - eventId
      - eventType
      - timestamp
      - orderId
      - customerId
      - orderItems
      - totalAmount
    properties:
      eventId:
        type: string
        format: uuid
        description: Unique identifier for the event
      eventType:
        type: string
        enum: [OrderCreated]
        description: Type of the event
      timestamp:
        type: string
        format: date-time
        description: When the event occurred
      orderId:
        type: string
        format: uuid
        description: Unique identifier for the order
      customerId:
        type: string
        format: uuid
        description: ID of the customer who placed the order
      orderItems:
        type: array
        items:
          $ref: "#/components/schemas/OrderItem"
        description: Items in the order
        minItems: 1
      totalAmount:
        type: number
        description: Total amount of the order
        minimum: 0
      shippingAddress:
        $ref: "#/components/schemas/Address"

OrderUpdatedEvent:
  payload:
    type: object
    required:
      - eventId
      - eventType
      - timestamp
      - orderId
      - changes
    properties:
      eventId:
        type: string
        format: uuid
        description: Unique identifier for the event
      eventType:
        type: string
        enum: [OrderUpdated]
        description: Type of the event
      timestamp:
        type: string
        format: date-time
        description: When the event occurred
      orderId:
        type: string
        format: uuid
        description: Unique identifier for the order
      changes:
        type: object
        description: Fields that were changed
        additionalProperties: true

OrderCancelledEvent:
  payload:
    type: object
    required:
      - eventId
      - eventType
      - timestamp
      - orderId
      - reason
    properties:
      eventId:
        type: string
        format: uuid
        description: Unique identifier for the event
      eventType:
        type: string
        enum: [OrderCancelled]
        description: Type of the event
      timestamp:
        type: string
        format: date-time
        description: When the event occurred
      orderId:
        type: string
        format: uuid
        description: Unique identifier for the order
      reason:
        type: string
        description: Reason for cancellation

OrderCompletedEvent:
  payload:
    type: object
    required:
      - eventId
      - eventType
      - timestamp
      - orderId
      - completedAt
    properties:
      eventId:
        type: string
        format: uuid
        description: Unique identifier for the event
      eventType:
        type: string
        enum: [OrderCompleted]
        description: Type of the event
      timestamp:
        type: string
        format: date-time
        description: When the event occurred
      orderId:
        type: string
        format: uuid
        description: Unique identifier for the order
      completedAt:
        type: string
        format: date-time
        description: When the order was completed

OrderItem:
  type: object
  required:
    - productId
    - quantity
    - unitPrice
  properties:
    productId:
      type: string
      format: uuid
      description: Unique identifier for the product
    quantity:
      type: integer
      description: Quantity of the product
      minimum: 1
    unitPrice:
      type: number
      description: Price per unit
      minimum: 0
    totalPrice:
      type: number
      description: Total price for this line item
      minimum: 0

Address:
  type: object
  properties:
    street:
      type: string
      description: Street address
    city:
      type: string
      description: City
    state:
      type: string
      description: State or province
    zipCode:
      type: string
      description: ZIP or postal code
    country:
      type: string
      description: Country
```

#### Generated C# Models
- `OrderCreatedEvent`
- `OrderUpdatedEvent`
- `OrderCancelledEvent`
- `OrderCompletedEvent`
- `OrderItem`
- `Address`

---

### 4. Retail-Products-AsyncAPI-v1.0.0.yaml

**Service**: Retail.Products  
**Purpose**: Product inventory and order processing  
**Version**: 1.0.0  
**AsyncAPI Version**: 2.0.0  

#### Channels
- `orderdomain.topic.exchange/order.created.event` (subscribe)
- `inventorydomain.topic.exchange/inventory.updated.event` (publish)
- `rollback.topic.exchange/inventory.error.event` (publish)

#### Messages
- **OrderCreatedEvent**: Received to process new orders
- **InventoryUpdatedEvent**: Published when inventory is updated
- **InventoryErrorEvent**: Published when inventory processing fails

#### Schema
```yaml
OrderCreatedEvent:
  payload:
    type: object
    required:
      - customerId
      - orderId
      - orderDate
      - totalAmount
      - lineItems
    properties:
      customerId:
        type: string
        format: uuid
        description: Unique identifier of the customer who placed the order
      orderId:
        type: string
        format: uuid
        description: Unique identifier of the order
      orderDate:
        type: string
        format: date-time
        description: Date and time when the order was created
      totalAmount:
        type: number
        description: Total amount of the order
        minimum: 0
      lineItems:
        type: array
        description: List of items in the order
        items:
          $ref: "#/components/schemas/LineItem"
        minItems: 1

InventoryUpdatedEvent:
  payload:
    type: object
    required:
      - customerId
      - orderId
      - orderDate
      - totalAmount
      - lineItems
    properties:
      customerId:
        type: string
        format: uuid
        description: Unique identifier of the customer who placed the order
      orderId:
        type: string
        format: uuid
        description: Unique identifier of the order
      orderDate:
        type: string
        format: date-time
        description: Date and time when the order was created
      totalAmount:
        type: number
        description: Total amount of the order
        minimum: 0
      lineItems:
        type: array
        description: List of items in the order
        items:
          $ref: "#/components/schemas/LineItem"
        minItems: 1

InventoryErrorEvent:
  payload:
    type: object
    required:
      - customerId
      - orderId
      - orderDate
      - totalAmount
    properties:
      customerId:
        type: string
        format: uuid
        description: Unique identifier of the customer who placed the order
      orderId:
        type: string
        format: uuid
        description: Unique identifier of the order
      orderDate:
        type: string
        format: date-time
        description: Date and time when the order was created
      totalAmount:
        type: number
        description: Total amount of the order
        minimum: 0
      errorMessage:
        type: string
        description: Description of the error that occurred during inventory processing

LineItem:
  type: object
  required:
    - skuId
    - qty
    - unitPrice
    - totalPrice
  properties:
    skuId:
      type: string
      format: uuid
      description: Unique identifier of the product SKU
    qty:
      type: integer
      description: Quantity of the product ordered
      minimum: 1
    unitPrice:
      type: number
      description: Price per unit of the product
      minimum: 0
    totalPrice:
      type: number
      description: Total price for this line item (qty * unitPrice)
      minimum: 0
```

#### Generated C# Models
- `OrderCreatedEvent`
- `InventoryUpdatedEvent`
- `InventoryErrorEvent`
- `LineItem`

## Message Flow Patterns

### 1. Order Creation Flow
```
Orders.Write → OrderCreatedEvent → Products → InventoryUpdatedEvent → Customers/Orders.Read
```

### 2. Error Handling Flow
```
Products → InventoryErrorEvent → Orders.Write → OrderCancelledEvent → Customers
```

### 3. Order Update Flow
```
Orders.Write → OrderUpdatedEvent → Orders.Read
```

## Contract Versioning

### Versioning Strategy
- **Semantic Versioning**: Major.Minor.Patch
- **Backward Compatibility**: New fields are optional
- **Breaking Changes**: Documented and coordinated
- **Migration Path**: Clear upgrade instructions

### Version Compatibility Matrix

| Service | Contract Version | Compatible With |
|---------|------------------|-----------------|
| Customers | 1.0.0 | Orders.Write 1.0.0, Products 1.0.0 |
| Orders.Read | 1.0.0 | Orders.Write 1.0.0, Products 1.0.0 |
| Orders.Write | 1.0.0 | Products 1.0.0, Customers 1.0.0 |
| Products | 1.0.0 | Orders.Write 1.0.0, Customers 1.0.0 |

## Validation and Testing

### Contract Validation
- **AsyncAPI Studio**: Online validation
- **CLI Validation**: `npx @asyncapi/cli validate`
- **Schema Validation**: JSON Schema compliance
- **Message Validation**: Runtime message validation

### Testing Strategy
- **Unit Tests**: Generated model validation
- **Integration Tests**: End-to-end message flow
- **Contract Tests**: Service compatibility
- **Performance Tests**: Message throughput

## Best Practices

### 1. Contract Design
- Use descriptive field names
- Include comprehensive descriptions
- Define clear required fields
- Use appropriate data types

### 2. Version Management
- Increment versions for breaking changes
- Maintain backward compatibility
- Document migration paths
- Coordinate releases across services

### 3. Error Handling
- Include error events in contracts
- Use consistent error structures
- Implement proper retry logic
- Provide clear error messages

### 4. Documentation
- Keep contracts as single source of truth
- Update documentation with changes
- Use clear examples
- Maintain version history
