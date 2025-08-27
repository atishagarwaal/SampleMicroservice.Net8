@IntegrationTest
@OrderWriteTest
Feature: Order Write Service

As a user
I want the Order Write Service to be running and healthy
So that I can create, update, and delete orders

@OrderWrite
Scenario: Service startup and health check
    Given the Order Write Service is running
    And the database connection is established
    When I request a health check
    Then the service should be running
    And the database should be accessible
    And the service should respond with a healthy status

@OrderWrite
Scenario: Service dependency injection verification
    Given the Order Write Service is running
    When I resolve the Order Write Service from the DI container
    Then the Order Write Service should be properly configured
    And all dependencies should be injected

@OrderWrite
Scenario: Service error handling with database unavailable
    Given the Order Write Service is running
    And the database is unavailable
    When I attempt to perform a database operation
    Then an error should be handled gracefully

@OrderWrite
Scenario: Validate service configuration
    Given the Order Write Service is running
    When I check the service configuration
    Then the following configuration should be properly set:
        | Setting           | Expected Value | Description                    |
        | DatabaseTimeout   | 30             | Database connection timeout   |
        | MaxConnections    | 100            | Maximum database connections  |
        | LogLevel          | Information    | Logging level                 |
        | EnableCaching     | true           | Enable response caching       |
        | CommandTimeout    | 60             | Command processing timeout    |

@OrderWrite
Scenario: Health check response structure validation
    Given the Order Write Service is running
    When I request a health check
    Then the health check response should contain:
        | Field           | Type   | Description                    |
        | Status          | String | Service health status         |
        | Timestamp       | String | Health check timestamp        |
        | DatabaseStatus  | String | Database connection status    |
        | ServiceVersion  | String | Current service version       |
        | Uptime          | String | Service uptime duration       |
        | CommandQueue    | Number | Pending commands in queue     |
