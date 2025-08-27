@IntegrationTest
@OrderEventHandlingTest
Feature: Order Event Handling

As a system
I want to handle order-related events
So that I can process order updates and notifications

@OrderEventHandling
Scenario: Handle order created event
    Given an order created event is received
    When the event is processed
    Then the order should be stored in the read model
    And the order data should be accessible

@OrderEventHandling
Scenario: Handle order updated event
    Given an order updated event is received
    When the event is processed
    Then the order should be updated in the read model
    And the updated data should be reflected

@OrderEventHandling
Scenario: Handle order cancelled event
    Given an order cancelled event is received
    When the event is processed
    Then the order status should be updated
    And the cancellation should be recorded

@OrderEventHandling
Scenario: Handle line item added event
    Given a line item added event is received
    When the event is processed
    Then the line item should be added to the order
    And the order total should be recalculated

@OrderEventHandling
Scenario: Handle invalid event
    Given an invalid event is received
    When the event is processed
    Then the event should be rejected
    And an error should be logged
