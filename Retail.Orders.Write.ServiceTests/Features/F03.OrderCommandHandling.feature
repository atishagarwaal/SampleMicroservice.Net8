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
