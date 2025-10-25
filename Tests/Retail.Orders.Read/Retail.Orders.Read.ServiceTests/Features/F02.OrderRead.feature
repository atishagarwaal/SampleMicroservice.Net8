@IntegrationTest
@OrderReadTest
Feature: Order Read Operations

As a user
I want to read order information from the system
So that I can view order details and history

@OrderRead
Scenario: Read all orders
    Given there are orders in the system
    When I request all orders
    Then I should receive a list of orders
    And each order should have valid data

@OrderRead
Scenario: Read order by ID
    Given there is an order with ID "123"
    When I request the order with ID "123"
    Then I should receive the order details
    And the order should have the correct ID

@OrderRead
Scenario: Read orders by customer ID
    Given there are orders for customer "456"
    When I request orders for customer "456"
    Then I should receive a list of orders
    And all orders should belong to customer "456"

@OrderRead
Scenario: Read order with line items
    Given there is an order with line items
    When I request the order details
    Then I should receive the order with line items
    And the line items should have valid data

@OrderRead
Scenario: Handle non-existent order
    Given there is no order with ID "999"
    When I request the order with ID "999"
    Then I should receive a not found response

@OrderStructure
Scenario: Validate order data structure
    Given there are orders in the system
    When I request all orders
    Then each order should contain the following structure:
        | Field           | Type   | Description                    |
        | OrderId         | Number | Unique order identifier       |
        | CustomerId      | Number | Customer who placed the order |
        | OrderDate       | Date   | Date when order was placed    |
        | TotalAmount     | Number | Total order amount            |
        | Status          | String | Current order status          |
        | LineItems       | Array  | List of order line items      |

@LineItemStructure
Scenario: Validate line item data structure
    Given there are orders with line items in the system
    When I request order details
    Then each line item should contain:
        | Field      | Type   | Description                |
        | SkuId      | Number | Product identifier         |
        | SkuName    | String | Product name               |
        | Quantity   | Number | Quantity ordered           |
        | UnitPrice  | Number | Price per unit             |
        | TotalPrice | Number | Total price for line item  |

@OrderStatus
Scenario: Read orders by status
    Given there are orders with different statuses in the system:
        | OrderId | Status      | CustomerId | TotalAmount |
        | 1       | Pending     | 100        | 150.00      |
        | 2       | Processing  | 101        | 75.50       |
        | 3       | Shipped     | 102        | 200.00      |
        | 4       | Delivered   | 103        | 125.75      |
    When I request orders with status "Pending"
    Then I should receive only pending orders
    And the results should contain order ID 1
    And the results should not contain other status orders

@OrderFiltering
Scenario: Filter orders by date range
    Given there are orders in different date ranges:
        | OrderId | OrderDate  | CustomerId | TotalAmount |
        | 1       | 2024-01-15 | 100        | 150.00      |
        | 2       | 2024-02-01 | 101        | 75.50       |
        | 3       | 2024-02-15 | 102        | 200.00      |
        | 4       | 2024-03-01 | 103        | 125.75      |
    When I request orders between "2024-02-01" and "2024-02-28"
    Then I should receive orders from February 2024
    And the results should contain order IDs 2 and 3
    And the results should not contain orders from January or March

@OrderAggregation
Scenario: Get order summary statistics
    Given there are orders with various amounts in the system:
        | OrderId | CustomerId | TotalAmount | Status   |
        | 1       | 100        | 150.00      | Pending  |
        | 2       | 101        | 75.50       | Shipped  |
        | 3       | 102        | 200.00      | Delivered|
        | 4       | 103        | 125.75      | Pending  |
    When I request order summary statistics
    Then I should receive the following summary:
        | Metric        | Value |
        | TotalOrders   | 4     |
        | TotalAmount   | 551.25|
        | PendingCount  | 2     |
        | ShippedCount  | 1     |
        | DeliveredCount| 1     |
