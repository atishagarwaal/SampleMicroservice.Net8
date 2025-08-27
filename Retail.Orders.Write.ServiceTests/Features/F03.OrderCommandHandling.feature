@IntegrationTest
@OrderWriteTest
Feature: Order Command Handling

As a user
I want the system to handle order commands properly
So that orders can be managed through the command pattern

@OrderWrite
Scenario: Handle CreateOrderCommand successfully
    Given a valid CreateOrderCommand is received
    When the command is processed
    Then the order should be created in the database
    And an OrderCreatedEvent should be published
    And the transaction should be committed

@OrderWrite
Scenario: Handle UpdateOrderCommand successfully
    Given a valid UpdateOrderCommand is received
    When the command is processed
    Then the order should be updated in the database
    And an OrderUpdatedEvent should be published
    And the transaction should be committed

@OrderWrite
Scenario: Handle DeleteOrderCommand successfully
    Given a valid DeleteOrderCommand is received
    When the command is processed
    Then the order should be deleted from the database
    And an OrderDeletedEvent should be published
    And the transaction should be committed

@OrderWrite
Scenario: Handle command with database transaction failure
    Given a valid command is received
    And the database transaction fails
    When the command is processed
                    Then the transaction should be rolled back
                And no event should be published
                And an appropriate error should be returned for command

@OrderWrite
Scenario: Handle command with message publishing failure
    Given a valid command is received
    And the message publishing fails
    When the command is processed
                    Then the transaction should be rolled back
                And no order changes should be persisted
                And an appropriate error should be returned for command

@OrderWrite
Scenario: Handle command with invalid data
    Given an invalid command is received
    When the command is processed
                    Then the command operation should fail gracefully
                And no database changes should occur
                And no events should be published

@OrderWrite
Scenario: Validate CreateOrderCommand data structure
    Given a CreateOrderCommand with the following data:
        | Field       | Value      | Type   | Description                |
        | CommandId   | cmd-001    | String | Unique command identifier  |
        | CustomerId  | 12345      | Number | Customer identifier        |
        | OrderDate   | 2024-01-15 | Date   | Date when order placed    |
        | LineItems   | 3          | Number | Number of line items      |
        | TotalAmount | 299.99     | Number | Total order amount         |
    When the command is processed
    Then the command should be validated successfully
    And the order should be created in the database

@OrderWrite
Scenario: Validate command event publishing structure
    Given a valid command is processed successfully
    When the corresponding event is published
    Then the event should contain the following structure:
        | Field           | Type   | Description                    |
        | EventId         | String | Unique event identifier       |
        | EventType       | String | Type of event published       |
        | AggregateId     | Number | Order identifier              |
        | Timestamp       | String | Event occurrence time         |
        | Version         | Number | Event version number          |
        | Data            | Object | Event payload data            |

@OrderWrite
Scenario: Handle multiple commands in sequence
    Given the following commands are received in sequence:
        | CommandType        | OrderId | CustomerId | Status    | ExpectedResult |
        | CreateOrderCommand | 1001    | 12345      | Pending   | Success        |
        | UpdateOrderCommand | 1001    | 12345      | Processing| Success        |
        | UpdateOrderCommand | 1001    | 12345      | Shipped   | Success        |
        | UpdateOrderCommand | 1001    | 12345      | Delivered | Success        |
    When all commands are processed
    Then all commands should be processed successfully
    And the order should progress through all statuses
    And corresponding events should be published for each command

@OrderWrite
Scenario: Validate transaction rollback scenarios
    Given a command processing scenario with the following failures:
        | FailureType        | Description                    | ExpectedBehavior        |
        | DatabaseConnection | Database connection lost       | Rollback, No Event     |
        | ValidationError    | Command data validation fails  | Rollback, No Event     |
        | EventPublishError  | Event publishing fails         | Rollback, No Persist   |
        | ConcurrentAccess   | Order modified by another user| Rollback, Retry Logic  |
    When the failure occurs during command processing
    Then the transaction should be rolled back appropriately
    And the system should handle the failure gracefully
