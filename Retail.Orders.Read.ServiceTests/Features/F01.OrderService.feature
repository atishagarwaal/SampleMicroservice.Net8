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
