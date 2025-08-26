@IntegrationTest
@CustomerEventHandlingTest
Feature: Customer Event Handling

As a system integrator
I want to process customer-related events and create notifications
So that I can maintain data consistency across the system

@EventHandling
Scenario: Process valid order created event with existing customer
    Given the Customer Service is running
    And the database connection is established
    And a valid order created event with the following data:
        | OrderId | CustomerId | ServiceName        |
        | 12345   | 67890      | Retail.Customers   |
    And the following customers exist for event processing:
        | FirstName | LastName |
        | John      | Doe      |
        | Jane      | Smith    |
    When I process the order created event
    Then the event should be processed successfully
    And a notification should be created
    And the notification data should match the event data
    And the notification should be persisted in the database
    And the notification should be linked to an existing customer

@EventHandling
Scenario: Process valid order created event with new customer
    Given the Customer Service is running
    And the database connection is established
    And a valid order created event with the following data:
        | OrderId | CustomerId | ServiceName        |
        | 12346   | 67891      | Retail.Customers   |
    And no customers exist in the database for event processing
    When I process the order created event
    Then the event should be processed successfully
    And a notification should be created
    And the notification data should match the event data
    And the notification should be persisted in the database
    And a new customer should be created
    And the notification should be linked to the new customer

@EventHandling
Scenario: Process invalid order created event
    Given the Customer Service is running
    And the database connection is established
    And an invalid order created event with the following data:
        | OrderId | CustomerId | ServiceName |
        | 0       | -1         |             |
    When I process the order created event
    Then the event should not be processed
    And no notification should be created
    And an error should be handled gracefully

@EventHandling
Scenario: Process event with database unavailable
    Given the Customer Service is running
    And the database connection is established
    And a valid order created event with the following data:
        | OrderId | CustomerId | ServiceName        |
        | 12347   | 67892      | Retail.Customers   |
    And the database is unavailable
    When I attempt to process the event with database unavailable
    Then the event should not be processed
    And no notification should be created
    And an error should be handled gracefully
    And the transaction should be rolled back
    And the exception should be logged
