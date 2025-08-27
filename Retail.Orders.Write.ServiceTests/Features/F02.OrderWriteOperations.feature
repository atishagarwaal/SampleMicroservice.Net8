@IntegrationTest
@OrderWriteTest
Feature: Order Write Operations

As a user
I want to perform write operations on orders
So that I can manage order data in the system

@OrderWrite
Scenario: Create new order
    Given there is a valid order to create
    When I create the order
    Then the order should be created successfully
    And the order should have a valid ID
    And the order data should be persisted

@OrderWrite
Scenario: Update existing order
    Given there is an existing order to update
    When I update the order
    Then the order should be updated successfully
    And the updated data should be reflected
    And the order should maintain its ID

@OrderWrite
Scenario: Delete existing order
    Given there is an existing order to delete
    When I delete the order
    Then the order should be deleted successfully
    And the order should no longer exist in the system

@OrderWrite
Scenario: Create order with line items
    Given there is a valid order with line items to create
    When I create the order with line items
    Then the order should be created successfully
    And all line items should be associated with the order
    And the order total should be calculated correctly

@OrderWrite
Scenario: Handle invalid order data
    Given there is invalid order data
    When I attempt to create the order
    Then the operation should fail gracefully
    And an appropriate error should be returned
    And no order should be created

@OrderWrite
Scenario: Handle duplicate order creation
    Given there is an order with duplicate information
    When I attempt to create the duplicate order
    Then the operation should fail gracefully
    And an appropriate error should be returned

@OrderWrite
Scenario: Validate order data structure
    Given there is a valid order with the following data:
        | Field       | Value      | Type   | Description                |
        | CustomerId  | 12345      | Number | Customer identifier        |
        | OrderDate   | 2024-01-15 | Date   | Date when order placed    |
        | TotalAmount | 299.99     | Number | Total order amount         |
        | Status      | Pending    | String | Current order status       |
    When I create the order
    Then the order should be created successfully
    And the order data should match the input structure

@OrderWrite
Scenario: Validate line item data structure
    Given there is a valid order with the following line items:
        | SkuId | SkuName | Quantity | UnitPrice | TotalPrice |
        | 1001  | Laptop  | 1        | 999.99    | 999.99     |
        | 1002  | Mouse   | 2        | 29.99     | 59.98      |
        | 1003  | Keyboard| 1        | 89.99     | 89.99      |
    When I create the order with line items
    Then the order should be created successfully
    And all line items should be persisted correctly
    And the order total should be 1149.96

@OrderWrite
Scenario: Bulk update order statuses
    Given the following orders exist in the system:
        | OrderId | CustomerId | Status      | ExpectedResult |
        | 1001    | 12345      | Pending     | Success        |
        | 1002    | 12346      | Processing  | Success        |
        | 1003    | 12347      | Shipped     | Success        |
        | 1004    | 12348      | Pending     | Success        |
    When I bulk update the order statuses
    Then all status updates should be successful
    And the new statuses should match the expected values

@OrderWrite
Scenario: Validate order constraints and business rules
    Given I attempt to create an order with invalid constraints
    When I submit the order data
    Then the following validation rules should be enforced:
        | Field       | Constraint        | Description                    |
        | CustomerId  | Required, Valid   | Customer must exist in system |
        | OrderDate   | Required, Future  | Order date cannot be past     |
        | TotalAmount | Min 0.01          | Order must have positive total|
        | Status      | Enum Values       | Status must be valid enum     |
