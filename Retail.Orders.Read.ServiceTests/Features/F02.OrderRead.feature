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
