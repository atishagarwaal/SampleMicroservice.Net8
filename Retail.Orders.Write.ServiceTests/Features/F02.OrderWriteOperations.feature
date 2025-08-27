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
