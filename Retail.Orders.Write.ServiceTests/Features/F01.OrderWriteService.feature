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
