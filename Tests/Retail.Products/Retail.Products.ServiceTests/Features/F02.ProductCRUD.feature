Feature: Product CRUD Operations
    As a retail system
    I want to perform CRUD operations on products
    So that I can manage product lifecycle

@ProductCRUD
Scenario: Add new product successfully
    Given I have a product service
    And I have a valid product to add with the following data:
        | Field      | Value           |
        | SkuName    | Test Product    |
        | Price      | 29.99           |
        | Category   | Electronics     |
        | Description| A test product  |
    When I add the new product
    Then the product should be added successfully
    And I should receive the added product with an ID

@ProductCRUD
Scenario: Update existing product successfully
    Given I have a product service
    And there is an existing product with ID "1"
    And I have updated product information:
        | Field      | Value              |
        | SkuName    | Updated Product   |
        | Price      | 39.99             |
        | Category   | Electronics       |
        | Description| An updated product|
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
    And I have invalid product data:
        | Field      | Value |
        | SkuName    |       |
        | Price      | -5.00 |
        | Category   |       |
        | Description|       |
    When I add the invalid product
    Then the operation should fail
    And an appropriate error should be thrown

@ProductValidation
Scenario: Validate product data constraints
    Given I have a product service
    When I attempt to create a product with invalid constraints
    Then the following validation rules should be enforced:
        | Field      | Constraint           | Description                    |
        | SkuName    | Required, Max 100   | Product name is mandatory     |
        | Price      | Required, Min 0     | Price must be non-negative    |
        | Category   | Required, Max 50    | Category is mandatory         |
        | Description| Optional, Max 500   | Description is optional       |

@ProductSearch
Scenario: Search products by criteria
    Given I have a product service
    And the following products exist in the system:
        | SkuId | SkuName        | Price  | Category    | Description        |
        | 1     | Laptop         | 999.99 | Electronics | High-end laptop    |
        | 2     | Mouse          | 29.99  | Electronics | Wireless mouse     |
        | 3     | Keyboard       | 79.99  | Electronics | Mechanical keyboard|
    When I search for products with criteria:
        | Field    | Value      |
        | Category | Electronics|
        | MinPrice | 50.00     |
    Then I should receive matching products
    And the results should contain laptop and keyboard
    And the results should not contain mouse

@ProductBulkOperations
Scenario: Bulk update product prices
    Given I have a product service
    And the following products exist in the system:
        | SkuId | SkuName    | CurrentPrice | NewPrice |
        | 1     | Product A  | 10.00        | 12.00    |
        | 2     | Product B  | 20.00        | 24.00    |
        | 3     | Product C  | 30.00        | 36.00    |
    When I update all product prices by 20%
    Then all products should be updated successfully
    And the new prices should match the expected values
