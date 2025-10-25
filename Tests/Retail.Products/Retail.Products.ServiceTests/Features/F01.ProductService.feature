Feature: Product Service Basic Operations
    As a retail system
    I want to manage products
    So that I can maintain inventory and product information

@ProductService
Scenario: Get all products successfully
    Given I have a product service
    And there are products in the system:
        | SkuId | SkuName        | Price  | Category    | Description        |
        | 1     | Laptop         | 999.99 | Electronics | High-end laptop    |
        | 2     | Mouse          | 29.99  | Electronics | Wireless mouse     |
        | 3     | Keyboard       | 79.99  | Electronics | Mechanical keyboard|
    When I request all products
    Then I should receive a list of all products
    And the list should contain the correct number of products

@ProductService
Scenario: Get product by ID successfully
    Given I have a product service
    And there is a product with ID "1" with the following data:
        | Field      | Value           |
        | SkuId      | 1               |
        | SkuName    | Laptop          |
        | Price      | 999.99          |
        | Category   | Electronics     |
        | Description| High-end laptop |
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

@ProductService
Scenario: Validate product data structure
    Given I have a product service
    And there are products in the system
    When I request all products
    Then each product should contain the following structure:
        | Field      | Type   | Description                |
        | SkuId      | Number | Unique product identifier  |
        | SkuName    | String | Product name               |
        | Price      | Number | Product unit price         |
        | Category   | String | Product category           |
        | Description| String | Product description        |

@ProductService
Scenario: Get products by category
    Given I have a product service
    And the following products exist in the system:
        | SkuId | SkuName        | Price  | Category    | Description        |
        | 1     | Laptop         | 999.99 | Electronics | High-end laptop    |
        | 2     | Mouse          | 29.99  | Electronics | Wireless mouse     |
        | 3     | T-Shirt        | 19.99  | Clothing    | Cotton t-shirt     |
        | 4     | Jeans          | 49.99  | Clothing    | Blue jeans         |
    When I request products in category "Electronics"
    Then I should receive only electronics products
    And the results should contain laptop and mouse
    And the results should not contain clothing items
