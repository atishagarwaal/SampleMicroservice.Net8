Feature: Product CRUD Operations
    As a retail system
    I want to perform CRUD operations on products
    So that I can manage product lifecycle

@ProductCRUD
Scenario: Add new product successfully
    Given I have a product service
    And I have a valid product to add
    When I add the new product
    Then the product should be added successfully
    And I should receive the added product with an ID

@ProductCRUD
Scenario: Update existing product successfully
    Given I have a product service
    And there is an existing product with ID "1"
    And I have updated product information
    When I update the product with ID "1"
    Then the product should be updated successfully
    And I should receive the updated product details

@ProductCRUD
Scenario: Delete existing product successfully
    Given I have a product service
    And there is an existing product with ID "1"
    When I delete the product with ID "1"
    Then the product should be deleted successfully
    And the operation should return true

@ProductCRUD
Scenario: Delete non-existent product returns false
    Given I have a product service
    And there is no product with ID "999"
    When I delete the product with ID "999"
    Then the operation should return false

@ProductCRUD
Scenario: Add product with invalid data fails
    Given I have a product service
    And I have invalid product data
    When I add the invalid product
    Then the operation should fail
    And an appropriate error should be thrown
