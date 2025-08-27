Feature: Product Service Basic Operations
    As a retail system
    I want to manage products
    So that I can maintain inventory and product information

@ProductService
Scenario: Get all products successfully
    Given I have a product service
    And there are products in the system
    When I request all products
    Then I should receive a list of all products
    And the list should contain the correct number of products

@ProductService
Scenario: Get product by ID successfully
    Given I have a product service
    And there is a product with ID "1"
    When I request the product with ID "1"
    Then I should receive the product details
    And the product should have the correct ID

@ProductService
Scenario: Get product by invalid ID returns null
    Given I have a product service
    And there is no product with ID "999"
    When I request the product with ID "999"
    Then I should receive null

@ProductService
Scenario: Get all products when no products exist
    Given I have a product service
    And there are no products in the system
    When I request all products
    Then I should receive an empty list
