@IntegrationTest
@CustomerServiceTest
Feature: Customer Service Startup and Health Checks

As a system administrator
I want to verify that the Customer Service starts up correctly and responds to health checks
So that I can ensure the service is operational

@ServiceStartup
Scenario: Service startup and health check
    Given the Customer Service is running
    And the database connection is established
    When I request a health check
    Then the service should be running
    And the database should be accessible
    And the service should respond with a healthy status

@ServiceStartup
Scenario: Service dependency injection verification
    Given the Customer Service is running
    And the database connection is established
    When I resolve the Customer Service from the DI container
    Then the Customer Service should be properly configured
    And all dependencies should be injected

@ServiceStartup
Scenario: Service error handling with database unavailable
    Given the Customer Service is running
    And the database is unavailable
    When I attempt to perform a database operation
    Then an error should be handled gracefully

@CustomerService
Scenario: Validate customer data structure
    Given the Customer Service is running
    And the database connection is established
    When I request customer information
    Then each customer should contain the following structure:
        | Field        | Type   | Description                |
        | CustomerId   | Number | Unique customer identifier |
        | FirstName    | String | Customer first name        |
        | LastName     | String | Customer last name         |

@CustomerService
Scenario: Service configuration validation
    Given the Customer Service is running
    When I check the service configuration
    Then the following configuration should be properly set:
        | Setting           | Expected Value | Description                    |
        | DatabaseTimeout   | 30             | Database connection timeout   |
        | MaxConnections    | 100            | Maximum database connections  |
        | LogLevel          | Information    | Logging level                 |
        | EnableCaching    | true           | Enable response caching       |

@CustomerService
Scenario: Health check response structure
    Given the Customer Service is running
    When I request a health check
    Then the health check response should contain:
        | Field           | Type   | Description                    |
        | Status          | String | Service health status         |
        | Timestamp       | String | Health check timestamp        |
        | DatabaseStatus  | String | Database connection status    |
        | ServiceVersion  | String | Current service version       |
        | Uptime          | String | Service uptime duration       |
