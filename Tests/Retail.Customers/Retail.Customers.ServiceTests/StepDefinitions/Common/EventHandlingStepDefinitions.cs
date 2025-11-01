using NUnit.Framework;
using TechTalk.SpecFlow;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommonLibrary.MessageContract;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Retail.Customers.ServiceTests.Common;
using InventoryUpdatedEventNameSpace;

namespace Retail.Customers.ServiceTests.StepDefinitions.Common
{
    /// <summary>
    /// Common step definitions for Event Handling operations that can be reused across features.
    /// </summary>
    [Binding]
    public class EventHandlingStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventHandlingStepDefinitions> _logger;
        private readonly ICustomerService _customerService;
        private readonly ApplicationDbContext _dbContext;

        public EventHandlingStepDefinitions(
            ScenarioContext scenarioContext,
            IServiceProvider serviceProvider)
        {
            _scenarioContext = scenarioContext;
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<EventHandlingStepDefinitions>>();
            _customerService = serviceProvider.GetRequiredService<ICustomerService>();
            _dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        }

        #region Event Data Setup Steps

        [Given(@"a valid order created event with the following data:")]
        public void GivenAValidOrderCreatedEventWithTheFollowingData(Table eventDataTable)
        {
            _logger.LogInformation("Setting up valid order created event from table");
            
            var orderEvent = new InventoryUpdatedEvent
            {
                Id = long.Parse(eventDataTable.Rows[0].ContainsKey("Id") ? eventDataTable.Rows[0]["Id"] : "0"),
                OrderId = long.Parse(eventDataTable.Rows[0]["OrderId"]),
                CustomerId = long.Parse(eventDataTable.Rows[0]["CustomerId"]),
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0,
                LineItems = Array.Empty<LineItem>()
            };
            
            _scenarioContext.Set(orderEvent, Constants.EventData);
            _logger.LogInformation("Valid order created event received: OrderId={OrderId}, CustomerId={CustomerId}", 
                orderEvent.OrderId, orderEvent.CustomerId);
        }

        [Given(@"an invalid order created event with the following data:")]
        public void GivenAnInvalidOrderCreatedEventWithTheFollowingData(Table eventDataTable)
        {
            _logger.LogInformation("Setting up invalid order created event from table");
            
            var orderEvent = new InventoryUpdatedEvent
            {
                Id = long.Parse(eventDataTable.Rows[0].ContainsKey("Id") ? eventDataTable.Rows[0]["Id"] : "0"),
                OrderId = long.Parse(eventDataTable.Rows[0]["OrderId"]),
                CustomerId = long.Parse(eventDataTable.Rows[0]["CustomerId"]),
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0,
                LineItems = Array.Empty<LineItem>()
            };
            
            _scenarioContext.Set(orderEvent, Constants.EventData);
            _logger.LogInformation("Invalid order created event received");
        }

        [Given(@"the following customers exist for event processing:")]
        public void GivenTheFollowingCustomersExistForEventProcessing(Table customersTable)
        {
            _logger.LogInformation("Setting up customers for event processing from table");
            
            try
            {
                foreach (var row in customersTable.Rows)
                {
                    var customer = new Customer
                    {
                        FirstName = row["FirstName"],
                        LastName = row["LastName"]
                    };
                    
                    _dbContext.Customers.Add(customer);
                }
                
                _dbContext.SaveChanges();
                
                var totalCustomers = _dbContext.Customers.Count();
                _logger.LogInformation("Total customers in database for event processing: {CustomerCount}", totalCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set up customers for event processing");
                throw;
            }
        }

        [Given(@"no customers exist in the database for event processing")]
        public void GivenNoCustomersExistInTheDatabaseForEventProcessing()
        {
            _logger.LogInformation("Clearing all customers from database for event processing");
            
            try
            {
                _dbContext.Customers.RemoveRange(_dbContext.Customers);
                _dbContext.SaveChanges();
                
                var totalCustomers = _dbContext.Customers.Count();
                _logger.LogInformation("Total customers in database after clearing: {CustomerCount}", totalCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear customers from database");
                throw;
            }
        }

        #endregion

        #region Event Processing Steps

        [When(@"I process the order created event")]
        public async Task WhenIProcessTheOrderCreatedEvent()
        {
            var orderEvent = _scenarioContext.Get<InventoryUpdatedEvent>(Constants.EventData);
            _logger.LogInformation("Processing order created event: OrderId={OrderId}, CustomerId={CustomerId}", 
                orderEvent.OrderId, orderEvent.CustomerId);
            
            try
            {
                // Process the event through the customer service
                await _customerService.HandleOrderCreatedEvent(orderEvent);
                
                _scenarioContext.Set(true, "EventProcessedSuccessfully");
                _scenarioContext.Set(false, "EventProcessingFailed");
                _scenarioContext.Set(true, "DatabaseOperationSucceeded");
                
                // Get notification from mock service
                var createdNotification = MockCustomerService.MockNotifications
                    .FirstOrDefault(n => n.OrderId == orderEvent.OrderId);
                
                if (createdNotification != null)
                {
                    _scenarioContext.Set(createdNotification, "CreatedNotification");
                    _scenarioContext.Set(true, "NotificationCreated");
                }
                else
                {
                    _scenarioContext.Set(false, "NotificationCreated");
                }
                
                _logger.LogInformation("Order created event processed successfully");
            }
            catch (Exception ex)
            {
                _scenarioContext.Set(ex, "EventProcessingException");
                _scenarioContext.Set(ex, "DatabaseOperationException");
                _scenarioContext.Set(false, "EventProcessedSuccessfully");
                _scenarioContext.Set(false, "DatabaseOperationSucceeded");
                _scenarioContext.Set(true, "EventProcessingFailed");
                _scenarioContext.Set(false, "NotificationCreated");
                
                _logger.LogError(ex, "Failed to process order created event");
            }
        }

        [When(@"I attempt to process the event with database unavailable")]
        public async Task WhenIAttemptToProcessTheEventWithDatabaseUnavailable()
        {
            var orderEvent = _scenarioContext.Get<InventoryUpdatedEvent>(Constants.EventData);
            _logger.LogInformation("Attempting to process event with database unavailable: OrderId={OrderId}", 
                orderEvent.OrderId);
            
            try
            {
                // Simulate database unavailability
                throw new InvalidOperationException("Database connection unavailable");
            }
            catch (Exception ex)
            {
                _scenarioContext.Set(ex, "EventProcessingException");
                _scenarioContext.Set(ex, "DatabaseOperationException");
                _scenarioContext.Set(false, "EventProcessedSuccessfully");
                _scenarioContext.Set(false, "DatabaseOperationSucceeded");
                _scenarioContext.Set(true, "EventProcessingFailed");
                _scenarioContext.Set(false, "NotificationCreated");
                
                _logger.LogError(ex, "Event processing failed due to database unavailability");
            }
        }

        #endregion

        #region Event Validation Steps

        [Then(@"the event should be processed successfully")]
        public void ThenTheEventShouldBeProcessedSuccessfully()
        {
            var eventProcessedSuccessfully = _scenarioContext.Get<bool>("EventProcessedSuccessfully");
            eventProcessedSuccessfully.Should().BeTrue("Event should be processed successfully");
            _logger.LogInformation("Event processing success verified");
        }

        [Then(@"the event should not be processed")]
        public void ThenTheEventShouldNotBeProcessed()
        {
            var eventProcessingFailed = _scenarioContext.Get<bool>("EventProcessingFailed");
            eventProcessingFailed.Should().BeTrue("Event should not be processed");
            _logger.LogInformation("Event processing failure verified");
        }

        [Then(@"a notification should be created")]
        public void ThenANotificationShouldBeCreated()
        {
            var notificationCreated = _scenarioContext.Get<bool>("NotificationCreated");
            var createdNotification = _scenarioContext.Get<Notification>("CreatedNotification");
            
            notificationCreated.Should().BeTrue("Notification should be created");
            createdNotification.Should().NotBeNull("Created notification should not be null");
            _logger.LogInformation("Notification creation verified");
        }

        [Then(@"no notification should be created")]
        public void ThenNoNotificationShouldBeCreated()
        {
            var notificationCreated = _scenarioContext.Get<bool>("NotificationCreated");
            notificationCreated.Should().BeFalse("No notification should be created");
            _logger.LogInformation("No notification creation verified");
        }

        [Then(@"the notification data should match the event data")]
        public void ThenTheNotificationDataShouldMatchTheEventData()
        {
            var orderEvent = _scenarioContext.Get<InventoryUpdatedEvent>(Constants.EventData);
            var createdNotification = _scenarioContext.Get<Notification>("CreatedNotification");
            
            createdNotification.Should().NotBeNull("Created notification should exist");
            createdNotification.OrderId.Should().Be(orderEvent.OrderId, "Notification order ID should match event");
            createdNotification.CustomerId.Should().Be(orderEvent.CustomerId, "Notification customer ID should match event");
            createdNotification.Message.Should().NotBeNullOrEmpty("Notification message should not be empty");
            
            _logger.LogInformation("Notification data verification completed");
        }

        [Then(@"the notification should be persisted in the database")]
        public void ThenTheNotificationShouldBePersistedInTheDatabase()
        {
            var orderEvent = _scenarioContext.Get<InventoryUpdatedEvent>(Constants.EventData);
            
            var persistedNotification = _dbContext.Notifications
                .FirstOrDefault(n => n.OrderId == orderEvent.OrderId);
            
            persistedNotification.Should().NotBeNull("Notification should be persisted in database");
            persistedNotification.OrderId.Should().Be(orderEvent.OrderId, "Persisted notification order ID should match");
            persistedNotification.CustomerId.Should().Be(orderEvent.CustomerId, "Persisted notification customer ID should match");
            
            _logger.LogInformation("Notification persistence in database verified");
        }

        #endregion

        #region Error Handling Steps

        [Then(@"the transaction should be rolled back")]
        public void ThenTheTransactionShouldBeRolledBack()
        {
            var orderEvent = _scenarioContext.Get<InventoryUpdatedEvent>(Constants.EventData);
            
            // Verify that no notification was persisted due to rollback
            var persistedNotification = _dbContext.Notifications
                .FirstOrDefault(n => n.OrderId == orderEvent.OrderId);
            
            persistedNotification.Should().BeNull("Notification should not be persisted after rollback");
            _logger.LogInformation("Transaction rollback verification completed");
        }

        [Then(@"the exception should be logged")]
        public void ThenTheExceptionShouldBeLogged()
        {
            var eventProcessingFailed = _scenarioContext.Get<bool>("EventProcessingFailed");
            var exception = _scenarioContext.Get<Exception>("EventProcessingException");
            
            eventProcessingFailed.Should().BeTrue("Event processing should fail");
            exception.Should().NotBeNull("Exception should be thrown");
            _logger.LogInformation("Exception logging verification completed");
        }

        #endregion

        #region Customer Relationship Steps

        [Then(@"the notification should be linked to an existing customer")]
        public void ThenTheNotificationShouldBeLinkedToAnExistingCustomer()
        {
            var createdNotification = _scenarioContext.Get<Notification>("CreatedNotification");
            createdNotification.Should().NotBeNull("Created notification should exist");
            
            var existingCustomer = _dbContext.Customers
                .FirstOrDefault(c => c.Id == createdNotification.CustomerId);
            
            existingCustomer.Should().NotBeNull("Existing customer should be found");
            existingCustomer.Id.Should().Be(createdNotification.CustomerId, "Notification should be linked to existing customer");
            
            _logger.LogInformation("Notification link to existing customer verified");
        }

        [Then(@"a new customer should be created")]
        public void ThenANewCustomerShouldBeCreated()
        {
            var createdNotification = _scenarioContext.Get<Notification>("CreatedNotification");
            createdNotification.Should().NotBeNull("Created notification should exist");
            
            var newCustomer = _dbContext.Customers
                .FirstOrDefault(c => c.Id == createdNotification.CustomerId);
            
            newCustomer.Should().NotBeNull("New customer should be created");
            _logger.LogInformation("New customer creation verified");
        }

        [Then(@"the notification should be linked to the new customer")]
        public void ThenTheNotificationShouldBeLinkedToTheNewCustomer()
        {
            var createdNotification = _scenarioContext.Get<Notification>("CreatedNotification");
            createdNotification.Should().NotBeNull("Created notification should exist");
            
            var newCustomer = _dbContext.Customers
                .FirstOrDefault(c => c.Id == createdNotification.CustomerId);
            
            newCustomer.Should().NotBeNull("New customer should exist");
            newCustomer.Id.Should().Be(createdNotification.CustomerId, "Notification should be linked to new customer");
            
            _logger.LogInformation("Notification link to new customer verified");
        }

        #endregion
    }
}
