Feature: BFF Service Operations
    As a client application
    I want to interact with the BFF service
    So that I can retrieve aggregated order details

    Background:
        Given the BFF service is running
        And there are products available in the system
        And there are customers available in the system
        And there are orders available in the system

    Scenario: Retrieve all order details successfully
        When I request all order details
        Then I should receive a successful response
        And the response should contain order information
        And each order should have customer details
        And each order should have product details

    Scenario: Handle empty orders gracefully
        Given there are no orders in the system
        When I request all order details
        Then I should receive a successful response
        And the response should be an empty list

    Scenario: Handle missing customer gracefully
        Given an order references a non-existent customer
        When I request all order details
        Then I should receive a successful response
        And the order should show "Unknown" customer

    Scenario: Handle missing product gracefully
        Given an order references a non-existent product
        When I request all order details
        Then I should receive a successful response
        And the line item should show "Unknown" product

    Scenario: Handle service errors gracefully
        Given the order service is unavailable
        When I request all order details
        Then I should receive an error response
        And the error should indicate internal server error

    Scenario: Validate response structure
        When I request all order details
        Then the response should contain the following structure:
            | Field        | Type   | Description                    |
            | CustomerId   | Number | Customer identifier            |
            | CustomerName | String | Full customer name            |
            | OrderId      | Number | Order identifier              |
            | OrderDate    | Date   | Date when order was placed    |
            | LineItems    | Array  | List of order line items      |

    Scenario: Validate line item structure
        When I request all order details
        Then each line item should contain:
            | Field   | Type   | Description           |
            | SkuId   | Number | Product identifier    |
            | SkuName | String | Product name          |
            | Qty     | Number | Quantity ordered      |

    Scenario: Performance under load
        Given there are 100 orders in the system
        When I send multiple concurrent requests
        Then all requests should complete successfully
        And response times should be reasonable

    Scenario: Validate customer data structure
        When I request all order details
        Then each customer should contain:
            | Field        | Type   | Description                |
            | CustomerId   | Number | Unique customer identifier |
            | CustomerName | String | Full customer name        |
            | Email        | String | Customer email address    |
            | Phone        | String | Customer phone number     |

    Scenario: Validate product data structure
        When I request all order details
        Then each product should contain:
            | Field    | Type   | Description                |
            | SkuId    | Number | Unique product identifier  |
            | SkuName  | String | Product name               |
            | Price    | Number | Product unit price         |
            | Category | String | Product category           |

    Scenario: Validate order summary data
        When I request all order details
        Then each order should contain:
            | Field           | Type   | Description                    |
            | OrderId         | Number | Unique order identifier       |
            | OrderDate       | Date   | Date when order was placed    |
            | TotalAmount     | Number | Total order amount            |
            | Status          | String | Current order status          |
            | LineItemCount   | Number | Number of items in order      |
