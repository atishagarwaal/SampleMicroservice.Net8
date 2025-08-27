@IntegrationTest
@OrderServiceTest
Feature: Order Service Startup and Health Checks

As a system administrator
I want to verify that the Order Service starts up correctly and responds to health checks
So that I can ensure the service is operational

@ServiceStartup
Scenario: Service startup and health check
    Given the Order Service is running
    And the database connection is established
    When I request a health check
    Then the service should be running
    And the database should be accessible
    And the service should respond with a healthy status

@ServiceStartup
Scenario: Service dependency injection verification
    Given the Order Service is running
    And the database connection is established
    When I resolve the Order Service from the DI container
    Then the Order Service should be properly configured
    And all dependencies should be injected

@ServiceStartup
Scenario: Service error handling with database unavailable
    Given the Order Service is running
    And the database is unavailable
    When I attempt to perform a database operation
    Then an error should be handled gracefully

@OrderService
Scenario: Validate order data structure
    Given the Order Service is running
    And the database connection is established
    When I request order information
    Then each order should contain the following structure:
        | Field           | Type   | Description                    |
        | OrderId         | Number | Unique order identifier       |
        | CustomerId      | Number | Customer who placed the order |
        | OrderDate       | Date   | Date when order was placed    |
        | TotalAmount     | Number | Total order amount            |
        | Status          | String | Current order status          |
        | LineItems       | Array  | List of order line items      |

@OrderService
Scenario: Service configuration validation
    Given the Order Service is running
    When I check the service configuration
    Then the following configuration should be properly set:
        | Setting           | Expected Value | Description                    |
        | DatabaseTimeout   | 30             | Database connection timeout   |
        | MaxConnections    | 100            | Maximum database connections  |
        | LogLevel          | Information    | Logging level                 |
        | EnableCaching    | true           | Enable response caching       |
        | OrderHistoryDays | 365            | Days to retain order history  |

@OrderService
Scenario: Health check response structure
    Given the Order Service is running
    When I request a health check
    Then the health check response should contain:
        | Field           | Type   | Description                    |
        | Status          | String | Service health status         |
        | Timestamp       | String | Health check timestamp        |
        | DatabaseStatus  | String | Database connection status    |
        | ServiceVersion  | String | Current service version       |
        | Uptime          | String | Service uptime duration       |
        | OrderCount      | Number | Total orders in system       |

@OrderService
Scenario: Order status enumeration validation
    Given the Order Service is running
    When I request available order statuses
    Then the service should support the following order statuses:
        | Status      | Description                    | IsActive |
        | Pending     | Order received, not processed  | true     |
        | Processing  | Order is being processed       | true     |
        | Shipped     | Order has been shipped         | true     |
        | Delivered   | Order has been delivered       | true     |
        | Cancelled   | Order has been cancelled       | false    |
        | Returned    | Order has been returned        | false    |
