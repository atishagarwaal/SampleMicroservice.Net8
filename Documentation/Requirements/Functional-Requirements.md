# Functional Requirements

This document describes what each service must do. For how the system performs, see [Non-Functional Requirements](./Non-Functional-Requirements.md).

## Services Covered

- Retail.Customers - Customer management
- Retail.Products - Product catalog and inventory
- Retail.Orders.Write - Order creation and updates
- Retail.Orders.Read - Order queries and reports
- Retail.BFF - API gateway for frontend
- Retail.UI - Web interface

---

## Functional Requirements

### FR-001: Customer Management

**Priority**: High

**What it does**:
- Create, read, and update customer profiles
- Subscribe to inventory events and notify customers
- Validate customer data (email must be unique)

**Business Rules**:
- Email addresses must be unique
- Customer notifications are opt-in

---

### FR-002: Product Management

**Priority**: High

**What it does**:
- Manage product catalog (SKU, name, description, price)
- Track and update inventory levels
- Process orders and update inventory
- Publish inventory events

**Business Rules**:
- Inventory cannot go negative
- SKU codes must be unique
- Prices must be positive

---

### FR-003: Order Creation and Processing

**Priority**: High

**What it does**:
- Create, update, cancel, and complete orders
- Publish order events (created, updated, cancelled, completed)
- Handle inventory errors and cancel orders if needed

**Business Rules**:
- Orders must have at least one line item
- Cannot modify orders after completion
- Cancelled orders cannot be reactivated

---

### FR-004: Order Querying and Reporting

**Priority**: High

**What it does**:
- Query orders with filters (customer ID, date range)
- Get order by ID
- Maintain order history
- Keep read models synchronized with events

**Business Rules**:
- Read models may be eventually consistent (within 5 seconds)
- Results are paginated

---

### FR-005: Backend-for-Frontend (BFF) API

**Priority**: High

**What it does**:
- Aggregates data from Customers, Products, and Orders services
- Provides unified API for the frontend
- Handles service failures gracefully

**Business Rules**:
- No business logic in BFF (only aggregation)
- Must handle service timeouts

---

### FR-006: User Interface

**Priority**: Medium

**What it does**:
- Dashboard with system overview and metrics
- Customer, product, and order management interfaces
- Real-time updates

**Business Rules**:
- Must be responsive and accessible
- Clear error messages

---

## Key User Flows

1. **Customer Registration**: Customer submits form → System validates email → Profile created
2. **Order Placement**: Customer selects products → System validates inventory → Order created → Inventory updated
3. **Order Query**: Administrator queries orders → System returns paginated results with filters
4. **Inventory Update**: Order created → Products service processes → Inventory updated → Events published

## Data Models

**Customer**: ID (UUID), Name, Email (unique), Address, Dates  
**Product**: SKU (unique), Name, Description, Price, Inventory Quantity, Dates  
**Order**: ID (UUID), Customer ID, Date, Total, Status, Line Items, Dates

## Integration Points

- **RabbitMQ**: Event messaging between services
- **SQL Server**: Data persistence
- **HTTP**: Service-to-service communication (via BFF)

