@IntegrationTest
@CustomerCRUDTest
Feature: Customer CRUD Operations

As a customer service user
I want to perform Create, Read, Update, and Delete operations on customer data
So that I can manage customer information effectively

@CustomerCreate
Scenario: Create a valid customer
    Given the Customer Service is running
    And the database connection is established
    And a valid customer with the following data:
        | FirstName | LastName |
        | Jane      | Smith    |
    When I create a new customer
    Then the customer should be created successfully
    And the customer should be persisted in the database

@CustomerCreate
Scenario: Create an invalid customer
    Given the Customer Service is running
    And the database connection is established
    And an invalid customer with the following data:
        | FirstName | LastName |
        |           |          |
    When I create a new customer
    Then a validation error should be returned
    And the customer should not be created

@CustomerRead
Scenario: Retrieve customer by ID
    Given the Customer Service is running
    And the database connection is established
    And a customer with ID 1 exists in the database
    When I retrieve the customer by ID
    Then the customer data should be returned

@CustomerRead
Scenario: Retrieve all customers
    Given the Customer Service is running
    And the database connection is established
    And the following customers exist in the database:
        | FirstName | LastName |
        | John      | Doe      |
        | Jane      | Smith    |
        | Bob       | Johnson  |
    When I retrieve all customers
    Then all customers should be returned
    And the customer count should match the expected count

@CustomerRead
Scenario: Retrieve non-existent customer
    Given the Customer Service is running
    And the database connection is established
    And no customer with ID 999 exists in the database
    When I retrieve the customer by ID
    Then a not found error should be returned

@CustomerUpdate
Scenario: Update existing customer
    Given the Customer Service is running
    And the database connection is established
    And a customer with ID 1 exists in the database
    When I update the customer with the following data:
        | FirstName | LastName |
        | Updated   | Name     |
    Then the customer should be updated successfully
    And the customer should be persisted in the database

@CustomerUpdate
Scenario: Update non-existent customer
    Given the Customer Service is running
    And the database connection is established
    And no customer with ID 999 exists in the database
    When I update the customer with the following data:
        | FirstName | LastName |
        | Updated   | Name     |
    Then a not found error should be returned

@CustomerDelete
Scenario: Delete existing customer
    Given the Customer Service is running
    And the database connection is established
    And a customer with ID 1 exists in the database
    When I delete the customer
    Then the customer should be deleted successfully
    And the customer should not be retrievable from the database

@CustomerDelete
Scenario: Delete non-existent customer
    Given the Customer Service is running
    And the database connection is established
    And no customer with ID 999 exists in the database
    When I delete the customer
    Then a not found error should be returned
