Feature: Product Event Handling
    As a retail system
    I want to handle product-related events
    So that I can maintain inventory consistency

@ProductEventHandling
Scenario: Handle order created event successfully
    Given I have a product service
    And there are products with sufficient inventory
    And an order created event is received
    When I handle the order created event
    Then the inventory should be updated correctly
    And an inventory updated event should be published
    And the transaction should be committed

@ProductEventHandling
Scenario: Handle order created event with insufficient inventory
    Given I have a product service
    And there are products with insufficient inventory
    And an order created event is received
    When I handle the order created event
    Then an exception should be thrown
    And an inventory error event should be published
    And the transaction should be rolled back

@ProductEventHandling
Scenario: Handle order created event with zero inventory
    Given I have a product service
    And there are products with zero inventory
    And an order created event is received
    When I handle the order created event
    Then an exception should be thrown
    And an inventory error event should be published
    And the transaction should be rolled back

@ProductEventHandling
Scenario: Handle order created event with multiple line items
    Given I have a product service
    And there are multiple products with sufficient inventory
    And an order created event with multiple line items is received
    When I handle the order created event
    Then all product inventories should be updated correctly
    And an inventory updated event should be published
    And the transaction should be committed
